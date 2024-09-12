using Unity.Netcode;
using UnityEngine;

public class ModuleSpawner : NetworkBehaviour
{ 

    private static ModuleSpawner _instance;

    [SerializeField] private Transform[] modulePrefabs; // Prefab registered in NetworkManager
    [SerializeField] private Transform moduleContainer; // Parent container for spawned objects

    private Transform _moduleSpawnPos;

    public Transform ModuleSpawnPos
    {
        get => _moduleSpawnPos;
        set => _moduleSpawnPos = value;
    }

    public static ModuleSpawner Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<ModuleSpawner>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject moduleSpawnObject = new GameObject("ModuleSpawner");
                    _instance =moduleSpawnObject.AddComponent<ModuleSpawner>();
                }
            }

            return _instance;
        }
    }
    // Method called by clients to request the server to spawn a module
    public void OnSpawnButtonPressed(int moduleIndex)
    {
        if (IsClient)
        {
            // Call the ServerRpc to ask the server to spawn the module
            RequestSpawnModuleServerRpc(moduleIndex, NetworkManager.Singleton.LocalClientId);
        }
    }

    // ServerRpc method to spawn the module on the server
    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnModuleServerRpc(int moduleIndex, ulong clientId)
    {
        Debug.Log("Server spawning module: " + moduleIndex + " for client: " + clientId);

        float yval = DesignNetworkSyncScript.Instance.floorNo.Value * 0.03f;

        Debug.Log("Spawning position: " + yval+ "Floor No:"+DesignNetworkSyncScript.Instance.floorNo);

        moduleContainer.transform.position += new Vector3(0,yval, 0);
        // Instantiate the module prefab
        Transform moduleInstance = Instantiate(modulePrefabs[moduleIndex], moduleContainer);

        // Spawn the object across the network
        moduleInstance.GetComponent<NetworkObject>().Spawn(true);

        // Optionally transfer ownership to the client who requested the spawn
        //Debug.Log("Owner Client ID: " +moduleInstance.GetComponent<NetworkObject>().OwnerClientId);

        if (moduleInstance.GetComponent<NetworkObject>().OwnerClientId != clientId)
        {
            moduleInstance.GetComponent<NetworkObject>().ChangeOwnership(clientId);
        }
    }
}
