using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Fish))]
public class Boid : MonoBehaviour
{
    public float speed = 2f;
    public float neighborRadius = 2f;
    public float separationRadius = 1f;
    public float initialSize = 0.8f;

    [Range(0f, 1f)] public float verticalInfluence = 0.3f;
    public float horizontalBias = 0.5f;

    [HideInInspector] public Vector2 velocity;
    private int initialDir = 1; // +1 = sang phải, -1 = sang trái

    // --- thêm mới ---
    public float fleeRadius = 5f;
    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

    void Start()
    {
        velocity = new Vector2(initialDir, 0).normalized * speed;

        selfFish = GetComponent<Fish>();
        if (selfFish != null) selfFish.SetSize(initialSize);

        // tìm player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFish = playerObj.GetComponent<Fish>();
        }
    }

    public void SetDirection(int dir)
    {
        initialDir = (int)Mathf.Sign(dir); // đảm bảo -1 hoặc +1
        velocity = new Vector2(initialDir, 0) * speed;

        // flip sprite cá
        FlipSprite(initialDir);
    }

    void Update()
    {
        // --- logic bỏ chạy ---
        if (player != null && playerFish != null && selfFish != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= fleeRadius && playerFish.size > selfFish.size)
            {
                Vector2 fleeDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
                velocity = fleeDir * speed * 1.5f; // chạy nhanh hơn
                transform.position += (Vector3)(velocity * Time.deltaTime);

                // flip sprite theo hướng chạy
                FlipSprite(velocity.x);
                return; // bỏ qua logic boid thường
            }
        }

        // --- logic boid bình thường ---
        List<Boid> neighbors = GetNeighbors();

        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 separation = Vector2.zero;

        if (neighbors.Count > 0)
        {
            foreach (Boid neighbor in neighbors)
            {
                alignment += neighbor.velocity;
                cohesion += (Vector2)neighbor.transform.position;

                float dist = Vector2.Distance(transform.position, neighbor.transform.position);
                if (dist < separationRadius && dist > 0.001f)
                {
                    separation += (Vector2)(transform.position - neighbor.transform.position) / dist;
                }
            }

            alignment /= neighbors.Count;
            cohesion /= neighbors.Count;
            cohesion = (cohesion - (Vector2)transform.position);
        }

        alignment.y *= verticalInfluence;
        cohesion.y *= verticalInfluence;
        separation.y *= verticalInfluence;

        Vector2 keepHorizontal = new Vector2(Mathf.Sign(velocity.x), 0) * horizontalBias;

        Vector2 acceleration = alignment + cohesion + separation + keepHorizontal;
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * speed;

        float maxAngle = 30f;
        float angle = Vector2.Angle(Vector2.right * Mathf.Sign(velocity.x), velocity);
        if (angle > maxAngle)
        {
            velocity = Vector2.Lerp(velocity, new Vector2(Mathf.Sign(velocity.x), 0), 0.5f).normalized * speed;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);

        // flip sprite theo hướng bơi
        FlipSprite(velocity.x);
    }

    void FlipSprite(float dirX)
    {
        if (dirX > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (dirX < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    List<Boid> GetNeighbors()
    {
        List<Boid> neighbors = new List<Boid>();
        foreach (Boid boid in FindObjectsOfType<Boid>())
        {
            if (boid == this) continue;
            if (Vector2.Distance(transform.position, boid.transform.position) < neighborRadius)
                neighbors.Add(boid);
        }
        return neighbors;
    }
}
