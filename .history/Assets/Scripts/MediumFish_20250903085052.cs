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
    private Transform fleeTarget; // con cá lớn gần nhất

    void Start()
    {
        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        Fish biggerFish = FindNearestBiggerFish();
        Vector3 moveDir = Vector3.zero;

        if (biggerFish != null)
        {
            float distance = Vector2.Distance(transform.position, biggerFish.transform.position);

            if (distance <= fleeRadius)
            {
                // bắt đầu chạy
                fleeTarget = biggerFish.transform;
                fleeTimer = extraFleeTime;
                moveDir = (transform.position - fleeTarget.position).normalized;
            }
            else if (fleeTarget != null && fleeTimer > 0f)
            {
                // vẫn chạy thêm dù cá lớn đã đi xa
                moveDir = (transform.position - fleeTarget.position).normalized;
                fleeTimer -= Time.deltaTime;
            }
        }
        else if (fleeTarget != null && fleeTimer > 0f)
        {
            // vẫn chạy thêm nếu không tìm thấy cá lớn nữa
            moveDir = (transform.position - fleeTarget.position).normalized;
            fleeTimer -= Time.deltaTime;
        }
        else if (biggerFish == null)
        {
            // không bị đe dọa → kiểm tra chase player
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Fish playerFish = playerObj.GetComponent<Fish>();
                float distance = Vector2.Distance(transform.position, playerObj.transform.position);

                if (distance <= chaseRadius && selfFish.size > playerFish.size)
                {
                    moveDir = (playerObj.transform.position - transform.position).normalized;
                }
            }

            // nếu không chase → di chuyển wave
            if (moveDir == Vector3.zero)
            {
                float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
                moveDir = new Vector3(direction, waveY, 0f).normalized;
            }
        }

        // di chuyển
        transform.position += moveDir * speed * Time.deltaTime;

        // cập nhật flip và tilt
        UpdateVisual(moveDir);
    }

    Fish FindNearestBiggerFish()
    {
        Fish[] allFish = FindObjectsOfType<Fish>();
        Fish nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var f in allFish)
        {
            if (f == selfFish) continue; // bỏ qua bản thân
            if (f.size <= selfFish.size) continue; // chỉ quan tâm cá lớn hơn

            float dist = Vector2.Distance(transform.position, f.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
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
