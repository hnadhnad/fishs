using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public float speed = 5f; // tốc độ bơi
    private Rigidbody2D rb;
    public float stopDistance = 0.1f;

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

        // đảm bảo collider trigger
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
        moveDir.Normalize();

        if (distance > stopDistance)
        {
            Vector2 newPos = rb.position + moveDir * speed * Time.fixedDeltaTime;

            float minX = mapManager.bottomLeft.x;
            float maxX = mapManager.topRight.x;
            float minY = mapManager.bottomLeft.y;
            float maxY = mapManager.topRight.y;

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            rb.MovePosition(newPos);

            // cập nhật flip + tilt
            UpdateVisual(moveDir);
        }
        else
        {
            rb.velocity = Vector2.zero;

            // khi đứng yên → trở lại thẳng đứng
            UpdateVisual(Vector2.zero);
        }
    }

    void UpdateVisual(Vector2 moveDir)
    {
        float currentSize = (fish != null) ? fish.size : 1f;

        if (moveDir == Vector2.zero)
        {
            // đứng yên → xoay thẳng đứng
            Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, tiltLerpSpeed * Time.deltaTime);

            // scale đồng đều (không flip khi không có hướng)
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

        // tilt theo hướng di chuyển y
        float tiltAngle = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        tiltAngle *= signX;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, tiltAngle);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, tiltLerpSpeed * Time.deltaTime);
    }

}
