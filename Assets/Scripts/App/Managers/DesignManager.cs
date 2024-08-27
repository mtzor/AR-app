using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class DesignManager : MonoBehaviour
{
    private static DesignManager _instance;
    [SerializeField] private Transform buildingPrefab;
    [SerializeField] private Transform[] modulePrefabs;
    [SerializeField] private Transform floorPrefab;
    [SerializeField] private Transform container;
    [SerializeField] private Transform moduleContainer;
    [SerializeField] private Transform FloorContainer;

    [SerializeField] private GameObject finalizeFloorButton;

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

    public void SpawnModule(int i)
    {
        Instantiate(modulePrefabs[i], moduleContainer);
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

    private void TriggerAreaUpdatedEvent()
    {
        OnAreaUpdatedEvent?.Invoke(coveredArea);

        // Additional logic can be added here if needed
        if (coveredArea >= TOTAL_AREA)
        {
            Debug.Log("Building is full");
            finalizeFloorButton.SetActive(true);
        }
    }

    public void AddFloor()
    {

        // Increment the floor count
        floorCount++;

        if (floorCount < MAX_FLOORS)
        {
            // Calculate the height offset for the new floor
            float heightOffset = floorPrefab.localScale.y * floorCount;

            // Instantiate the new floor at the appropriate height
            Transform newFloor = Instantiate(floorPrefab, FloorContainer);

            // Adjust the position of the new floor
            newFloor.localPosition = new Vector3(0, heightOffset, 0);
        }
    }
}
