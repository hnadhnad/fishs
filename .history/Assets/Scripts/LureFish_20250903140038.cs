using UnityEngine;

[RequireComponent(typeof(Fish))]
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
    }

    void Update()
    {
        // Di chuyển theo đường thẳng
        transform.position += (Vector3)moveDirection.normalized * speed * Time.deltaTime;

        // Cập nhật hình ảnh (flip + tilt)
        UpdateVisual(moveDirection);
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
}
