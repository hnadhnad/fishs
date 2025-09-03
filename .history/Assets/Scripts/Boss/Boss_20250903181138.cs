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

    [Header("Boss Movement")]
    [Tooltip("Tốc độ di chuyển bình thường (retreat, reentry, v.v.)")]
    public float moveSpeed = 4f;

    [Tooltip("Tốc độ khi dí theo lure")]
    public float chaseLureSpeed = 6f;


    // ================= PHASE 1 =================
    [Header("Phase1 - Dash")]
    [Tooltip("Số lần dash trong 1 vòng tấn công")]
    public int phase1DashCount = 3;

    [Tooltip("Khoảng cách boss dash mỗi lần")]
    public float phase1DashDistance = 6f;

    [Tooltip("Thời gian 1 dash (giây)")]
    public float phase1DashDuration = 0.25f;

    [Tooltip("Thời gian nghỉ giữa các dash")]
    public float phase1DashInterval = 0.5f;

    [Tooltip("Thời gian pause sau khi dash trúng (giả lập lực)")]
    public float phase1DashImpactPause = 0.12f;


    [Header("Phase1 - Shoot")]
    [Tooltip("Prefab đạn của boss")]
    public GameObject phase1BulletPrefab;

    [Tooltip("Số viên đạn bắn trong 1 vòng")]
    public int phase1ShootCount = 3;

    [Tooltip("Khoảng cách spawn đạn tính từ boss về phía player")]
    public float phase1BulletSpawnOffset = 1.5f;

    [Tooltip("Thời gian nghỉ giữa mỗi phát bắn")]
    public float phase1ShootInterval = 0.5f;

    [Tooltip("Tốc độ bay của đạn")]
    public float phase1BulletSpeed = 8f;


    [Header("Phase1 - Cycle Timings")]
    [Tooltip("Thời gian nghỉ giữa dash và shoot")]
    public float phase1CyclePause = 0.4f;


    [Header("Phase1 - Retreat / Lure")]
    [Tooltip("Nếu hunger <= % này của maxHunger → boss sẽ rút lui")]
    [Range(0f, 1f)] public float phase1RetreatHungerFraction = 0.5f;

    [Tooltip("Khoảng boss đi ra ngoài map (theo X) khi retreat")]
    public float phase1ExitDistance = 6f;

    [Tooltip("Prefab lure cá mồi để boss dụ vào map")]
    public GameObject phase1LurePrefab;

    [Tooltip("Tốc độ bơi của lure")]
    public float phase1LureSpeed = 2.5f;

    [Tooltip("Khoảng spawn lure trước mặt boss (theo trục X)")]
    public float phase1LureSpawnForward = 1.5f;

    [Tooltip("Lượng hunger mà boss hồi khi ăn lure")]
    public float phase1LureHealAmount = 150f;


    [Header("Phase1 - After Lure")]
    [Tooltip("Số lần dash sau khi ăn lure")]
    public int phase1PostLureDashCount = 2;

    [Tooltip("Số lần bắn sau khi ăn lure")]
    public int phase1PostLureShootCount = 5;

    [Tooltip("Thời gian nghỉ sau khi ăn lure trước khi tiếp tục vòng mới")]
    public float phase1AfterLurePause = 0.5f;

    // -------- Phase 2 (Thả bom) -------------
    [Header("Phase2 - Chase")]
    [Tooltip("Hệ số nhân tốc độ boss khi dí player trong Phase2")]
    public float phase2ChaseMultiplier = 1.2f;

    [Header("Phase2 - Bomb/Volley")]
    [Tooltip("Prefab bom (attach FallingBomb)")]
    public GameObject phase2BombPrefab;

    [Tooltip("Số bom mỗi volley (thường 5)")]
    public int phase2BombCount = 5;

    [Tooltip("Khoảng thời gian giữa 2 quả bom trong vòng volley")]
    public float phase2BombInterval = 0.6f;

    [Tooltip("Thời gian boss đợi giữa 2 volley")]
    public float phase2VolleyPause = 1.2f;

    [Tooltip("Số quả cuối cùng trong volley khiến boss mệt đứng yên")]
    public int phase2FinalBombsThatExhaust = 2;

    [Header("Phase2 - Bomb flight")]
    [Tooltip("Độ cao spawn bom so với mép trên map")]
    public float phase2BombSpawnHeight = 6f;

    [Tooltip("Thời gian rơi của bom (giây)")]
    public float phase2BombFallDuration = 0.9f;

    [Tooltip("Bán kính nổ ảnh hưởng (units)")]
    public float phase2BombExplodeRadius = 1.6f;

    [Tooltip("Damage bom gây lên boss nếu trúng")]
    public float phase2BombDamage = 120f;

    [Header("Phase2 - Meat (after explosion)")]
    [Tooltip("Prefab mảnh thịt (là prefab có Fish component)")]
    public GameObject phase2MeatPrefab;

    [Tooltip("Số mảnh thịt spawn khi bom nổ (ví dụ 4 hướng)")]
    public int phase2MeatCount = 4;


    void Awake()
    {
        fish = GetComponent<Fish>();
        animator = GetComponent<Animator>();

        if (bossUIPanel == null)
            bossUIPanel = GameObject.Find("BossUI");

        if (healthBar == null && bossUIPanel != null)
        {
            Transform hb = bossUIPanel.transform.Find("HealthBar");
            if (hb != null)
                healthBar = hb.GetComponent<Slider>();
        }

        if (hungerBar == null && bossUIPanel != null)
        {
            Transform hub = bossUIPanel.transform.Find("HungerBar");
            if (hub != null)
                hungerBar = hub.GetComponent<Slider>();
        }
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

        // Check phase chuyển đổi (giữ nguyên)
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
