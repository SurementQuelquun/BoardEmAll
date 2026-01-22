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