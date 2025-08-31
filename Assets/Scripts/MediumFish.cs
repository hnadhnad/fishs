using UnityEngine;

[RequireComponent(typeof(Fish))]
public class MediumFish : MonoBehaviour
{
    public float speed = 2f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 5f;

    private Transform player;
    private float waveOffset;
    private Fish fish;        // cá hiện tại
    private Fish playerFish;  // cá người chơi
    public int direction = -1;
    void Start()
    {
        fish = GetComponent<Fish>();
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

        if (distance <= chaseRadius && fish.size > playerFish.size)
        {
            // rượt theo player
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            // 👇 SỬA: dùng direction thay vì luôn -1
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            Vector3 moveDir = new Vector3(direction, waveY, 0f).normalized;
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }
}
