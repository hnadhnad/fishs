using UnityEngine;

[RequireComponent(typeof(Fish))]
public class MediumFish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 5f;
    public float fleeRadius = 5f;

    [Header("Flee Settings")]
    public float extraFleeTime = 2f; // thời gian chạy thêm sau khi thoát bán kính

    [Header("Rotation Settings")]
    public float maxTiltAngle = 15f; // góc nghiêng tối đa

    [HideInInspector] public int direction = -1;

    private float waveOffset;
    private Fish selfFish;
    private float baseScaleX;

    private float fleeTimer = 0f;
    private Transform fleeTarget; // con cá lớn "đe dọa nhất"

    void Start()
    {
        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        bool startedFleeThisFrame = false;

        // 1) Tìm con cá lớn "đe dọa nhất"
        Fish threatFish = FindThreatFish();

        if (threatFish != null)
        {
            float dist = Vector2.Distance(transform.position, threatFish.transform.position);
            if (dist <= fleeRadius)
            {
                fleeTarget = threatFish.transform;
                fleeTimer = extraFleeTime;
                moveDir = (transform.position - threatFish.transform.position).normalized;
                startedFleeThisFrame = true;
            }
        }

        // 2) Nếu không bắt đầu flee mới nhưng còn thời gian extraFleeTime → tiếp tục chạy
        if (!startedFleeThisFrame)
        {
            if (fleeTarget != null && fleeTimer > 0f)
            {
                moveDir = (transform.position - fleeTarget.position).normalized;
                fleeTimer -= Time.deltaTime;

                if (fleeTarget == null) fleeTimer = 0f;
            }
            else
            {
                // 3) Nếu không bị đe dọa → thử chase player
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    Fish playerFish = playerObj.GetComponent<Fish>();
                    if (playerFish != null)
                    {
                        float distToPlayer = Vector2.Distance(transform.position, playerObj.transform.position);
                        if (distToPlayer <= chaseRadius && selfFish != null && selfFish.size > playerFish.size)
                        {
                            moveDir = (playerObj.transform.position - transform.position).normalized;
                        }
                    }
                }

                // 4) Nếu vẫn không có hướng → wave patrol
                if (moveDir == Vector3.zero)
                {
                    float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
                    moveDir = new Vector3(direction, waveY, 0f).normalized;
                }
            }
        }

        // Di chuyển
        transform.position += moveDir * speed * Time.deltaTime;
        UpdateVisual(moveDir);
    }

    // --- Tìm cá lớn trong bán kính, chọn con xa nhất để chạy hướng an toàn ---
    Fish FindThreatFish()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fleeRadius);
        Fish nearest = null;
        Fish farthest = null;

        float minDist = Mathf.Infinity;
        float maxDist = 0f;

        foreach (var hit in hits)
        {
            Fish f = hit.GetComponent<Fish>();
            if (f == null || f == selfFish) continue;
            if (f.size <= selfFish.size) continue;

            float d = Vector2.Distance(transform.position, f.transform.position);

            if (d < minDist)
            {
                minDist = d;
                nearest = f;
            }

            if (d > maxDist)
            {
                maxDist = d;
                farthest = f;
            }
        }

        // Nếu có nhiều con lớn → ưu tiên chạy tránh con xa nhất (tránh đổi hướng liên tục)
        return farthest != null ? farthest : nearest;
    }

    void UpdateVisual(Vector3 moveDir)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);
        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        float tilt = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        tilt *= signX;

        transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
