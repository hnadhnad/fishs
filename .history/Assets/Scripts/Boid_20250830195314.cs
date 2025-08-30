using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Fish))]
public class Boid : MonoBehaviour
{
    public float speed = 2f;
    public float neighborRadius = 2f;
    public float separationRadius = 1f;
    public float initialSize = 0.8f;

    [Range(0f, 1f)] public float verticalInfluence = 0.3f; // <--- ảnh hưởng theo trục Y (0 = chỉ ngang, 1 = full 2D)
    public float horizontalBias = 0.5f; // <--- lực ưu tiên đi ngang

    [HideInInspector] public Vector2 velocity;

    void Start()
    {
        velocity = Random.insideUnitCircle.normalized * speed;

        // ép hướng ban đầu thiên về ngang
        velocity.y *= verticalInfluence;  

        Fish f = GetComponent<Fish>();
        if (f != null) f.SetSize(initialSize);
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

        // Áp giảm ảnh hưởng theo trục Y
        alignment.y *= verticalInfluence;
        cohesion.y *= verticalInfluence;
        separation.y *= verticalInfluence;

        // Lực kéo ngang (đảm bảo đàn đi theo X)
        Vector2 keepHorizontal = new Vector2(Mathf.Sign(velocity.x), 0) * horizontalBias;

        Vector2 acceleration = alignment + cohesion + separation + keepHorizontal;
        velocity += acceleration * Time.deltaTime;

        // Chuẩn hóa và scale theo speed
        velocity = velocity.normalized * speed;

        // Giới hạn góc bơi: tránh dốc quá (vd > 30 độ so với trục X)
        float maxAngle = 30f; 
        float angle = Vector2.Angle(Vector2.right * Mathf.Sign(velocity.x), velocity);
        if (angle > maxAngle)
        {
            // ép lại theo hướng ngang
            velocity = Vector2.Lerp(velocity, new Vector2(Mathf.Sign(velocity.x), 0), 0.5f).normalized * speed;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.up = velocity; // xoay cá theo hướng bơi
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
