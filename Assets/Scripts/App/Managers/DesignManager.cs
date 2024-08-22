using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class DesignManager : MonoBehaviour { 

    private static DesignManager _instance;
    [SerializeField] private Transform buildingPrefab;
    [SerializeField] private Transform container;

    private Transform _building;
    //singleton
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
                    // Create a new GameObject and attach UIManager if not found
                    GameObject designManagerObject = new GameObject("DesignManager");
                    _instance = designManagerObject.AddComponent<DesignManager>();
                }
            }

            return _instance;
        }
    }

    public void Start()
    {
        //listen to app phase change
        if (AppManager.Instance != null)
        {
           // AppManager.Instance.OnAppPhaseChanged += UpdateAppPhaseEvent;
        }

        ///bool placed= placeBuilding();
    }


    void OnDestroy() // Unsubscribe from event when UIManager is destroyed
    {
        if (AppManager.Instance != null)
        {
            //  AppManager.Instance.OnAppPhaseChanged -= UpdateAppPhaseEvent;
        }
    }

    //

    public bool placeBuilding()
    {
        if (_building == null)
        {
            Transform buildingTransform = Instantiate(buildingPrefab, container);
            buildingTransform.gameObject.SetActive(true);
        }

        return true;
    }

}


