using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Fish))]
public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public float maxHealth = 1000f;
    public float currentHealth;

    public float maxHunger = 500f;
    public float currentHunger;

    [Header("Decay Settings")]
    public float hungerDecayRate = 5f;
    public float hungerDamageRate = 10f;

    [Header("UI References")]
    public GameObject bossUIPanel;
    public Slider healthBar;
    public Slider hungerBar;

    [HideInInspector] public Fish fish;
    [HideInInspector] public Animator animator;

    private IBossState currentState;

    // ✅ Thêm các biến Phase1 (KHÔNG ảnh hưởng code gốc)
    [Header("Phase1 - Dash settings")]
    public int phase1DashCount = 3;
    public float phase1DashDistance = 6f;
    public float phase1DashDuration = 0.25f;
    public float phase1DashInterval = 0.5f;
    public float phase1DashImpactPause = 0.12f;

    [Header("Phase1 - Shoot settings")]
    public GameObject phase1BulletPrefab;
    public Transform phase1BulletSpawn;
    public int phase1ShootCount = 3;
    public float phase1ShootInterval = 0.5f;
    public float phase1BulletSpeed = 8f;

    [Header("Phase1 - Cycle timings")]
    public float phase1CyclePause = 0.4f;

    [Header("Phase1 - Retreat / Lure settings")]
    [Tooltip("Phần trăm maxHunger để boss quyết định ra rìa (0.5 = nửa thanh đói)")]
    public float phase1RetreatHungerFraction = 0.5f;
    public float phase1ExitOffset = 2f;
    public float phase1ExitDistance = 6f;
    public float phase1ExitSpeed = 6f;
    public float phase1OutsideWait = 1.0f;
    public GameObject phase1LurePrefab;
    public float phase1LureSpeed = 2.5f;
    public float phase1LureSpawnForward = 1.5f;
    public float phase1ReturnSpeed = 3f;
    public float phase1LureClaimTime = 3.0f;
    public float phase1LureHealAmount = 150f;

    [Header("Phase1 - After lure")]
    public int phase1PostLureDashCount = 2;
    public int phase1PostLureShootCount = 5;
    public float phase1AfterLurePause = 0.5f;

    void Awake()
    {
        fish = GetComponent<Fish>();
        animator = GetComponent<Animator>();

        if (bossUIPanel == null)
            bossUIPanel = GameObject.Find("BossUI");
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentHunger = maxHunger;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        if (bossUIPanel != null)
            bossUIPanel.SetActive(true);

        // ✅ Bắt đầu ở Phase1
        ChangeState(new BossPhase1State());
    }

    void Update()
    {
        // Giảm đói theo thời gian
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        if (currentHunger <= 0)
        {
            currentHealth -= hungerDamageRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        if (healthBar != null) healthBar.value = currentHealth;
        if (hungerBar != null) hungerBar.value = currentHunger;

        // Update logic của state hiện tại
        currentState?.Update(this);

        // Check phase chuyển đổi (GIỮ NGUYÊN Y)
        HandlePhaseLogic();

        if (currentHealth <= 0) Die();
    }

    void HandlePhaseLogic()
    {
        if (currentHealth <= maxHealth * 0.7f && !(currentState is BossPhase2State))
            ChangeState(new BossPhase2State());
        else if (currentHealth <= maxHealth * 0.5f && !(currentState is BossPhase3State))
            ChangeState(new BossPhase3State());
        else if (currentHealth <= maxHealth * 0.2f && !(currentState is BossEnragedState))
            ChangeState(new BossEnragedState());
    }

    public void ChangeState(IBossState newState)
    {
        currentState?.Exit(this);
        currentState = newState;
        currentState.Enter(this);
    }

    void Die()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);

        Destroy(gameObject);
    }
}
