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
    }

    void Update()
    {
        if (player == null || playerFish == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // --- Nếu player to hơn -> bỏ chạy ---
        if (distance <= fleeRadius && playerFish.size > selfFish.size)
        {
            Vector3 fleeDir = (transform.position - player.position).normalized;
            transform.position += fleeDir * speed * Time.deltaTime;
        }
        // --- Nếu MediumFish to hơn -> dí ---
        else if (distance <= chaseRadius && selfFish.size > playerFish.size)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        // --- Không có player trong phạm vi -> wave ---
        else
        {
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            Vector3 moveDir = new Vector3(direction, waveY, 0f).normalized;
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }
}
