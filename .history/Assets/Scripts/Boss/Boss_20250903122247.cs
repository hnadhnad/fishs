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
    public GameObject bossUIPanel;   // Panel BossUI
    public Slider healthBar;         // HealthBar Slider
    public Slider hungerBar;         // HungerBar Slider

    [Header("Phase Settings")]
    public BossPhase currentPhase = BossPhase.Phase1;

    void Awake()
    {
        fish = GetComponent<Fish>();

        // 🔥 Tự tìm UI trong Scene nếu chưa gán
        if (bossUIPanel == null)
        {
            bossUIPanel = GameObject.Find("BossUI");
            if (bossUIPanel == null)
                Debug.LogWarning("Không tìm thấy BossUI trong Scene!");
        }

        if (healthBar == null && bossUIPanel != null)
        {
            Transform hb = bossUIPanel.transform.Find("HealthBar");
            if (hb != null)
                healthBar = hb.GetComponent<Slider>();
            else
                Debug.LogWarning("Không tìm thấy HealthBar trong BossUI!");
        }

        if (hungerBar == null && bossUIPanel != null)
        {
            Transform hub = bossUIPanel.transform.Find("HungerBar");
            if (hub != null)
                hungerBar = hub.GetComponent<Slider>();
            else
                Debug.LogWarning("Không tìm thấy HungerBar trong BossUI!");
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        // Bật UI khi boss xuất hiện
        if (bossUIPanel != null)
            bossUIPanel.SetActive(true);
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
            Die();
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
            SwitchPhase(BossPhase.Phase2);
        else if (currentHealth <= maxHealth * 0.5f && currentPhase == BossPhase.Phase2)
            SwitchPhase(BossPhase.Phase3);
        else if (currentHealth <= maxHealth * 0.2f && currentPhase == BossPhase.Phase3)
            SwitchPhase(BossPhase.Enraged);
    }

    void SwitchPhase(BossPhase newPhase)
    {
        currentPhase = newPhase;
        Debug.Log("Boss switched to phase: " + newPhase);

        switch (newPhase)
        {
            case BossPhase.Phase2:
                break;
            case BossPhase.Phase3:
                break;
            case BossPhase.Enraged:
                break;
        }
    }

    void Die()
    {
        // Ẩn UI
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);

        Destroy(gameObject);
    }
}

public enum BossPhase
{
    Phase1,
    Phase2,
    Phase3,
    Enraged
}
