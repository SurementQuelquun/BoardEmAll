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

    //[Header("Tower Prefabs")]
    //public GameObject flyingFishPrefab;
    //public GameObject krakenPrefab;
    //public GameObject seaUrchinPrefab;
    //public GameObject sharkPrefab;

    // The root element of the active UI
    private VisualElement root;

    private void OnEnable()
    {
        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
        if (towerBuilder == null) Debug.LogError("UIController: TOWER BUILDER IS NULL! Assign Game Area in Inspector.");

        LoadGameMenu();
    }

    // --- LOADING FUNCTIONS ---

    public void LoadGameMenu()
    {
        // 1. Clone the template into the root
        Debug.Log("UI: Loading Game Menu");
        uiDocument.visualTreeAsset = gameMenuTemplate;
        root = uiDocument.rootVisualElement;

        // 2. Find the buttons in this specific template
        //Button attackBtn = root.Q<Button>("AttackButton");
        //Button supportBtn = root.Q<Button>("SupportButton"); // If you have one

        // 3. Connect the buttons to functions
        //if (attackBtn != null)
        //    attackBtn.clicked += LoadAttackTowers;

        //if (supportBtn != null)
        //    supportBtn.clicked += LoadSupportTowers;
        Button attackBtn = root.Q<Button>("AttackButton");
        if (attackBtn != null) attackBtn.clicked += LoadAttackTowers;

        // If you have a Support button
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

        // 2. Find the Tower Buttons (MAKE SURE NAMES MATCH UXML)
        //Button fishBtn = root.Q<Button>("FishButton");
        //Button krakenBtn = root.Q<Button>("KrakenButton");
        //Button urchinBtn = root.Q<Button>("UrchinButton");
        //Button sharkBtn = root.Q<Button>("SharkButton");

        //// 3. Connect Buttons to the Tower Script
        //// We use a "Lambda" () => to pass the specific prefab
        //if (fishBtn != null) fishBtn.clicked += () => towerBuilder.SelectTowerByID(0);
        //if (krakenBtn != null) krakenBtn.clicked += () => towerBuilder.SelectTowerByID(1);
        //if (urchinBtn != null) urchinBtn.clicked += () => towerBuilder.SelectTowerByID(2);
        //if (sharkBtn != null) sharkBtn.clicked += () => towerBuilder.SelectTowerByID(3);
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

        Debug.Log("Switched to Support Towers Menu");
    }
    // Helper to keep code clean and add logging
    private void SetupTowerButton(string buttonName, int id)
    {
        Button btn = root.Q<Button>(buttonName);
        if (btn != null)
        {
            btn.clicked += () =>
            {
                Debug.Log($"UI: {buttonName} clicked! Sending ID {id} to Tower.cs");
                towerBuilder.SelectTowerByID(id);
            };
        }
        else
        {
            Debug.LogWarning($"UI: Could not find button named '{buttonName}' in AttackTowers.uxml");
        }
    }
}