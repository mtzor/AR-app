using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

public class DesignManager : MonoBehaviour
{
    private static DesignManager _instance;
    [SerializeField] private Transform buildingPrefab;
    [SerializeField] private Transform container;

    public delegate void AreaUpdated(int area);
    public static event AreaUpdated OnAreaUpdatedEvent;

    public static int TOTAL_AREA = 240;
    public static int MAX_FLOORS = 3;

    private Transform _building;
    private int coveredArea = 0;
    private int floorCount = 0;

    // Singleton pattern
    public static DesignManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<DesignManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach DesignManager if not found
                    GameObject designManagerObject = new GameObject("DesignManager");
                    _instance = designManagerObject.AddComponent<DesignManager>();
                }
            }

            return _instance;
        }
    }

    public void Start()
    {
        // Listen to app phase change
        if (AppManager.Instance != null)
        {
            // AppManager.Instance.OnAppPhaseChanged += UpdateAppPhaseEvent;
        }

        // bool placed = placeBuilding();
    }

    void OnDestroy() // Unsubscribe from event when UIManager is destroyed
    {
        if (AppManager.Instance != null)
        {
            // AppManager.Instance.OnAppPhaseChanged -= UpdateAppPhaseEvent;
        }
    }

    public void PlaceBuilding()
    {
        if (_building == null)
        {
            Transform buildingTransform = Instantiate(buildingPrefab, container);
            buildingTransform.gameObject.SetActive(true);
        }
    }

    public void AddArea(int area)
    {
        coveredArea += area;
        TriggerAreaUpdatedEvent();
    }

    public void SubtractArea(int area)
    {
        coveredArea -= area;
        TriggerAreaUpdatedEvent();
    }

    public void ResetAreaCovered()
    {
        coveredArea = 0;
        TriggerAreaUpdatedEvent();

        Debug.Log("Area covered set to 0");
    }

    private void TriggerAreaUpdatedEvent()
    {
        OnAreaUpdatedEvent?.Invoke(coveredArea);

        // Additional logic can be added here if needed
        if (coveredArea == TOTAL_AREA)
        {
            Debug.Log("Building is full");
            DesignNetworkSyncScript.Instance.ActivateNextFloorBtnServerRpc();
        }
    }

 }
