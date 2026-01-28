using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CoinsPlayerUI : MonoBehaviour
{

    [SerializeField] private string CoinsElementName = "coinsText";
    [SerializeField] private string cPrefix = "Coins: ";

    private UIDocument uiDocument;
    private Label coinsText;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        coinsText = root.Q<Label>(CoinsElementName) ?? root.Q<Label>();
        UpdateTextCoins(CoinsManager.Coins);
    }

    private void OnEnable()
    {
        CoinsManager.OnCoinsChanged += UpdateTextCoins;
    }

    private void OnDisable()
    {
        CoinsManager.OnCoinsChanged -= UpdateTextCoins;
    }

    private void UpdateTextCoins(int newCoins)
    {
        coinsText.text = cPrefix + newCoins.ToString();
    }
}