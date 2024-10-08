using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;

    [SerializeField] private List<GameObject> uiElements;  // List to hold all UI elements

    private Dictionary<string, GameObject> uiElementsDict;

    private string previousInterface;
    private string currentInterface;
    //singleton
    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<UIManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject uiManagerObject = new GameObject("UIManager");
                    _instance = uiManagerObject.AddComponent<UIManager>();
                }
            }

            return _instance;
        }
    }

    public void Start()
    {
        InitializeUIElements();
        //listen to app phase change
        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnAppPhaseChanged += UpdateAppPhaseEvent;
        }
    }

    #region UI init & toggling
    // Initialize all UI elements and store them in a dictionary for easy access
    private void InitializeUIElements()
    {
        uiElementsDict = new Dictionary<string, GameObject>();

        foreach (GameObject uiElement in uiElements)
        {
            if (uiElement != null)
            {
                uiElementsDict.Add(uiElement.name, uiElement);
                uiElement.SetActive(false);  // Start with all UI elements hidden
            }
        }
    }

    // Show a specific UI element by name
    public void Show(string uiElementName)
    {
        if (uiElementsDict.ContainsKey(uiElementName))
        {
            uiElementsDict[uiElementName].SetActive(true);
        }
        else
        {
            Debug.LogWarning($"UI Element {uiElementName} not found.");
        }
    }

    // Hide a specific UI element by name
    public void Hide(string uiElementName)
    {
        if (uiElementsDict.ContainsKey(uiElementName))
        {
            uiElementsDict[uiElementName].SetActive(false);
        }
        else
        {
            Debug.LogWarning($"UI Element {uiElementName} not found.");
        }
    }

    // Hide all UI elements
    public void HideAll()
    {
        foreach (var uiElement in uiElementsDict.Values)
        {
            uiElement.SetActive(false);
        }
    }

    #endregion
    private void UpdateAppPhaseEvent(AppManager.AppPhaseChangeEvent e)
    {
        // Implement logic to show/hide UI elements based on the newPhase (e.newPhase)

        previousInterface = currentInterface;

        switch (e.newPhase)
        {
            case AppManager.AppPhase.Startup:

                InitializeUIElements();
                // Hide all UI elements (already done in Start)
                break;

            case AppManager.AppPhase.Tutorial:
                HideAll();
                Show("HandMenu");
                currentInterface = "HandMenu";
                break;
            // Add similar cases for other phases
            case AppManager.AppPhase.MainMenu:
                HideAll();
                Show("MainMenu");
                currentInterface = "MainMenu";
                break;

            case AppManager.AppPhase.Lobby_List_Customize:
                HideAll();
                Show("Join/Create Lobby Interface");
                currentInterface = "Join/Create Lobby Interface";
                break;

            case AppManager.AppPhase.Lobby_List_Design:
                HideAll();
                Show("Join/Create Lobby Interface");
                currentInterface = "Join/Create Lobby Interface";
                break;

            case AppManager.AppPhase.Lobby_Customize:
                HideAll();
                Show("Lobby UI");
                currentInterface = "Lobby UI";
                break;

            case AppManager.AppPhase.Lobby_Design:
                HideAll();
                Show("Lobby UI");
                currentInterface = "Lobby UI";
                break;

            case AppManager.AppPhase.Customize_P1:
                HideAll();
                Show("Customize_P1 UI");
                currentInterface = "Customize_P1 UI";
                break;

            case AppManager.AppPhase.Design_P1:
                HideAll();
                Show("Design_P1 UI");
                currentInterface = "Design_P1 UI";
                break;

            case AppManager.AppPhase.Customize_P2:
                HideAll();
                Show("Customize_P2 UI");
                currentInterface = "Customize_P2 UI";
                break;

            case AppManager.AppPhase.Design_P2:

                currentInterface = "Design_P2 UI";
                HideAll();
                break;

            case AppManager.AppPhase.Saving_Design:

                currentInterface = "Saving_Design";
                HideAll();
                Show("MainMenu");
                break;
            case AppManager.AppPhase.HomeDialogue:
                HideAll();
                break;

        }
    }

    void OnDestroy() // Unsubscribe from event when UIManager is destroyed
    {
        if (AppManager.Instance != null)
        {
            AppManager.Instance.OnAppPhaseChanged -= UpdateAppPhaseEvent;
        }
    }
}
