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
    private Transform fleeTarget; // con cá lớn gần nhất từng gây flee

    void Start()
    {
        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;

        // 1) tìm con cá lớn gần nhất (nếu có)
        Fish biggerFish = FindNearestBiggerFish();

        bool startedFleeThisFrame = false;

        // 2) Nếu có con cá lớn gần đó và trong bán kính flee -> bắt đầu chạy
        if (biggerFish != null)
        {
            float distToBigger = Vector2.Distance(transform.position, biggerFish.transform.position);
            if (distToBigger <= fleeRadius)
            {
                fleeTarget = biggerFish.transform;
                fleeTimer = extraFleeTime;        // reset thời gian chạy thêm
                moveDir = (transform.position - fleeTarget.position).normalized;
                startedFleeThisFrame = true;
            }
        }

        // 3) Nếu không bắt đầu flee mới, nhưng đang trong thời gian fleeTimer thì vẫn chạy tiếp
        if (!startedFleeThisFrame)
        {
            if (fleeTarget != null && fleeTimer > 0f)
            {
                // vẫn chạy tiếp về hướng rời khỏi fleeTarget
                moveDir = (transform.position - fleeTarget.position).normalized;
                fleeTimer -= Time.deltaTime;

                // optional: nếu fleeTarget bị destroy thì clear
                if (fleeTarget == null) fleeTimer = 0f;
            }
            else
            {
                // 4) Không bị đe doạ → check chase player rồi wave
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

                // 5) Nếu vẫn không có hướng di chuyển → wave patrol
                if (moveDir == Vector3.zero)
                {
                    float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
                    moveDir = new Vector3(direction, waveY, 0f).normalized;
                }
            }
        }

        // Di chuyển và cập nhật hình
        transform.position += moveDir * speed * Time.deltaTime;
        UpdateVisual(moveDir);
    }

    // Tìm cá lớn nhất gần nhất (trả về Fish hoặc null)
    Fish FindNearestBiggerFish()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fleeRadius * 2f); 
        // *2f để có dư một chút, bạn muốn chặt thì để đúng fleeRadius cũng được

        Fish nearest = null;
        float minDist = Mathf.Infinity;

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
        }

        return nearest;
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
