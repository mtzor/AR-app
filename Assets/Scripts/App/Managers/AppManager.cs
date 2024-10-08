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

    #region AppPhases & AppPhaseChangeEvent
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
        Saving_Design,
        Visualize_Menu,
        Visualize

    }

    //Get current phase of the app
    public AppPhase CurrentPhase()
    {
        return currentPhase;
    }
    //Get the previous App Phase
    public AppPhase PreviousPhase()
    {
        return previousPhase;
    }

    //custom event fired when the app changes phase
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

    #endregion


    // Start is called before the first frame update
    void Start()
    {
        UpdatePhase(AppPhase.Startup);
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

    public async Task UpdatePhase(AppPhase nextPhase)
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
                    UpdatePhase(AppPhase.Tutorial);//calling update phase for the tutorial
                }
                else
                {
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

                currentPhase = AppPhase.MainMenu;

                Debug.Log("Update phase Main menu");
                TriggerAppPhaseChange();

                break;

            case AppPhase.Lobby_List_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Design;

                LoadingManager.Instance.SetLoadingText("Loading Lobbies");
                LoadingManager.Instance.EnableLoadingScreen();
                               
                    await Task.Delay(3000);
               
                LoadingManager.Instance.DisableLoadingScreen();


                break;

            case AppPhase.Lobby_List_Customize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_List_Customize;

                LoadingManager.Instance.SetLoadingText("Loading Lobbies");
                LoadingManager.Instance.EnableLoadingScreen();

                while (!LobbyManager.Instance.LobbyCreated && !LobbyManager.Instance.LobbyJoined)
                {
                    await Task.Delay(200);
                }

                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();

                break;

            case AppPhase.Lobby_Customize:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Customize;

                LoadingManager.Instance.SetLoadingText("Loading Lobby");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(5000);

                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();

                break;

            case AppPhase.Lobby_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Lobby_Design;

                LoadingManager.Instance.SetLoadingText("Loading Lobby");
                LoadingManager.Instance.EnableLoadingScreen();

                while (!LobbyManager.Instance.LobbyCreated && !LobbyManager.Instance.LobbyJoined)
                {
                    await Task.Delay(200);
                }
                
                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();
                break;

            case AppPhase.Customize_P1:
                // Handle tutorial logic
                currentPhase = AppPhase.Customize_P1;

                LoadingManager.Instance.SetLoadingText("Loading Lobby");
                LoadingManager.Instance.EnableLoadingScreen();

                while (!LobbyManager.Instance.LobbyCreated && !LobbyManager.Instance.LobbyJoined)
                {
                    await Task.Delay(200);
                }

                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();

                break;

            case AppPhase.Design_P1:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P1;


                LoadingManager.Instance.SetLoadingText("Loading Design Interface");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(3000);

                LoadingManager.Instance.DisableLoadingScreen();

                TriggerAppPhaseChange();
                break;

            case AppPhase.Customize_P2:
                // Handle tutorial logic
                currentPhase = AppPhase.Customize_P2;

                TriggerAppPhaseChange();

                break;

            case AppPhase.Design_P2:
                // Handle tutorial logic
                currentPhase = AppPhase.Design_P2;

                TriggerAppPhaseChange();

                break;

            case AppPhase.Saving_Design:
                // Handle tutorial logic
                currentPhase = AppPhase.Saving_Design;

                LoadingManager.Instance.SetLoadingText("Saving Design");
                LoadingManager.Instance.EnableLoadingScreen();

                await Task.Delay(5000);

                LoadingManager.Instance.DisableLoadingScreen();

                UpdatePhase(AppPhase.MainMenu);

                //TriggerAppPhaseChange();

                break;

            case AppPhase.HomeDialogue:
                // Handle tutorial logic
                currentPhase = AppPhase.HomeDialogue;

                break;
                // Add similar cases for other phases
        }

        //trigger app phase changed event
        TriggerAppPhaseChange();

    }

    private void TriggerAppPhaseChange()
    {
        if (OnAppPhaseChanged != null)
        {
            OnAppPhaseChanged(new AppPhaseChangeEvent(currentPhase)); // Trigger the event with a new AppPhaseChangeEvent object
        }
    }

    #region setPhase Functions

    //Changes AppPhase Main Menu -> Lobby_list
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

    //Changes AppPhase Lobby_list -> Lobby
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

    //Changes AppPhase Lobby -> P1 (Design or Customize)
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

    //Changes AppPhase P1 (Design or Customize) -> P2
    public void setNextPhase()
    {
        if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Design)
        {
            UpdatePhase(AppPhase.Design_P2);
        }
        else if (LobbyManager.Instance.sessionMode == LobbyManager.SessionMode.Customize)
        {
            UpdatePhase(AppPhase.Customize_P2);
        }
    }
    
    #endregion
    public async Task<DialogButtonType> StartApp()
    {

        DialogButtonType result = await DialogueManager.Instance.SpawnDialogWithAsync("Welcome to the AR-App application !", "Would you like to view the application tutorial ?", "VIEW","CANCEL");
        return result;
    }

}
