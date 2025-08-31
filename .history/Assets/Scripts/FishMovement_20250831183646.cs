using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public float speed = 5f; // tốc độ bơi
    private Rigidbody2D rb;
    public float stopDistance = 0.1f;

    private MapManager mapManager;
    private Fish fish;

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
    }

    void FixedUpdate()
    {
        if (mapManager == null) return;

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 direction = (mouseWorldPos - transform.position);
        float distance = direction.magnitude;
        direction.Normalize();

        if (distance > stopDistance)
        {
            Vector2 newPos = rb.position + direction * speed * Time.fixedDeltaTime;

            float minX = mapManager.bottomLeft.x;
            float maxX = mapManager.topRight.x;
            float minY = mapManager.bottomLeft.y;
            float maxY = mapManager.topRight.y;

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            rb.MovePosition(newPos);


        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
