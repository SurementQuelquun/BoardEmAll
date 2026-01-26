using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI Instance { get; private set; }

    [Header("Player HP")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("UI")]
    public Text healthText; // assign a UI Text (uGUI) in the inspector

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        ChangeHealth(-amount);
    }

    public void ChangeHealth(int delta)
    {
        currentHealth = Mathf.Clamp(currentHealth + delta, 0, maxHealth);
        UpdateUI();

        if (currentHealth <= 0)
            OnPlayerDead();
    }

    private void UpdateUI()
    {
        if (healthText != null)
            healthText.text = $"HP: {currentHealth}/{maxHealth}";
    }

    private void OnPlayerDead()
    {
        // TODO: Game over handling (disable spawner, show game over UI, etc.)
        Debug.Log("Player dead - implement game over flow.");
    }
}
