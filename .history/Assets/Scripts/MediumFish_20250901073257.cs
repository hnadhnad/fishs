using UnityEngine;

[RequireComponent(typeof(Fish))]
public class MediumFish : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float waveAmplitude = 1f;
    public float waveFrequency = 2f;
    public float chaseRadius = 5f;
    public float fleeRadius = 5f;

    [Header("Rotation Settings")]
    public float maxTiltAngle = 15f; // góc nghiêng tối đa

    [HideInInspector] public int direction = -1;

    private Transform player;
    private float waveOffset;
    private Fish selfFish;
    private Fish playerFish;

    private float baseScaleX; // scale gốc để flip

    void Start()
    {
        selfFish = GetComponent<Fish>();
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerFish = playerObj.GetComponent<Fish>();
        }

        waveOffset = Random.value * Mathf.PI * 2f;

        baseScaleX = Mathf.Abs(transform.localScale.x); // scale X gốc
    }

    void Update()
    {
        if (player == null || playerFish == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        Vector3 moveDir = Vector3.zero;

        // --- Nếu player to hơn -> bỏ chạy ---
        if (distance <= fleeRadius && playerFish.size > selfFish.size)
        {
            moveDir = (transform.position - player.position).normalized;
        }
        // --- Nếu MediumFish to hơn -> dí ---
        else if (distance <= chaseRadius && selfFish.size > playerFish.size)
        {
            moveDir = (player.position - transform.position).normalized;
        }
        // --- Không có player trong phạm vi -> bơi sóng ---
        else
        {
            float waveY = Mathf.Sin(Time.time * waveFrequency + waveOffset) * waveAmplitude;
            moveDir = new Vector3(direction, waveY, 0f);
            moveDir.Normalize();
        }

        // di chuyển
        transform.position += moveDir * speed * Time.deltaTime;

        // cập nhật flip và tilt
        UpdateVisual(moveDir);
    }

    void UpdateVisual(Vector3 moveDir)
    {
        if (Mathf.Abs(moveDir.x) < 0.001f) return;

        // flip theo hướng X
        float signX = Mathf.Sign(moveDir.x);
        transform.localScale = new Vector3(signX * baseScaleX,
                                           transform.localScale.y,
                                           transform.localScale.z);

        // tính tilt theo hướng Y (không bị ngược khi flip)
        float tilt = Mathf.Clamp(moveDir.y * maxTiltAngle, -maxTiltAngle, maxTiltAngle);

        // áp tilt lên localRotation
        transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
    }
}
