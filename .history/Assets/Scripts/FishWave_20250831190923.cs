using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishWave : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float amplitude = 0.5f;
    public float frequency = 2f;
    public float fleeRadius = 5f;

    [HideInInspector] public int direction = 1;

    private Vector3 startPos;
    private float waveOffset;
    private Fish selfFish;
    private Transform player;
    private Fish playerFish;

    void Start()
    {
        startPos = transform.position;
        waveOffset = Random.value * Mathf.PI * 2f;

        selfFish = GetComponent<Fish>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFish = playerObj.GetComponent<Fish>();
        }
    }

    void Update()
    {
        if (player != null && playerFish != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // Player to hơn -> bỏ chạy
            if (distance <= fleeRadius && playerFish.size > selfFish.size)
            {
                Vector3 fleeDir = (transform.position - player.position).normalized;
                transform.position += fleeDir * speed * Time.deltaTime;

                if (!IsVisible())
                    Destroy(gameObject);
                return;
            }
        }

        // Nếu không bỏ chạy thì bơi sóng như cũ
        SwimWave();

        if (!IsVisible())
            Destroy(gameObject);
    }

    void SwimWave()
    {
        float waveY = Mathf.Sin(Time.time * frequency + waveOffset) * amplitude;
        Vector3 moveDir = new Vector3(direction, waveY, 0f).normalized;
        transform.position += moveDir * speed * Time.deltaTime;
    }

    bool IsVisible()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewPos.x > -0.1f && viewPos.x < 1.1f && viewPos.y > -0.1f && viewPos.y < 1.1f);
    }
}
