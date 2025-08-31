using UnityEngine;

public class MediumFish : MonoBehaviour
{
    public float speed = 2f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 5f;

    private Transform player;
    private Vector3 startPos;
    private float waveOffset;
    private bool chasing = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        startPos = transform.position;
        waveOffset = Random.value * Mathf.PI * 2f; // lệch sóng mỗi con cho tự nhiên
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRadius)
        {
            // Rượt theo
            chasing = true;
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            // Di chuyển kiểu Wave
            chasing = false;
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            Vector3 moveDir = new Vector3(-1f, waveY, 0f).normalized;
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }
}
