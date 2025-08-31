using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishStraight : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 direction = Vector2.right;
    public float initialSize = 0.9f;

    // --- thêm mới ---
    public float fleeRadius = 5f; // bán kính khi gặp player sẽ bỏ chạy
    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

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
                direction = fleeDir; // cập nhật hướng để tiếp tục chạy xa hơn
                if (!IsVisible()) Destroy(gameObject);
                return; // bỏ qua code di chuyển bình thường
            }
        }

        // --- di chuyển bình thường ---
        transform.Translate(direction.normalized * speed * Time.deltaTime);

        if (!IsVisible())
            Destroy(gameObject);
    }

    bool IsVisible()
    {
        if (Camera.main == null) return true;
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewPos.x > -0.1f && viewPos.x < 1.1f && viewPos.y > -0.1f && viewPos.y < 1.1f);
    }
}
