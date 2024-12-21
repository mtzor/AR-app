using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Threading.Tasks;

public class CustomizeDwellingSelector : NetworkBehaviour
{
    private static CustomizeDwellingSelector _instance;
    private List<ModuleData> _savedModules;

    [SerializeField] private Transform[] moduleBtnPrefabs; // Array of module prefabs
    [SerializeField] private GameObject floorPrefab; // Prefab for the "floor" box

    private string _selectedModule;

    private List<GameObject> _spawnedItems;
    void Start()
    {
        _savedModules = new List<ModuleData>();
        _spawnedItems = new List<GameObject>();
    }

    public override void OnNetworkSpawn()
    {
        LoadSavedModulesServerRPC();

    }

    public static CustomizeDwellingSelector Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CustomizeDwellingSelector>();
                if (_instance == null)
                {
                    GameObject customizeDwellingSelectorObject = new GameObject("CustomizeDwellingSelector");
                    _instance = customizeDwellingSelectorObject.AddComponent<CustomizeDwellingSelector>();
                }
            }
            return _instance;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void LoadSavedModulesServerRPC()
    {
        if (!IsServer)
        {
            return;
        }

        // Call loading saved Modules on clients
        LoadSavedModules();
    }

    private void LoadSavedModules()
    {
        // Load saved modules data
        _savedModules = SaveSystem.LoadAllModules();

        if (_savedModules != null)
        {
            List<Transform> spawnedModules = new List<Transform>();
            float lowestY = float.MaxValue; // Start with a high value to find the lowest Y

            // Spawn modules
            foreach (ModuleData moduleData in _savedModules)
            {
                // Check if the moduleID is valid and if a prefab exists for this moduleID
                if (moduleData.moduleID - 1 >= 0 && moduleData.moduleID - 1 < moduleBtnPrefabs.Length)
                {
                    // Instantiate the prefab at the specified position and rotation
                    Transform modulePrefab = moduleBtnPrefabs[moduleData.moduleID - 1];
                    Vector3 position = new Vector3(moduleData.positionX, moduleData.positionY, moduleData.positionZ);
                    Quaternion rotation = Quaternion.Euler(moduleData.rotationX, moduleData.rotationY, moduleData.rotationZ);

                    Transform spawnedModule = Instantiate(modulePrefab, position, rotation);

                    // Add the spawned module to the list
                    spawnedModules.Add(spawnedModule);
                    _spawnedItems.Add(spawnedModule.gameObject);

                    // Track the lowest Y position
                    if (position.y < lowestY)
                    {
                        lowestY = position.y;
                    }

                    // If you want it to be a networked object, use NetworkObject.Spawn
                    NetworkObject netObject = spawnedModule.GetComponent<NetworkObject>();
                    if (netObject != null)
                    {
                        netObject.Spawn(true); // Spawn it across the network
                    }

                }
                else
                {
                    Debug.LogWarning("Invalid module ID: " + moduleData.moduleID);
                }
            }

            // Find all modules on the bottom floor and calculate the bounds for the floor
            Bounds bottomFloorBounds = new Bounds();
            bool boundsInitialized = false;

            foreach (Transform module in spawnedModules)
            {
                if (Mathf.Approximately(module.position.y, lowestY))
                {
                    Collider moduleCollider = module.GetComponent<Collider>();
                    if (moduleCollider != null)
                    {
                        if (!boundsInitialized)
                        {
                            bottomFloorBounds = moduleCollider.bounds;
                            boundsInitialized = true;
                        }
                        else
                        {
                            bottomFloorBounds.Encapsulate(moduleCollider.bounds);
                        }
                    }
                }
            }

            // Spawn the floor in the middle of the bottom floor modules
            if (boundsInitialized && floorPrefab != null)
            {
                Vector3 floorPosition = new Vector3(
                    bottomFloorBounds.center.x,
                    bottomFloorBounds.min.y, // Place the floor at the lowest Y of the bounds
                    bottomFloorBounds.center.z
                );

                Transform spawnedGround = Instantiate(floorPrefab, floorPosition, Quaternion.identity).transform;

                // If you want it to be a networked object, use NetworkObject.Spawn
                NetworkObject netObject = spawnedGround.GetComponent<NetworkObject>();
                if (netObject != null)
                {
                    netObject.Spawn(true); // Spawn it across the network
                }

                _spawnedItems.Add(spawnedGround.gameObject);
            }     
            
        }

    }

    [ServerRpc(RequireOwnership = false)]
    public  void SelectModuleDialogServerRPC(string selectedModule )
    {
        if (!IsServer)
        {
            return;
        }
        // Call loading saved Modules on clients

        SelectModuleDialogClientRPC(selectedModule);

        selectModuleDialog(selectedModule);
    }

    public async Task selectModuleDialog(string selectedModule)
    {
        // Extract the name before "(Clone)" using string manipulation
        string moduleName = selectedModule;
        int cloneIndex = selectedModule.IndexOf("(Clone)");
        if (cloneIndex > 0)
        {
            moduleName = selectedModule.Substring(0, cloneIndex).Trim(); // Extract name before "(Clone)" and trim any extra spaces
        }

        DialogButtonType answer = await DialogManager.Instance.SpawnDialogWithAsync("Module" + selectedModule + " selected!", "Would you like to confirm your choice ?", "YES", "NO");

        if (answer == DialogButtonType.Positive)
        {
            _selectedModule = moduleName;

            SetSelectedModuleClientRPC(moduleName);

            DespawnItemsClientRPC();

            SetNextPhaseClientRPC();

            SetupCustomizeUIServerRPC();

            SetupCustomizeUIClientRPC();

        }
    }

    [ClientRpc(RequireOwnership = false)]
    public void SelectModuleDialogClientRPC(string selectedModule)
    {
        if (IsServer)
        {
            return;
        }
        // Call loading saved Modules on clients

        selectModuleNeutralDialog(selectedModule);
    }

    public async Task selectModuleNeutralDialog(string selectedModule)
    {
        DialogButtonType answer = await DialogManager.Instance.SpawnDialogWithAsync("Module" + selectedModule + " selected!", "Waiting for host to confirm the selection ", "OK");    
    }


    [ClientRpc(RequireOwnership = false)]
    public void SetSelectedModuleClientRPC(string moduleName)
    {
        Debug.Log("SETIING SELECTE MODULE ON CLIENT" + NetworkManager.LocalClientId);
        CustomizeManager.Instance.SelectedModule = moduleName;

    }

    [ClientRpc(RequireOwnership = false)]
    public void SetNextPhaseClientRPC()
    {
        Debug.Log("SetNextphase UIMANAGER RESPOND ?");
        AppManager.Instance.setNextPhase();
    }


    [ServerRpc(RequireOwnership = false)]
    public void SetupCustomizeUIServerRPC()
    {
        Debug.Log("SetupCustomizeUIServerRPC Local CLIENT ID " + (NetworkManager.LocalClientId));
        CustomizeManager.Instance.SetupServerLayoutInterfaces();
    }

    [ClientRpc(RequireOwnership = false)]
    public void SetupCustomizeUIClientRPC()
    {
        if (IsServer) {return; }   

        Debug.Log("SetupCustomizeUIClientRPC Local CLIENT ID " + (NetworkManager.LocalClientId));
        CustomizeManager.Instance.SetupClientLayoutInterfaces();
    }

    [ClientRpc(RequireOwnership = false)]
    public void DespawnItemsClientRPC()
    {
        foreach (var item in _spawnedItems) { 
        
        item.GetComponent<NetworkObject>().Despawn(true);
        }

        HandManager.Instance.DespawnAndDestroyAllHandsServerRpc();
    }
}
