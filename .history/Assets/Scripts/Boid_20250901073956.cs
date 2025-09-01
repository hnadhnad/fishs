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

    [Header("Visual Settings")]
    public float maxTiltAngle = 25f; // góc nghiêng tối đa khi bơi
    public float tiltLerpSpeed = 5f; // tốc độ xoay mượt

    [HideInInspector] public Vector2 velocity;
    private int initialDir = 1; // +1 = sang phải, -1 = sang trái

    // --- thêm mới ---
    public float fleeRadius = 5f;
    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

    private float baseScaleX;

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

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    public void SetDirection(int dir)
    {
        initialDir = (int)Mathf.Sign(dir); // đảm bảo -1 hoặc +1
        velocity = new Vector2(initialDir, 0) * speed;

        UpdateVisual(velocity);
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

                // cập nhật flip + tilt
                UpdateVisual(velocity);
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

        // cập nhật flip + tilt theo hướng velocity
        UpdateVisual(velocity);
    }

    void UpdateVisual(Vector2 moveDir)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);

        // flip sprite
        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        // tilt theo hướng y
        float tiltAngle = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        tiltAngle *= signX;

        Quaternion targetRot = Quaternion.Euler(0f, 0f, tiltAngle);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, tiltLerpSpeed * Time.deltaTime);
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
