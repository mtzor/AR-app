using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] PressableButton cancelTutorialButton;
    private static TutorialManager _instance;

    public GameObject cube;

    private bool handDetected;
    private bool handRemoved;
    private bool menuManipulated;
    private bool menuToggled;
    private bool menuClosed;
    private bool sliderUpdated;
    public bool cubeMoved;

    private int tutorial_exit_code;
    private CancellationTokenSource _cancellationTokenSource;
    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Find the UIManager in the scene
                _instance = FindObjectOfType<TutorialManager>();

                if (_instance == null)
                {
                    // Create a new GameObject and attach UIManager if not found
                    GameObject tutorialeManagerObject = new GameObject("TutorialManager");
                    _instance = tutorialeManagerObject.AddComponent<TutorialManager>();
                }
            }

            return _instance;
        }
    }

    void Start()
    {
        //disable quit tutorial button
        cancelTutorialButton.gameObject.SetActive(false);
        cube.SetActive(false);

        tutorial_exit_code = 0;
        handDetected = false;
        handRemoved = false;
        menuManipulated = false;
        menuToggled = false;
        menuClosed = false;
        sliderUpdated = false;

        cancelTutorialButton.OnClicked.AddListener(() => { cancelTutorial(); });

        UIManager.Instance.Show("HandMenu");//???????

        //
        //runTutorial();
    }

    private void cancelTutorial()
    {
        Debug.Log("Cancel");
        _cancellationTokenSource?.Cancel();
    }

    public bool HandDetected
    {
        get => handDetected;
        set => handDetected = value;
    }

    public bool HandRemoved
    {
        get => handRemoved;
        set => handRemoved = value;
    }

    public bool MenuManipulated
    {
        get => menuManipulated;
        set => menuManipulated = value;
    }

    public bool MenuToggled
    {
        get => menuToggled;
        set => menuToggled = value;
    }
    public bool MenuClosed
    {
        get => menuClosed;
        set => menuClosed = value;
    }

    public bool SliderUpdated
    {
        get => sliderUpdated;
        set => sliderUpdated = value;
    }

    public bool CubeMoved
    {
        get => cubeMoved;
        set => cubeMoved = value;
    }

    public async Task<int> runTutorial()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken token = _cancellationTokenSource.Token;

        try
        {
            await DialogueManager.Instance.SpawnDialogWithAsync("Welcome to the application tutorial!", "In this tutorial we will show you the main gestures needed to navigate the app. Its fairly easy!", "PRESS TO CONTINUE");//??????????????? token


            //enable quit tutorial button
            cancelTutorialButton.gameObject.SetActive(true);
            
            DialogueManager.Instance.SpawnNeutralDialogFromCode("Let's view the Hand Menu.", "Raise your hand and look at your flat palm.You may need to move your hands out of view then back into view for the toggled menu to appear.");

            while (!handDetected)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);                
            }

            DialogueManager.Instance.SpawnNeutralDialogFromCode("Well done!", "This is the Hand Menu. Remove your hand and it will remain on your view");

            while (!handRemoved)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }

            DialogueManager.Instance.SpawnNeutralDialogFromCode("Use the bar on the bottom to place it in your view.", "Give it a try now!");

            while (!menuManipulated)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }

            DialogueManager.Instance.SpawnNeutralDialogFromCode("Moving on. Let's explore some features of the hand menu", "Press the scene helper toggle!");

            while (!menuToggled)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }

            await Task.Delay(2000, token);
            DialogueManager.Instance.SpawnNeutralDialogFromCode("The scene helper is there to guide you throughout the app.", "You can turn it off now.");

            while (menuToggled)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }

            DialogueManager.Instance.SpawnNeutralDialogFromCode("Now on to the next Gesture! The pinch Gesture :", "We will try pinching to adjust the app volume.\r\n 1.Hold up your other arm and connect your pointer with your thumb to grab the volume nob. \r\n 2.Then move your Hand Up/Down to adjust the app Volume.");
            await Task.Delay(1000, token);

            while (!sliderUpdated)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }
            DialogueManager.Instance.SpawnNeutralDialogFromCode("Well done!", "You can now close the hand menu.");

            while (!menuClosed)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }

            DialogueManager.Instance.SpawnNeutralDialogFromCode("Well done!Let's now see far interaction.", "When an object is not close enough you can interact with it by:\r\n 1. Pointing at it with your index finger \r\n2. Pinching \r\n Try to move this cube from afar ");
            cube.SetActive(true);
            while (!cubeMoved)
            {
                token.ThrowIfCancellationRequested();
                await Task.Delay(500, token);
            }

            await DialogueManager.Instance.SpawnDialogWithAsync("Well done!The tutorial is now complete.", "You can view the tutorial at any time by using the corresponding hand menu button", "OK");// token ??????
            cube.SetActive(false);
            cancelTutorialButton.gameObject.SetActive(false);
            await Task.Delay(2000, token);

            return 1;
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Tutorial was canceled.");

           // DialogueManager.Instance.DismissDialog();
            return 0; // Return 0 if the tutorial was canceled

        }
    }
}
