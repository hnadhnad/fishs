using UnityEngine;

public class FishMovement : MonoBehaviour
{
    public float speed = 5f; // tốc độ bơi
    private Rigidbody2D rb;

    // Khoảng cách tối thiểu để cá ngừng lại
    public float stopDistance = 0.1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Lấy vị trí chuột theo toạ độ thế giới
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        // Vector hướng từ cá tới chuột
        Vector2 direction = (mouseWorldPos - transform.position);
        float distance = direction.magnitude; // khoảng cách
        direction.Normalize();

        if (distance > stopDistance)
        {
            // Di chuyển cá về phía chuột
            Vector2 newPos = rb.position + direction * speed * Time.fixedDeltaTime;
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
            // Đủ gần → đứng yên
            rb.velocity = Vector2.zero;
        }
    }
}
