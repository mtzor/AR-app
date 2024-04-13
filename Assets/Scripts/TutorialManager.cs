using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MixedReality.Toolkit.UX;
using UnityEngine.Events;

public class TutorialManager : MonoBehaviour
{
    public DialogueManager dialogManager;
    public static TutorialManager instance;

    public bool handDetected;
    public bool handRemoved;
    public bool menuManipulated;

    private int tutorial_exit_code;
    // Start is called before the first frame update
    void Start()
    {
        tutorial_exit_code = 0;
        handDetected= false;

        runTutorial();
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
        // Update is called once per frame
        void Update()
    {
        
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

    public async Task<int> runTutorial()
    {
        //Showing Welcome To the Tutorial Dialogue asynchronously
        await dialogManager.SpawnDialogWithAsync("Welcome to the application tutorial!", "In this tutorial we will show you the main gestures needed to navigate the app. Its fairly easy!", "PRESS TO CONTINUE");

        dialogManager.SpawnNeutralDialogFromCode("Let's view the Hand Menu.", "Raise your hand and look at your flat palm.You may need to move your hands out of view then back into view for the toggled menu to appear.");

        while(!handDetected){
            Debug.Log("in while 1");
            await Task.Delay(500);
        }

        Debug.Log("awaited");

        dialogManager.SpawnNeutralDialogFromCode("Well done!", "This is the Hand Menu. Remove your hand and it will remain on your view");


        while (!handRemoved)
        {
            Debug.Log("in while 2");
            await Task.Delay(500);
        }

        Debug.Log("awaited 2");

       // await Task.Delay(1000);

        dialogManager.SpawnNeutralDialogFromCode("Use the bar on the bottom to place it in your view.", "Give it a try now!");

        while (!menuManipulated)
        {
            Debug.Log("in while 3");
            await Task.Delay(500);
        }
        Debug.Log("awaited 3");


        dialogManager.SpawnNeutralDialogFromCode("Moving on. Let's explore some features of the hand menu", "Press the scene helper toggle!");

        while (!menuManipulated)
        {
            Debug.Log("in while 4");
            await Task.Delay(500);
        }
        Debug.Log("awaited 4");

        return 0;

    }
}
