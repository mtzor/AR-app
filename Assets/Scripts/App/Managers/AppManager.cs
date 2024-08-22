using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;


public class AppManager : MonoBehaviour
{
    private static AppManager _instance;

    private AppPhase currentPhase;
    private AppPhase previousPhase;

    public enum AppPhase
    {
        Startup,
        MainMenu,
        Tutorial,
        HomeDialogue,
        Lobby_List_Design,
        Lobby_List_Customize,
        Lobby_Design,
        Lobby_Customize,
        Design_P1,
        Design_P2,
        Customize_P1,
        Customize_P2,
        Visualize_Menu,
        Visualize

    }

    public class AppPhaseChangeEvent
    {
        public AppManager.AppPhase newPhase; // The new game phase

        public AppPhaseChangeEvent(AppManager.AppPhase phase)
        {
            newPhase = phase;
        }
    }

    public delegate void OnAppPhaseChange(AppPhaseChangeEvent e);
    public OnAppPhaseChange OnAppPhaseChanged; // Event variable

    public AppPhase PreviousPhase()
    {
        return previousPhase;
    }

    // Start is called before the first frame update
    void Start()
    {
        //UpdatePhase(AppPhase.Startup);
       // UIManager.Instance.HideAll();  // Hide all UI elements at the start
        ///UIManager.Instance.Show("MainMenu");  // Show the main menu UI

    }
    public static AppManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<AppManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject appManagerObject = new GameObject("AppManager");
                    _instance = appManagerObject.AddComponent<AppManager>();
                }
            }

            return _instance;
        }
    }

    public async void UpdatePhase(AppPhase nextPhase)
    {
        previousPhase = currentPhase;//updating previous phase

        switch (nextPhase)
        {
            case AppPhase.Startup:
                // Perform startup tasks
                currentPhase = AppPhase.Startup;

                DialogButtonType result = await StartApp();

                if(result == DialogButtonType.Positive)
                {
                    Debug.Log("view tutorial");
                    UpdatePhase(AppPhase.Tutorial);//calling update phase for the tutorial
                }
                else
                {
                    Debug.Log("Main Menu . No tutorial");
                    TriggerAppPhaseChange();
                    UpdatePhase(AppPhase.MainMenu);//calling update phase for the tutorial
                }
                break;

            case AppPhase.Tutorial:
                currentPhase = AppPhase.Tutorial;

                TriggerAppPhaseChange();

                //running tutorial task
                int exitCode = await TutorialManager.Instance.runTutorial();

                UpdatePhase(AppPhase.MainMenu);

                break;

            case AppPhase.MainMenu:
                // Handle main menu logic

                currentPhase = AppPhase.MainMenu;
                TriggerAppPhaseChange();

                break;
            case AppPhase.Lobby_List_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Design;

                break;
            case AppPhase.Lobby_List_Customize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Customize;

                break;
            case AppPhase.Lobby_Customize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Customize;

                break;
            case AppPhase.Lobby_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Design;

                break;
            case AppPhase.Customize_P1:
                // Handle tutorial logic
                currentPhase = AppPhase.Customize_P1;

                TriggerAppPhaseChange();


                break;
            case AppPhase.Design_P1:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P1;

                TriggerAppPhaseChange();


                break;

            case AppPhase.HomeDialogue:
                // Handle tutorial logic
                currentPhase = AppPhase.HomeDialogue;

                break;
                // Add similar cases for other phases
        }

        //trigger app phase changed event
        Debug.Log("Design Phase"+currentPhase);
        TriggerAppPhaseChange();

    }

    private void TriggerAppPhaseChange()
    {
        if (OnAppPhaseChanged != null)
        {
            OnAppPhaseChanged(new AppPhaseChangeEvent(currentPhase)); // Trigger the event with a new AppPhaseChangeEvent object
        }
    }


    public void setPhaseLobbyList()
    {
        if(LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            UpdatePhase(AppPhase.Lobby_List_Design);
        }
        else if(LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize)
        {
            UpdatePhase(AppPhase.Lobby_List_Customize);
        }
    }

    public void setPhaseLobby()
    {
        if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            UpdatePhase(AppPhase.Lobby_Design);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize)
        {
            UpdatePhase(AppPhase.Lobby_Customize);
        }
    }

    public void setPhase()
    {
        if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            UpdatePhase(AppPhase.Design_P1);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize)
        {
            UpdatePhase(AppPhase.Customize_P1);
        }
    }

    public async Task<DialogButtonType> StartApp()
    {

        DialogButtonType result = await DialogueManager.Instance.SpawnDialogWithAsync("Welcome to the AR-App application !", "Would you like to view the application tutorial ?", "VIEW","CANCEL");
        return result;
    }

   /* public async Task<int> Design_P1()
    {
        await 
    }*/
}
