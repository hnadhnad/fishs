using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishStraight : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public Vector2 direction = Vector2.right;
    public float initialSize = 0.9f;

    [Header("Flee Settings")]
    public float fleeRadius = 5f;       // bán kính phát hiện cá lớn hơn
    public float extraFleeTime = 2f;    // chạy thêm sau khi thoát bán kính

    [Header("Visual Settings")]
    public float maxTiltAngle = 20f;    // góc nghiêng tối đa khi chạy

    private Fish selfFish;
    private float baseScaleX;

    private float fleeTimer = 0f;
    private Transform fleeTarget;

    void Start()
    {
        selfFish = GetComponent<Fish>();
        if (selfFish != null)
        {
            selfFish.SetSize(initialSize);
        }

        baseScaleX = Mathf.Abs(transform.localScale.x);

        // flip ban đầu theo hướng
        UpdateVisual(direction, tilt: false);
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        bool startedFleeThisFrame = false;

        // --- tìm con cá lớn đe dọa ---
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

        // --- vẫn chạy thêm nếu còn extraFleeTime ---
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
                // --- không bị đe dọa → di chuyển bình thường ---
                moveDir = direction.normalized;
                transform.Translate(moveDir * speed * Time.deltaTime);
                UpdateVisual(moveDir, tilt: false);

                if (!IsVisible())
                    Destroy(gameObject);
                return;
            }
        }

        // --- di chuyển khi bỏ chạy ---
        transform.Translate(moveDir * speed * 1.5f * Time.deltaTime);
        UpdateVisual(moveDir, tilt: true);

        if (!IsVisible())
            Destroy(gameObject);
    }

    // Tìm cá lớn hơn trong bán kính, nếu có nhiều thì chọn con xa nhất để hướng chạy ổn định
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

        return farthest != null ? farthest : nearest;
    }

    void UpdateVisual(Vector2 moveDir, bool tilt = true)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);

        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        if (tilt)
        {
            float tiltAngle = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
            tiltAngle *= signX;
            transform.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);
        }
        else
        {
            transform.localRotation = Quaternion.identity;
        }
    }

    bool IsVisible()
    {
        if (Camera.main == null) return true;
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewPos.x > -0.1f && viewPos.x < 1.1f && viewPos.y > -0.1f && viewPos.y < 1.1f);
    }
}
