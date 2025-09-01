using UnityEngine;

[RequireComponent(typeof(Fish))]
public class BigFish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 2f;
    public float chargeSpeed = 6f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 6f;

    [Header("Charge Settings")]
    public float chargeUpTime = 1f;     // thời gian đứng yên trước khi lao
    public float chargeDuration = 2f;   // thời gian lao nhanh

    [Header("Flee Settings")]
    public float fleeRadius = 6f;       // bán kính bỏ chạy

    [Header("Visual Settings")]
    public float maxTiltAngle = 20f;    // góc nghiêng tối đa

    [HideInInspector] public int direction = -1; // 1 = qua phải, -1 = qua trái

    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

    private float waveOffset;
    private bool charging = false;
    private float chargeTimer = 0f;
    private Vector3 chargeDir;

    private float baseScaleX;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerFish = player.GetComponent<Fish>();

        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (player == null || playerFish == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Ưu tiên: bỏ chạy khi player to hơn ---
        if (distance <= fleeRadius && playerFish.size > selfFish.size)
        {
            Vector3 fleeDir = (transform.position - player.position).normalized;
            transform.position += fleeDir * normalSpeed * Time.deltaTime;

            UpdateVisual(fleeDir);
            return; // khi đang chạy thì không làm gì khác
        }

        // --- Hành vi gốc: Charge + Wave ---
        if (!charging && distance <= chaseRadius && selfFish.size > playerFish.size)
        {
            // Bắt đầu quy trình charge
            charging = true;
            chargeTimer = -chargeUpTime; // phase âm = charge up (đứng yên)
            chargeDir = (player.position - transform.position).normalized;
        }

        if (charging)
        {
            if (chargeTimer < 0f)
            {
                // đang charge up → đứng yên
                chargeTimer += Time.deltaTime;
            }
            else if (chargeTimer < chargeDuration)
            {
                // đang lao nhanh
                transform.position += chargeDir * chargeSpeed * Time.deltaTime;
                chargeTimer += Time.deltaTime;

                UpdateVisual(chargeDir);
            }
            else
            {
                // reset trạng thái
                charging = false;
            }
        }
        else
        {
            // di chuyển wave bình thường
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            Vector3 moveDir = new Vector3(direction, waveY, 0f).normalized;
            transform.position += moveDir * normalSpeed * Time.deltaTime;

            UpdateVisual(moveDir);
        }
    }

    void UpdateVisual(Vector3 moveDir)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);

        // flip sprite theo hướng x
        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        // tilt theo hướng y, bù flip
        float tilt = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        tilt *= signX;

        transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
