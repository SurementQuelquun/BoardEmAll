using UnityEngine;
using UnityEngine.UIElements; // Required for UI Toolkit

public class GlobalUIController : MonoBehaviour
{
    [Header("UI Document")]
    public UIDocument uiDocument; // Reference to the Game Object with the UI
    public Tower towerBuilder;

    [Header("UXML Templates")]
    public VisualTreeAsset gameMenuTemplate;
    public VisualTreeAsset attackTowersTemplate;
    public VisualTreeAsset supportTowersTemplate;

    // The root element of the active UI
    private VisualElement root;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        LoadGameMenu();
    }

    // --- LOADING FUNCTIONS ---

    public void LoadGameMenu()
    {

        uiDocument.visualTreeAsset = gameMenuTemplate;
        root = uiDocument.rootVisualElement;
        Button attackBtn = root.Q<Button>("AttackButton");
        if (attackBtn != null) attackBtn.clicked += LoadAttackTowers;


        Button supportBtn = root.Q<Button>("SupportButton");
        if (supportBtn != null) supportBtn.clicked += LoadSupportTowers;
    }

    public void LoadAttackTowers()
    {
        uiDocument.visualTreeAsset = attackTowersTemplate;
        root = uiDocument.rootVisualElement;

        // 1. Find the Back Button
        Button backBtn = root.Q<Button>("BackButton");
        if (backBtn != null) backBtn.clicked += LoadGameMenu;


        // Setup Buttons with Debugs
        SetupTowerButton("FishButton", 0);
        SetupTowerButton("KrakenButton", 1);
        SetupTowerButton("UrchinButton", 2);
        SetupTowerButton("SharkButton", 3);
    }

    public void LoadSupportTowers()
    {
        uiDocument.visualTreeAsset = supportTowersTemplate;
        root = uiDocument.rootVisualElement;

        Button backBtn = root.Q<Button>("BackButton");
        if (backBtn != null)
            backBtn.clicked += LoadGameMenu;

        // Setup support tower buttons (IDs 4..7)
        SetupTowerButton("KelpiButton", 4);
        SetupTowerButton("SirenButton", 5);
        SetupTowerButton("ScyllaButton", 6);

    }
    // Helper to keep code clean and add logging
    private void SetupTowerButton(string buttonName, int id)
    {
        Button btn = root.Q<Button>(buttonName);
        btn.clicked += () =>
        {

            towerBuilder.SelectTowerByID(id);
        };

    }
}