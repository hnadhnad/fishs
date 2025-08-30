using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
    public float speed = 2f;
    public float neighborRadius = 2f;
    public float separationRadius = 1f;

    private Vector2 velocity;

    void Start()
    {
        velocity = Random.insideUnitCircle.normalized * speed;
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
                if (dist < separationRadius)
                {
                    separation += (Vector2)(transform.position - neighbor.transform.position) / dist;
                }
            }

            alignment /= neighbors.Count;
            cohesion /= neighbors.Count;
            cohesion = (cohesion - (Vector2)transform.position);
        }

        Vector2 acceleration = alignment + cohesion + separation;
        velocity += acceleration * Time.deltaTime;
        velocity = velocity.normalized * speed;

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
