using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DesignNetworkSyncScript : NetworkBehaviour
{

    private static DesignNetworkSyncScript _instance;

    [SerializeField] Transform buildingTransform;

    [SerializeField] private GameObject NextPhaseButton;
    [SerializeField] private GameObject manipulationBar;
    [SerializeField] private GameObject[] floors=new GameObject[4];
    [SerializeField] private GameObject buildingHollow;
    [SerializeField] private PressableButton NextFloorBtn;

    [SerializeField] public NetworkVariable<int> floorNo = new NetworkVariable<int>();
    [SerializeField] public NetworkVariable<bool> floorBtnActiveNet = new NetworkVariable<bool>();

    [SerializeField] private GameObject Design_P2_interface;

    private int currentFloor = 0;

    private bool floorBtnActive = false;

    // Start is called before the first frame update
    public static DesignNetworkSyncScript Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<DesignNetworkSyncScript>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject designNetworkSyncScriptManagerObject = new GameObject("DesignNetworkSyncScript");
                    _instance = designNetworkSyncScriptManagerObject.AddComponent<DesignNetworkSyncScript>();
                }
            }

            return _instance;
        }
    }
    public override void OnNetworkSpawn()
    {
        floorNo.OnValueChanged += OnFloorUpdate;
        //floorBtnActiveNet.OnValueChanged += OnFloorBtnUpdate;
    }

    public void OnFloorUpdate(int previousValue, int newValue)
    {
        currentFloor = newValue;
        Debug.Log("Current Floor No changed from :" + previousValue + " to :" + newValue);

        float yval = floorNo.Value * 0.03f;

        Transform previousModulePos = ModuleSpawner.Instance.ModuleSpawnPos;

        ModuleSpawner.Instance.ModuleSpawnPos.transform.position = previousModulePos.transform.position + new Vector3(0, yval, 0);

        //deactivate previous floor gameobject
        floors[currentFloor - 1].SetActive(false);

        //activate current floor gameobject
        floors[currentFloor].SetActive(true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddFloorServerRpc()
    {
        currentFloor++;//increasing floor number

        floorNo.Value = currentFloor;//updating floor number network variable
    }

    [ServerRpc(RequireOwnership = false)]
    public void DeactivateNextFloorBtnServerRpc()
    {
        floorBtnActive=false;//increasing floor number

        floorBtnActiveNet.Value = floorBtnActive;//updating floor number network variable

        NextFloorBtn.gameObject.SetActive(floorBtnActive);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ActivateNextFloorBtnServerRpc()
    {
        floorBtnActive = true;//increasing floor number

        floorBtnActiveNet.Value = floorBtnActive;//updating floor number network variable

        NextFloorBtn.gameObject.SetActive(floorBtnActive);
    }

    public void OnNextFloorBtnPressed() {

        //deactivate next floor button
        DeactivateNextFloorBtnServerRpc();

        //setting cocvered area to 0
        DesignManager.Instance.ResetAreaCovered();

        //add next floor 
        AddFloorServerRpc();
    }

    // Update is called once per frame
    void Update()
    {
        if (AppManager.Instance.CurrentPhase() != AppManager.AppPhase.Design_P2)
        {
            if (DesignNetworkSyncScript.Instance.Design_P2_interface.gameObject.activeSelf == true)
            {
                Debug.Log("Design_P2_interface is active. Deactivating");
                DesignNetworkSyncScript.Instance.Design_P2_interface.gameObject.SetActive(false);
            }
        }
    }
   
    [ClientRpc(RequireOwnership =false)]
    public void SetNextPhaseClientRPC()
    {
        AppManager.Instance.setNextPhase();
        DesignNetworkSyncScript.Instance.NextPhaseButton.SetActive(false);
        DesignNetworkSyncScript.Instance.manipulationBar.SetActive(false);
        DesignNetworkSyncScript.Instance.floors[currentFloor].SetActive(true);
        DesignNetworkSyncScript.Instance.buildingHollow.SetActive(false);

        DesignNetworkSyncScript.Instance.Design_P2_interface.SetActive(true);
        Debug.Log("SetNextPhaseClientRPC called.");
    }

    public void DisableP2Components()
    {
        DesignNetworkSyncScript.Instance.Design_P2_interface.SetActive(false);
        Debug.Log("DisableP2Components called."+ NetworkObjectId);
    }


    
}
