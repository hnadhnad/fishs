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

    void Start()
    {
        // thay vì random, khởi tạo theo initialDir
        velocity = new Vector2(initialDir, 0).normalized * speed;

        Fish f = GetComponent<Fish>();
        if (f != null) f.SetSize(initialSize);
    }

    public void SetDirection(int dir)
    {
        initialDir = (int)Mathf.Sign(dir); // đảm bảo -1 hoặc +1
        velocity = new Vector2(initialDir, 0) * speed;

        // quay mặt cá theo hướng
        transform.localScale = new Vector3(initialDir * Mathf.Abs(transform.localScale.x),
                                           transform.localScale.y,
                                           transform.localScale.z);
    }

    void Update()
    {
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
        transform.up = velocity;
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
