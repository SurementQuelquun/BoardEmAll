using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerUI : MonoBehaviour
{
    [SerializeField]
    private string HealthElementName = "healthText";

    [SerializeField]
    private string Prefix = "Health: ";

    private UIDocument uiDocument;
    private Label healthText;

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        healthText = root.Q<Label>(HealthElementName) ?? root.Q<Label>();
        UpdateText(HealthManager.Health);
    }

    private void OnEnable()
    {
        HealthManager.OnHealthChanged += UpdateText;
    }

    private void OnDisable()
    {
        HealthManager.OnHealthChanged -= UpdateText;
    }

    private void UpdateText(int newHealth)
    {
        healthText.text = Prefix + newHealth.ToString();
    }
}