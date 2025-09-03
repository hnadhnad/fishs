using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishWave : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float fleeRadius = 5f;   // bán kính bỏ chạy
    public float extraFleeTime = 2f; // thời gian chạy thêm sau khi thoát bán kính

    [Header("Visual Settings")]
    public float maxTiltAngle = 20f; // góc nghiêng tối đa

    [HideInInspector] public int direction = -1;

    private float waveOffset;
    private Fish selfFish;
    private float baseScaleX;

    private float fleeTimer = 0f;
    private Transform fleeTarget;

    void Start()
    {
        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;

        baseScaleX = Mathf.Abs(transform.localScale.x);

        // flip sprite ban đầu theo direction
        UpdateVisual(new Vector3(direction, 0, 0));
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        bool startedFleeThisFrame = false;

        // 1) Tìm cá lớn hơn gần nhất
        Fish biggerFish = FindNearestBiggerFish();

        if (biggerFish != null)
        {
            float dist = Vector2.Distance(transform.position, biggerFish.transform.position);
            if (dist <= fleeRadius)
            {
                fleeTarget = biggerFish.transform;
                fleeTimer = extraFleeTime;
                moveDir = (transform.position - fleeTarget.position).normalized;
                startedFleeThisFrame = true;
            }
        }

        // 2) Nếu không bắt đầu flee mới nhưng còn thời gian flee → chạy tiếp
        if (!startedFleeThisFrame)
        {
            if (fleeTarget != null && fleeTimer > 0f)
            {
                moveDir = (transform.position - fleeTarget.position).normalized;
                fleeTimer -= Time.deltaTime;

                if (fleeTarget == null) fleeTimer = 0f; // target bị destroy
            }
            else
            {
                // 3) Bình thường thì bơi sóng
                float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
                moveDir = new Vector3(direction, waveY, 0f).normalized;
            }
        }

        // Di chuyển
        transform.position += moveDir * speed * Time.deltaTime;

        // Cập nhật hiển thị
        UpdateVisual(moveDir);
    }

    // Tìm cá lớn hơn gần nhất trong bán kính
    Fish FindNearestBiggerFish()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fleeRadius);
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
