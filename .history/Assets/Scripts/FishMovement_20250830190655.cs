using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public float speed = 5f; // tốc độ bơi
    private Rigidbody2D rb;

    public float stopDistance = 0.1f;

    private MapManager mapManager; // tham chiếu tới MapManager

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Tìm MapManager trong scene
        mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogError("Không tìm thấy MapManager trong scene!");
        }
    }

    void FixedUpdate()
    {
        if (mapManager == null) return;

        // Lấy vị trí chuột theo toạ độ thế giới
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Vector hướng từ cá tới chuột
        Vector2 direction = (mouseWorldPos - transform.position);
        float distance = direction.magnitude;
        direction.Normalize();

        if (distance > stopDistance)
        {
            // Vị trí mới
            Vector2 newPos = rb.position + direction * speed * Time.fixedDeltaTime;

            // Clamp vị trí trong giới hạn bản đồ
            float minX = mapManager.bottomLeft.x;
            float maxX = mapManager.topRight.x;
            float minY = mapManager.bottomLeft.y;
            float maxY = mapManager.topRight.y;

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            rb.MovePosition(newPos);

            // Xoay mặt cá theo hướng bơi
            if (direction.x != 0)
            {
                Vector3 localScale = transform.localScale;
                localScale.x = direction.x > 0 ? Mathf.Abs(localScale.x) : -Mathf.Abs(localScale.x);
                transform.localScale = localScale;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
}
