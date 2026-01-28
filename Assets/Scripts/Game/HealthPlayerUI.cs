using UnityEngine;
using UnityEngine.UIElements;

public class HealthPlayerUI : MonoBehaviour
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
        UpdateTextHealth(HealthManager.Health);
    }

    private void OnEnable()
    {
        HealthManager.OnHealthChanged += UpdateTextHealth;
    }

    private void OnDisable()
    {
        HealthManager.OnHealthChanged -= UpdateTextHealth;
    }

    private void UpdateTextHealth(int newHealth)
    {
        healthText.text = Prefix + newHealth.ToString();
    }

}