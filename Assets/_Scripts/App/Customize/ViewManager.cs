using MixedReality.Toolkit.UX;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using Unity.Netcode;


public class ViewManager : MonoBehaviour
{

    private static ViewManager _instance;

    [SerializeField] public IView sharedView;
    [SerializeField] public IView privateView;

    [SerializeField] public ViewUIController uiController;

    [SerializeField] public TMP_Text completeText;
    [SerializeField] public GameObject progressIndicator;

    [SerializeField] public PressableButton finalizeChoiceBtn;

    [SerializeField] private PressableButton sharedViewToggle;
    [SerializeField] private TMP_Text sharedViewText;

    public IView currentIVew;

    private int selectedItem;
    private bool isShared = false;


    public bool IsShared{ set; get; }

    public int SelectedItem { get; set; }
    private void Start()
    {
        sharedViewToggle.OnClicked.AddListener(OnSharedViewToggled);
        finalizeChoiceBtn.OnClicked.AddListener(OnFinalizeChoiceBtnPressed);
    }
   
    public static ViewManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ViewManager>();
                if (_instance == null)
                {
                    GameObject viewManager = new GameObject("ViewManager");
                    _instance = viewManager.AddComponent<ViewManager>();
                }
            }
            return _instance;
        }
    }

    public void SetPrivateView(IView view)
    {
        privateView = view;
    }

    public void SetSharedView(IView view)
    {
        sharedView = view;
    }

    public IView PrivateView { get; set; }
    public IView SharedView { get; set; }

    public void InitializeViewManager()
    {
        sharedViewText.text = "Private View";
        currentIVew = privateView;

       // uiController.SetView(currentIVew);
    }
    private void OnSharedViewToggled()
    {
        Debug.Log("OnsharedViewToggled");
        currentIVew.DestroyCurrentItem();

        isShared = !isShared;

        CustomizeManager.Instance.setLayoutManager(isShared);
        // Handle view switch externally if necessary
        if (isShared) {
            if (SharedView ==null) {
                Debug.Log("Shared View is NULL");
            }
            sharedViewText.text = "Shared View";
            currentIVew = sharedView;
            sharedView.ReportSharedViewState(true);
            uiController.SetView(sharedView);
            sharedView.ShowCurrentItem();//?????
            uiController.ToggleCompareModeToggle(false);
        }
        else
        {
            sharedViewText.text = "Private View";
            sharedView.ReportSharedViewState(false);
            currentIVew = privateView;
            uiController.SetView(privateView);
            uiController.ToggleCompareModeToggle(true);
        }
    }
  
    private void OnFinalizeChoiceBtnPressed()
    {
        OnFinalizeChoiceBtnAsync();

        return;
    }

    private async Task OnFinalizeChoiceBtnAsync()
    {
        Debug.Log("FINALIZE CHOICE BTN PRESSED");
        finalizeChoiceBtn.gameObject.SetActive(false);

        await currentIVew.FinalizeChoice();       

        return;
    }

    public void SetNextCurrentViewPhase()
    {
        if (currentIVew.SelectedIndex() != -1)
        {
            selectedItem = currentIVew.SelectedIndex();

            if (currentIVew.IsShared)
            {
                if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Choose_layout)
                {
                    CustomizeManager.Instance.SharedPhase = CustomizeManager.CustomizePhase.Choose_room_layout;
                    Debug.Log("Setting up room layouts");
                   CustomizeManager.Instance.SetupRoomLayouts(isShared);
                    finalizeChoiceBtn.gameObject.SetActive(true);
                    CustomizeManager.Instance.ToggleCustomize_P1_UI(true);

                }
                else if (CustomizeManager.Instance.SharedPhase == CustomizeManager.CustomizePhase.Choose_room_layout)
                {
                    CustomizeManager.Instance.SharedPhase = CustomizeManager.CustomizePhase.Customize_layout;
                    Debug.Log("Setting the next phase appmanager");
                    AppManager.Instance.setNextPhase();
                    CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
                    CustomizeManager.Instance.ToggleCustomize_P2_UI(true);
                    CustomizeManager.Instance.SetupCustomizeLayout(isShared);
                }

            }
            else
            {
                Debug.Log("CustomizeManager.Instance.PrivatePhase" + CustomizeManager.Instance.PrivatePhase);
                if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Choose_layout)
                {
                    CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Choose_room_layout;
                    CustomizeManager.Instance.SetupRoomLayouts(isShared);
                    finalizeChoiceBtn.gameObject.SetActive(true);
                    CustomizeManager.Instance.ToggleCustomize_P1_UI(true);
                    Debug.Log("Setting up room layouts");
                }
                else if (CustomizeManager.Instance.PrivatePhase == CustomizeManager.CustomizePhase.Choose_room_layout)
                {
                    Debug.Log("Setting the next phase appmanager");
                    CustomizeManager.Instance.PrivatePhase = CustomizeManager.CustomizePhase.Customize_layout;
                    AppManager.Instance.setNextPhase();
                    CustomizeManager.Instance.ToggleCustomize_P1_UI(false);
                    CustomizeManager.Instance.ToggleCustomize_P2_UI(true);
                    CustomizeManager.Instance.SetupCustomizeLayout(isShared);
                }
            }

        }
        else
        {
            finalizeChoiceBtn.gameObject.SetActive(true);
        }
    }
    
}
