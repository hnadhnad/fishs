using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishStraight : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 direction = Vector2.right;
    public float initialSize = 0.9f;

    public float fleeRadius = 5f; // bán kính khi gặp player sẽ bỏ chạy

    [Header("Visual Settings")]
    public float maxTiltAngle = 20f; // góc nghiêng tối đa khi chạy

    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

    private float baseScaleX;

    Fish fish;

    void Start()
    {
        fish = GetComponent<Fish>();
        if (fish != null)
        {
            fish.SetSize(initialSize);
            selfFish = fish;
        }

        // tìm player theo tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFish = playerObj.GetComponent<Fish>();
        }

        baseScaleX = Mathf.Abs(transform.localScale.x);

        // flip sprite ban đầu theo direction
        UpdateVisual(direction);
    }

    void Update()
    {
        // --- kiểm tra bỏ chạy ---
        if (player != null && playerFish != null && selfFish != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // nếu player trong bán kính và to hơn thì bỏ chạy
            if (distance <= fleeRadius && playerFish.size > selfFish.size)
            {
                Vector2 fleeDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
                transform.Translate(fleeDir * speed * 1.5f * Time.deltaTime); // chạy nhanh hơn bình thường

                // quay mặt + tilt theo hướng bỏ chạy
                UpdateVisual(fleeDir);

                if (!IsVisible()) Destroy(gameObject);
                return; // bỏ qua code di chuyển bình thường
            }
        }

        // --- di chuyển bình thường ---
        transform.Translate(direction.normalized * speed * Time.deltaTime);

        // chỉ flip theo X, không tilt
        UpdateVisual(direction, tilt: false);

        if (!IsVisible())
            Destroy(gameObject);
    }

    void UpdateVisual(Vector2 moveDir, bool tilt = true)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);

        // flip sprite
        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        if (tilt)
        {
            // tilt theo hướng y
            float tiltAngle = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
            tiltAngle *= signX;
            transform.localRotation = Quaternion.Euler(0f, 0f, tiltAngle);
        }
        else
        {
            // giữ thẳng khi bơi bình thường
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
