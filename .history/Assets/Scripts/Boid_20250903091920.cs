using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Fish))]
public class Boid : MonoBehaviour
{
    [Header("Movement")]
    public float minSpeed = 1.5f;
    public float maxSpeed = 3.0f;

    private float baseSpeed;   
    public float speed;       

    public float neighborRadius = 2f;
    public float separationRadius = 1f;
    public float initialSize = 0.8f;

    [Range(0f, 1f)] public float verticalInfluence = 0.3f;
    public float horizontalBias = 0.5f;

    [Header("Speed Oscillation")]
    public float speedOscillationAmplitude = 0.3f;  
    public float speedOscillationFrequency = 1.5f;  

    [Header("Visual Settings")]
    public float maxTiltAngle = 25f;
    public float tiltLerpSpeed = 5f;

    [HideInInspector] public Vector2 velocity;
    private int initialDir = 1;

    [Header("Flee Settings")]
    public float fleeRadius = 5f;
    public float extraFleeTime = 2f; // chạy thêm sau khi thoát bán kính

    private Fish selfFish;
    private float baseScaleX;

    private float fleeTimer = 0f;
    private Transform fleeTarget;

    void Start()
    {
        baseSpeed = Random.Range(minSpeed, maxSpeed);
        speed = baseSpeed;

        velocity = new Vector2(initialDir, 0).normalized * speed;

        selfFish = GetComponent<Fish>();
        if (selfFish != null) selfFish.SetSize(initialSize);

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    public void SetDirection(int dir)
    {
        initialDir = (int)Mathf.Sign(dir);
        velocity = new Vector2(initialDir, 0) * speed;
        UpdateVisual(velocity);
    }

    void Update()
    {
        // --- dao động tốc độ ---
        float osc = Mathf.Sin(Time.time * speedOscillationFrequency + GetInstanceID() * 0.1f);
        float oscillation = osc * speedOscillationAmplitude;
        speed = Mathf.Clamp(baseSpeed + oscillation, minSpeed, maxSpeed);

        // --- logic bỏ chạy ---
        Vector2 moveDir = Vector2.zero;
        bool startedFleeThisFrame = false;

        Fish biggerFish = FindNearestBiggerFish();

        if (biggerFish != null)
        {
            float dist = Vector2.Distance(transform.position, biggerFish.transform.position);
            if (dist <= fleeRadius)
            {
                fleeTarget = biggerFish.transform;
                fleeTimer = extraFleeTime;
                moveDir = ((Vector2)transform.position - (Vector2)fleeTarget.position).normalized;
                velocity = moveDir * speed * 1.5f;
                transform.position += (Vector3)(velocity * Time.deltaTime);
                UpdateVisual(velocity);
                return;
            }
        }

        // Nếu không có target mới nhưng còn fleeTimer → chạy tiếp
        if (!startedFleeThisFrame)
        {
            if (fleeTarget != null && fleeTimer > 0f)
            {
                moveDir = ((Vector2)transform.position - (Vector2)fleeTarget.position).normalized;
                velocity = moveDir * speed * 1.5f;
                transform.position += (Vector3)(velocity * Time.deltaTime);
                fleeTimer -= Time.deltaTime;

                if (fleeTarget == null) fleeTimer = 0f;
                UpdateVisual(velocity);
                return;
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

        Vector2 desiredVelocity = (velocity + acceleration).normalized * speed;

        float inertia = 5f; 
        velocity = Vector2.Lerp(velocity, desiredVelocity, inertia * Time.deltaTime);

        float maxAngle = 30f;
        float angle = Vector2.Angle(Vector2.right * Mathf.Sign(velocity.x), velocity);
        if (angle > maxAngle)
        {
            velocity = Vector2.Lerp(velocity, new Vector2(Mathf.Sign(velocity.x), 0), 0.5f).normalized * speed;
        }

        transform.position += (Vector3)(velocity * Time.deltaTime);
        UpdateVisual(velocity);
    }

    Fish FindNearestBiggerFish()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, fleeRadius);
        Fish nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            Fish f = hit.GetComponent<Fish>();
            if (f == null || f == selfFish) continue;
            if (f.size <= selfFish.size) continue;

            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = f;
            }
        }

        return nearest;
    }

    void UpdateVisual(Vector2 moveDir)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);

        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

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
