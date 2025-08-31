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

    [HideInInspector] public int direction = -1; // 1 = qua phải, -1 = qua trái

    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

    private float waveOffset;
    private bool charging = false;
    private float chargeTimer = 0f;
    private Vector3 chargeDir;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerFish = player.GetComponent<Fish>();

        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (player == null || playerFish == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

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
        }
    }
}
