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
    private Fish fish;

    void Start()
    {
        fish = GetComponent<Fish>(); // đảm bảo có Fish component
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        waveOffset = Random.value * Mathf.PI * 2f;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= chaseRadius)
        {
            // rượt theo player
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * speed * Time.deltaTime;
        }
        else
        {
            // bơi kiểu wave
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            Vector3 moveDir = new Vector3(-1f, waveY, 0f).normalized;
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }
}
