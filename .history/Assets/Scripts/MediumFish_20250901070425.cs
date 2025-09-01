using UnityEngine;

[RequireComponent(typeof(Fish))]
public class MediumFish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 5f;
    public float fleeRadius = 5f;   // bán kính bỏ chạy

    [HideInInspector] public int direction = -1;

    private Transform player;
    private float waveOffset;
    private Fish selfFish;    // cá hiện tại
    private Fish playerFish;  // cá người chơi

    void Start()
    {
        selfFish = GetComponent<Fish>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFish = playerObj.GetComponent<Fish>();
        }

        waveOffset = Random.value * Mathf.PI * 2f;

        // quay sprite ban đầu theo direction
        FlipSprite(direction);
    }

    void Update()
    {
        if (player == null || playerFish == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        Vector3 moveDir = Vector3.zero;

        // --- Nếu player to hơn -> bỏ chạy ---
        if (distance <= fleeRadius && playerFish.size > selfFish.size)
        {
            moveDir = (transform.position - player.position).normalized;
        }
        // --- Nếu MediumFish to hơn -> dí ---
        else if (distance <= chaseRadius && selfFish.size > playerFish.size)
        {
            moveDir = (player.position - transform.position).normalized;
        }
        // --- Không có player trong phạm vi -> wave ---
        else
        {
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            moveDir = new Vector3(direction, waveY, 0f).normalized;
        }

        transform.position += moveDir * speed * Time.deltaTime;

        // quay mặt theo hướng x
        FlipSprite(moveDir.x);
    }

    void FlipSprite(float dirX)
    {
        if (Mathf.Abs(dirX) < 0.001f) return; // tránh lỗi khi chỉ di chuyển lên xuống

        if (dirX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x),
                                               transform.localScale.y,
                                               transform.localScale.z);
        }
        else if (dirX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x),
                                               transform.localScale.y,
                                               transform.localScale.z);
        }
    }
}
