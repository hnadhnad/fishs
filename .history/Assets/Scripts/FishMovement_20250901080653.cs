using UnityEngine;

public class FishMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxSpeed = 5f;          // tốc độ tối đa
    public float acceleration = 15f;      // gia tốc khi bơi
    public float deceleration = 10f;      // giảm tốc khi thả
    public float stopDistance = 0.1f;    // khoảng cách coi như dừng

    private Rigidbody2D rb;
    private Vector2 currentVelocity;

    private MapManager mapManager;
    private Fish fish;

    [Header("Visual Settings")]
    public float maxTiltAngle = 25f;   // góc nghiêng tối đa
    public float tiltLerpSpeed = 6f;   // tốc độ xoay mượt

    private float baseScaleX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        // collider trigger
        Collider2D c = GetComponent<Collider2D>();
        if (c == null) c = gameObject.AddComponent<CircleCollider2D>();
        c.isTrigger = true;

        fish = GetComponent<Fish>();
        if (fish == null) fish = gameObject.AddComponent<Fish>();

        mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogError("Không tìm thấy MapManager trong scene!");
        }

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void FixedUpdate()
    {
        if (mapManager == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 moveDir = (mouseWorldPos - transform.position);
        float distance = moveDir.magnitude;

        if (distance > stopDistance)
        {
            moveDir.Normalize();

            // tăng tốc dần về hướng moveDir
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                moveDir * maxSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            // giảm tốc dần về 0
            currentVelocity = Vector2.MoveTowards(
                currentVelocity,
                Vector2.zero,
                deceleration * Time.fixedDeltaTime
            );
        }

        // di chuyển bằng velocity có quán tính
        Vector2 newPos = rb.position + currentVelocity * Time.fixedDeltaTime;

        // clamp trong bản đồ
        float minX = mapManager.bottomLeft.x;
        float maxX = mapManager.topRight.x;
        float minY = mapManager.bottomLeft.y;
        float maxY = mapManager.topRight.y;

        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        rb.MovePosition(newPos);

        // cập nhật flip + tilt
        UpdateVisual(currentVelocity);
    }

    void UpdateVisual(Vector2 moveDir)
    {
        float currentSize = (fish != null) ? fish.size : 1f;

        if (moveDir.magnitude < 0.01f)
        {
            // đứng yên → xoay về 0
            Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, tiltLerpSpeed * Time.deltaTime);

            transform.localScale = new Vector3(baseScaleX * currentSize,
                                            currentSize,
                                            currentSize);
            return;
        }

        float signX = Mathf.Sign(moveDir.x);

        // flip + scale đồng đều
        transform.localScale = new Vector3(signX * baseScaleX * currentSize,
                                        currentSize,
                                        currentSize);

        // --- chỉnh xoay đầu --- 
        float yRatio = moveDir.y / moveDir.magnitude; // tỷ lệ hướng y
        float tiltThreshold = 0.25f; // ngưỡng để bắt đầu nghiêng (0.25 = phải chếch lên ít nhất ~15°)

        float targetTilt = 0f;
        if (Mathf.Abs(yRatio) > tiltThreshold)
        {
            // tính góc nghiêng mượt theo độ mạnh của hướng Y
            float normalizedY = (Mathf.Abs(yRatio) - tiltThreshold) / (1f - tiltThreshold);
            targetTilt = Mathf.Clamp(normalizedY * maxTiltAngle * Mathf.Sign(moveDir.y), -maxTiltAngle, maxTiltAngle);
        }

        targetTilt *= signX; // đảo theo hướng đi ngang

        // xoay dần thay vì nhảy ngay
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, tiltLerpSpeed * Time.deltaTime);
    }

}
