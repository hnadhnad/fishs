using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Fish))]
public class BigFish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float normalSpeed = 2f;
    public float chargeSpeed = 6f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 6f;

    [Header("Charge Settings (chỉ với Player)")]
    public float chargeUpTime = 1f;     
    public float chargeDuration = 2f;   

    [Header("Flee Settings")]
    public float fleeRadius = 6f;        
    public float extraFleeTime = 2f;     

    [Header("Hunt Other Fish")]
    [Range(0f, 1f)] public float huntChance = 0.3f; 
    public float huntDuration = 3f;   // thời gian đuổi tối đa 1 cá thường
    private float huntTimer = 0f;
    private Transform huntTarget;

    [Header("Visual Settings")]
    public float maxTiltAngle = 20f;    

    [HideInInspector] public int direction = -1; 

    private Transform player;
    private Fish playerFish;
    private Fish selfFish;

    private float waveOffset;
    private bool charging = false;
    private float chargeTimer = 0f;
    private Vector3 chargeDir;

    private float fleeTimer = 0f;
    private float baseScaleX;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player != null) playerFish = player.GetComponent<Fish>();

        selfFish = GetComponent<Fish>();
        waveOffset = Random.value * Mathf.PI * 2f;

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (player == null || playerFish == null || selfFish == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // --- Ưu tiên: bỏ chạy nếu player to hơn ---
        if (playerFish.size > selfFish.size)
        {
            if (distanceToPlayer <= fleeRadius) fleeTimer = extraFleeTime;

            if (fleeTimer > 0f)
            {
                fleeTimer -= Time.deltaTime;
                Vector3 fleeDir = (transform.position - player.position).normalized;
                transform.position += fleeDir * normalSpeed * Time.deltaTime;
                UpdateVisual(fleeDir);
                return;
            }
        }

        // --- Nếu đang dash player ---
        if (charging)
        {
            HandleCharge();
            return;
        }

        // --- Nếu player nhỏ hơn và trong bán kính -> dash ---
        if (selfFish.size > playerFish.size && distanceToPlayer <= chaseRadius)
        {
            StartCharge(player.position);
            return;
        }

        // --- Nếu đang săn cá thường ---
        if (huntTarget != null)
        {
            huntTimer -= Time.deltaTime;

            if (huntTimer > 0f && huntTarget != null)
            {
                Vector3 dir = (huntTarget.position - transform.position).normalized;
                transform.position += dir * normalSpeed * Time.deltaTime;
                UpdateVisual(dir);
                return;
            }
            else
            {
                huntTarget = null; // hết thời gian thì bỏ
            }
        }

        // --- Wave khi không làm gì đặc biệt ---
        SwimWave();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (huntTarget != null) return; // đang săn thì bỏ qua

        Fish f = other.GetComponent<Fish>();
        if (f == null || f == selfFish) return;

        // Ưu tiên Player
        if (f == playerFish && selfFish.size > playerFish.size)
        {
            StartCharge(player.position);
            return;
        }

        // Cá thường
        if (f.size < selfFish.size)
        {
            if (Random.value < huntChance)
            {
                huntTarget = f.transform;
                huntTimer = huntDuration;
            }
        }
    }

    void SwimWave()
    {
        float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
        Vector3 moveDir = new Vector3(direction, waveY, 0f).normalized;
        transform.position += moveDir * normalSpeed * Time.deltaTime;
        UpdateVisual(moveDir);
    }

    void StartCharge(Vector3 targetPos)
    {
        charging = true;
        chargeTimer = -chargeUpTime;
        chargeDir = (targetPos - transform.position).normalized;
    }

    void HandleCharge()
    {
        if (chargeTimer < 0f)
        {
            chargeTimer += Time.deltaTime;
        }
        else if (chargeTimer < chargeDuration)
        {
            transform.position += chargeDir * chargeSpeed * Time.deltaTime;
            chargeTimer += Time.deltaTime;
            UpdateVisual(chargeDir);
        }
        else
        {
            charging = false;
        }
    }

    void UpdateVisual(Vector3 moveDir)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        float signX = Mathf.Sign(moveDir.x);

        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        float tilt = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);
        tilt *= signX;

        transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
