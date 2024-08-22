using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UnityEngine;

public class CustomizeManager : MonoBehaviour
{
    private static CustomizeManager _instance;
    [SerializeField] private Transform _buildingMoodelPrefab;
    [SerializeField] private Transform container;
    [SerializeField] BuildingModelScriptable buildingInfo;

    private GameObject _buildingModel;
    public static CustomizeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<CustomizeManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject customizeManagerObject = new GameObject("CustomizeManager");
                    _instance = customizeManagerObject.AddComponent<CustomizeManager>();
                }
            }

            return _instance;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        placeBuilding();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //?????????
    public void LoadModel()
    {
       
    }

    public async Task<int> placeBuilding()
    {
        if (_buildingModel == null)
        {
            Transform buildingTransform = Instantiate(_buildingMoodelPrefab,container);
            buildingTransform.gameObject.SetActive(true);

        }

        await DialogueManager.Instance.SpawnDialogWithAsync("Welcome to the customization interface!", "You can place the building using the bottom bar .", "OK");
        return 1;
    }
}
