using UnityEngine;

[RequireComponent(typeof(Fish))]
[RequireComponent(typeof(Collider2D))]
public class LureFish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;               // tốc độ bơi
    public Vector2 moveDirection = Vector2.right; // hướng di chuyển mặc định (X/Y)

    [Header("Rotation Settings")]
    public float maxTiltAngle = 15f;       // góc nghiêng tối đa khi bơi

    private Fish selfFish;
    private float baseScaleX;

    void Start()
    {
        selfFish = GetComponent<Fish>();
        baseScaleX = Mathf.Abs(transform.localScale.x);

        if (moveDirection == Vector2.zero)
            moveDirection = Vector2.right; // tránh 0 vector

        // Collider để ăn lure phải là trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Update()
    {
        // Di chuyển theo đường thẳng
        transform.position += (Vector3)moveDirection.normalized * speed * Time.deltaTime;

        // Cập nhật hình ảnh (flip + tilt)
        UpdateVisual(moveDirection);

        // Nếu lure bơi ra ngoài màn → tự hủy
        MapManager map = GameObject.FindObjectOfType<MapManager>();
        if (map != null)
        {
            if (transform.position.x < map.bottomLeft.x - 1f ||
                transform.position.x > map.topRight.x + 1f)
            {
                Destroy(gameObject);
            }
        }
    }

    void UpdateVisual(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 0.001f && Mathf.Abs(dir.y) < 0.001f) return;

        float signX = Mathf.Sign(dir.x == 0 ? 1 : dir.x);
        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        float tilt = Mathf.Clamp(dir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        tilt *= signX;

        transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Boss boss = other.GetComponent<Boss>();
        if (boss != null)
        {
            // Boss ăn lure → hồi Hunger
            boss.currentHunger = Mathf.Min(boss.currentHunger + boss.phase1LureHealAmount, boss.maxHunger);
            Destroy(gameObject);
        }
    }
}
