using MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class CustomizeManager : NetworkBehaviour
{
    private static CustomizeManager _instance;
    [SerializeField] private GameObject customize_P1_UI;
    [SerializeField] private GameObject customize_P2_UI;

    [SerializeField] private GameObject roomMenuL;
    [SerializeField] private GameObject roomMenuS;

    [SerializeField] private Transform sharedViewPrefab;

    [SerializeField] private Transform layoutContainer;

    [SerializeField] private ModuleLayouts[] moduleLayouts = new ModuleLayouts[9];

    [SerializeField]private LayoutManager privateLayoutManager;
    [SerializeField]private LayoutManager sharedLayoutManager;

    private CustomizePhase privatePhase;
    private CustomizePhase sharedPhase;

    public int[] privateChoices = new int[2];//to debug
    public int[] sharedChoices = new int[2];


    private LayoutManager currentLayoutManager;
    public string _selectedModule;

    public string SelectedModule { set => _selectedModule = value; }
    public LayoutManager CurrentLayoutManager { get; set; }
    public LayoutManager PrivateLayoutManager { get; }
    public LayoutManager SharedLayoutManager { get; }
    public void SetChoice(bool isShared, int choice)
    {
        Debug.Log("Setting Choice");
        if (isShared)
        {
            if (sharedPhase == CustomizePhase.Choose_layout)
            {
                sharedChoices[0]=choice;
            }
            else if (sharedPhase==CustomizePhase.Choose_room_layout)
            {
                sharedChoices[1] = choice;
            }
        }
        else
        {
            if (privatePhase == CustomizePhase.Choose_layout)
            {
                privateChoices[0] = choice;
                Debug.Log("Setting Choice private choose layout");
            }
            else if (privatePhase == CustomizePhase.Choose_room_layout)
            {
                privateChoices[1] = choice;

                Debug.Log("Setting Choice private choose room layout");
            }
        }
    }
    public CustomizePhase PrivatePhase { get; set; }
    public CustomizePhase SharedPhase { get; set; }

    public void ToggleCustomize_P1_UI(bool toggle)
    {
        customize_P1_UI.SetActive(toggle);
    }

    public void ToggleCustomize_P2_UI(bool toggle)
    {
        customize_P2_UI.SetActive(toggle);
    }

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

    public enum CustomizePhase
    {
        Choose_layout,
        Choose_room_layout,
        Customize_layout,
        Customize_facade//??????????????when to save ?
    }

    // Start is called before the first frame update
    void Start()
    {
        CurrentLayoutManager = privateLayoutManager;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void SetupServerLayoutInterfaces()
    {
        Debug.Log("SELECTED MODULE: " + _selectedModule);

        sharedPhase = CustomizePhase.Choose_layout;
        privatePhase = CustomizePhase.Choose_layout;

        // Find layouts for the selected module
        List<Transform> selectedModuleLayouts = new List<Transform>();
        foreach (var module in moduleLayouts)
        {
            if (module.moduleName == _selectedModule)
            {
                selectedModuleLayouts = module.layouts;
                break;
            }
        }

        // Instantiate and configure the PrivateView (not networked)
        PrivateView layoutPrivate = new PrivateView(selectedModuleLayouts, layoutContainer);

        // Instantiate and configure the SharedView (networked)
        SharedView layoutShared = InstantiateSharedView(selectedModuleLayouts, layoutContainer);

        // Set the views in the ViewManager
        ViewManager.Instance.PrivateView = layoutPrivate;
        ViewManager.Instance.SharedView = layoutShared;

        ViewManager.Instance.SetPrivateView(layoutPrivate);
        ViewManager.Instance.SetSharedView(layoutShared);

        ViewManager.Instance.uiController.Initialize(layoutPrivate);
        ViewManager.Instance.InitializeViewManager();
        ViewManager.Instance.uiController.SetView(layoutPrivate);
    }

    private SharedView InstantiateSharedView(List<Transform> selectedModuleLayouts, Transform layoutContainer)
    {
        // Ensure SharedViewPrefab is set up in the inspector
        if (sharedViewPrefab == null)
        {
            Debug.LogError("SharedViewPrefab is not assigned in the inspector!");
            return null;
        }

        // Instantiate the prefab
        Transform sharedViewObject = Instantiate(sharedViewPrefab);

        // Configure the SharedView instance
        SharedView sharedView = sharedViewObject.GetComponent<SharedView>();
        sharedView.Initialize(selectedModuleLayouts, layoutContainer);

        // Spawn it over the network (only the server can do this)
        if (NetworkManager.Singleton.IsServer)
        {
            sharedViewObject.GetComponent<NetworkObject>().Spawn();
        }


        return sharedView;
    }

    public void SetupClientLayoutInterfaces()
    {
        Debug.Log("SELECTED MODULE: " + _selectedModule);

        sharedPhase = CustomizePhase.Choose_layout;
        privatePhase = CustomizePhase.Choose_layout;

        // Find layouts for the selected module
        List<Transform> selectedModuleLayouts = new List<Transform>();
        foreach (var module in moduleLayouts)
        {
            if (module.moduleName == _selectedModule)
            {
                selectedModuleLayouts = module.layouts;
                break;
            }
        }

        // Instantiate and configure the PrivateView (not networked)
        PrivateView layoutPrivate = new PrivateView(selectedModuleLayouts, layoutContainer);


        // Instantiate and configure the SharedView (networked)
        SharedView layoutShared = FindObjectOfType<SharedView>();
        if (layoutShared != null)
        {
            Debug.Log("SharedView found in the hierarchy: " + layoutShared.gameObject.name);
            layoutShared.SetItems(selectedModuleLayouts);
        }
        else
        {
            Debug.Log("SharedView not found in the hierarchy.");
        }

            // Set the views in the ViewManager
        ViewManager.Instance.PrivateView = layoutPrivate;
        ViewManager.Instance.SharedView = layoutShared;

        ViewManager.Instance.SetPrivateView(layoutPrivate);
        ViewManager.Instance.SetSharedView(layoutShared);

        ViewManager.Instance.uiController.Initialize(layoutPrivate);
        ViewManager.Instance.InitializeViewManager();
        ViewManager.Instance.uiController.SetView(layoutPrivate);
    }

    public void SetupInterface(bool isShared)
    {
        if (isShared)
        {
            if (sharedPhase == CustomizePhase.Choose_layout)
            {
                SetupRoomLayouts(isShared);
            }
            else if (sharedPhase == CustomizePhase.Choose_room_layout)
            {

               SetupCustomizeLayout(isShared);
            }
        }
        else
        {
            if (privatePhase == CustomizePhase.Choose_layout)
            {
                SetupRoomLayouts(isShared);

                Debug.Log("Setting private room layouta interface");
            }
            else if (privatePhase == CustomizePhase.Choose_room_layout)
            {
                SetupCustomizeLayout(isShared);
                Debug.Log("Setting private customize layouta interface");
            }
        }
    }
    public ModuleLayouts currModule; 

    public List<Transform> selectedModuleRoomLayouts = new List<Transform>();
    public void SetupRoomLayouts(bool shared)
    {
        foreach (var module in moduleLayouts)
        {

            if (module.moduleName == _selectedModule)
            {
                int selectedIndex;
                currModule= module;

                if (shared)
                {
                    selectedIndex = sharedChoices[0];
                    sharedPhase = CustomizePhase.Choose_room_layout;
                    selectedModuleRoomLayouts = module.roomLayouts[selectedIndex].roomLayouts;

                    sharedLayoutManager.Layout= module.layouts[selectedIndex];
                   // ViewManager.Instance.SharedView.SetSharedItemsForClients(selectedIndex);

                }
                else
                {
                    Debug.Log("PRIVATE SETUP ROOM LAYOUTS");
                    selectedIndex = privateChoices[0];
                    privatePhase = CustomizePhase.Choose_room_layout;
                    selectedModuleRoomLayouts = module.roomLayouts[selectedIndex].roomLayouts;

                    privateLayoutManager.Layout = module.layouts[selectedIndex];
                }


                Debug.Log("Selected index" + selectedIndex);
                //Debug.Log("SELECTED MODULE ROOM LAYOUTS" +);

                foreach(var layout in selectedModuleRoomLayouts)
                {

                    Debug.Log("Layout Name " + layout.name);
                }

                break;
            }
        }

        if (shared)
        {
            ViewManager.Instance.SharedView.SetItems(selectedModuleRoomLayouts);
        }
        else
        {
            ViewManager.Instance.PrivateView.SetItems(selectedModuleRoomLayouts);
        }
    }
    
    public void SetupCustomizeLayout(bool shared)
    {
        //instantiate the correct interface based on choice[2]?

        //setup lists for shared or not of rooms and items

        List<Transform> itemList = new List<Transform>();

        foreach (var module in moduleLayouts)
            {

                if (module.moduleName == _selectedModule)
                {
                    int selectedIndex0;                
                    int selectedIndex1;
                    currModule = module;

                    if (shared)
                    {                    
                        selectedIndex0 = sharedChoices[0];
                        selectedIndex1 = sharedChoices[1];
                        sharedPhase = CustomizePhase.Customize_layout;

                        selectedModuleRoomLayouts = module.roomLayouts[selectedIndex0].roomLayouts;

                        sharedLayoutManager.SetRoomLayout(module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1]);
                        itemList.Add(privateLayoutManager.RoomLayout);
                        sharedLayoutManager.DisplayLayoutModel();
                    // ViewManager.Instance.SharedView.SetSharedItemsForClients(selectedIndex);

                }
                    else
                    {
                        selectedIndex0 = privateChoices[0];
                        selectedIndex1 = privateChoices[1];

                        privatePhase = CustomizePhase.Customize_layout;
                        selectedModuleRoomLayouts = module.roomLayouts[selectedIndex0].roomLayouts;

                        privateLayoutManager.SetRoomLayout(module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1]);

                        Debug.Log("Name:"+module.roomLayouts[selectedIndex0].customizationLayouts[selectedIndex1].name);
                        itemList.Add(privateLayoutManager.RoomLayout);

                        privateLayoutManager.DisplayLayoutModel();
                    }
                 
                }
            }

            if (shared)
            {
                ViewManager.Instance.SharedView.SetItems(itemList);
            }
            else
            {
                ViewManager.Instance.PrivateView.SetItems(itemList);
            }
        
    }
    
    public void setLayoutManager(bool isShared)
    {
        if (!isShared)
        {
            currentLayoutManager = privateLayoutManager;
        }
        else
        {
            currentLayoutManager = sharedLayoutManager;
        }
    }

}
