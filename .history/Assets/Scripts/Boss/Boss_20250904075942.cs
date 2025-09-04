using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    // ================= PHASE 2 =================
    [Header("Phase2 - Chase & Bomb")]
    public float phase2ChaseMultiplier = 1.2f;   // Boss dí player nhanh hơn (x tốc độ gốc)
    public GameObject phase2BombPrefab;          // Prefab bomb
    public int phase2BombPerCycle = 5;           // Mỗi lượt bắn 5 trái bomb
    public float phase2BombInterval = 0.8f;      // Delay giữa các bomb
    public float phase2BombRadius = 3f;          // Bán kính nổ
    public float phase2BombDelay = 1.5f;         // Thời gian cảnh báo trước khi nổ
    public float phase2BombDamage = 200f;        // Boss mất máu nếu dính bomb
    public float phase2BossStunDuration = 2f;    // Thời gian Boss bị choáng sau khi dính bomb
    public float phase2PreShootDelay = 0.3f;


    [Header("Phase2 - Meat drop")]
    public GameObject meatPrefab;             // Prefab miếng thịt
    public int phase2MeatCount = 4;           // Số lượng thịt rơi
    public float phase2MeatSpawnOffset = 1f;  // Khoảng cách spawn thịt so với boss
    public float phase2MeatScatterSpeed = 3f; // Tốc độ thịt bay ra ngoài

    public float phase2EatMeatSpeed = 3f;        // Tốc độ boss di chuyển để ăn thịt

    // Boss bị stun timer
    private float stunTimer = 0f;
    public bool IsStunned => stunTimer > 0f;

    private bool isInvulnerable = false;

    // ================= PHASE 3 =================
    [Header("Phase3 - Circle Bombs")]
    public GameObject phase3BombPrefab;       // prefab bomb dùng để tạo 'tường' vòng
    public int phase3BombCount = 12;          // tổng số bomb tạo vòng
    public float phase3CircleRadius = 4f;     // bán kính vòng tròn bomb
    public float phase3BombMoveDuration = 1.2f; // thời gian bomb di chuyển từ ngoại vi vào vị trí vòng
    public float phase3BombSpawnOffscreen = 2.5f; // offset spawn ngoài map (bao xa ngoài edge)
    public float phase3ColumnHeight = 6f;     // chiều cao cột spawn để trải dọc

    [Header("Phase3 - Boss Dash (riêng)")]
    public float phase3DashDistance = 6f;
    public float phase3DashDuration = 0.25f;
    public float phase3DashInterval = 0.25f;
    public float phase3BombCollisionThreshold = 0.6f; // khoảng cách coi là "đâm trúng" bomb
    public float phase3BombStunDuration = 2.0f; // boss bị choáng khi đâm trúng
    public float phase3BombHitDamage = 200f;    // lượng máu boss mất khi đâm trúng (để quy ra meat)
    public int phase3MeatCount = 4;             // số miếng thịt spawn khi boss đâm trúng
    public float phase3MeatSpawnOffset = 0.8f;  // khoảng cách spawn miếng thịt quanh boss (bên trong vòng)
    public float phase3RestAfterMeat = 1.0f;    // thời gian boss nghỉ sau ăn hết thịt




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
        // Nếu chưa set trong Inspector thì mặc định full máu/đói
        if (currentHealth <= 0) currentHealth = maxHealth;
        if (currentHunger <= 0) currentHunger = maxHunger;

        if (healthBar != null) healthBar.maxValue = maxHealth;
        if (hungerBar != null) hungerBar.maxValue = maxHunger;

        if (healthBar != null) healthBar.value = currentHealth;
        if (hungerBar != null) hungerBar.value = currentHunger;

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

                // 🔥 Giảm stunTimer theo thời gian
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer < 0f) stunTimer = 0f;
        }


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
    public void TakeDamage(float dmg, float stunDuration)
    {
        // Nếu đang stun và invulnerable thì bỏ qua dame mới
        if (isInvulnerable) return;

        currentHealth = Mathf.Max(0, currentHealth - dmg);
        Stun(stunDuration);

        if (currentHealth <= 0) Die();
    }

    public void Stun(float duration)
    {
        stunTimer = duration;
        isInvulnerable = true;
        StartCoroutine(ClearInvulnerability(duration));
    }

    private IEnumerator ClearInvulnerability(float duration)
    {
        yield return new WaitForSeconds(duration);
        isInvulnerable = false;
    }




    void Die()
    {
        if (bossUIPanel != null)
            bossUIPanel.SetActive(false);

        Destroy(gameObject);
    }
}
