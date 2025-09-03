using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Fish))]
public class Boss : MonoBehaviour
{
    private Fish fish;

    [Header("Boss Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;

    public float maxHunger = 500f;
    public float currentHunger;

    [Header("Decay Settings")]
    public float hungerDecayRate = 5f;   // tốc độ đói giảm theo giây
    public float hungerDamageRate = 10f; // tốc độ trừ máu/giây khi đói = 0

    [Header("UI References")]
    public GameObject bossUIPanel;   // gắn BossUI ở Canvas
    public Slider healthBar;         // gắn HealthBar Slider
    public Slider hungerBar;         // gắn HungerBar Slider

    [Header("Phase Settings")]
    public BossPhase currentPhase = BossPhase.Phase1;

    void Awake()
    {
        fish = GetComponent<Fish>();
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        // Lúc boss spawn -> bật UI
        if (bossUIPanel != null) bossUIPanel.SetActive(true);
    }

    void Update()
    {
        // Giảm đói theo thời gian
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        // Nếu hết đói thì máu giảm dần
        if (currentHunger <= 0)
        {
            currentHealth -= hungerDamageRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        // Update UI
        if (healthBar != null) healthBar.value = currentHealth;
        if (hungerBar != null) hungerBar.value = currentHunger;

        HandlePhaseLogic();

        // Nếu chết
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void ChangeHunger(float amount)
    {
        currentHunger += amount;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);
    }

    void HandlePhaseLogic()
    {
        if (currentHealth <= maxHealth * 0.7f && currentPhase == BossPhase.Phase1)
        {
            SwitchPhase(BossPhase.Phase2);
        }
        else if (currentHealth <= maxHealth * 0.4f && currentPhase == BossPhase.Phase2)
        {
            SwitchPhase(BossPhase.Phase3);
        }
        else if (currentHealth <= maxHealth * 0.1f && currentPhase == BossPhase.Phase3)
        {
            SwitchPhase(BossPhase.Enraged);
        }
    }

    void SwitchPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log("Boss switched to phase: " + newPhase);

        switch (newPhase)
        {
            case BossPhase.Phase2:
                // Ví dụ tăng tốc
                break;
            case BossPhase.Phase3:
                // Spawn minions
                break;
            case BossPhase.Enraged:
                // Tăng damage
                break;
        }
    }
}

public enum BossPhase
{
    Phase1,
    Phase2,
    Phase3,
    Enraged
}
