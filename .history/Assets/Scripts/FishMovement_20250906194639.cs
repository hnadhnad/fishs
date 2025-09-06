using UnityEngine;
using UnityEngine.UI; // thêm namespace UI
using UnityEngine.EventSystems;



public class FishMovement : MonoBehaviour
{
    // movement lock
    private bool movementLocked = false;
    public bool IsMovementLocked => movementLocked;

    [Header("Movement Settings")]
    public float maxSpeed = 5f;
    public float acceleration = 15f;
    public float deceleration = 10f;
    public float stopDistance = 0.1f;

    private Rigidbody2D rb;
    private Vector2 currentVelocity;

    private MapManager mapManager;
    private Fish fish;

    [Header("Visual Settings")]
    public float maxTiltAngle = 25f;
    public float tiltLerpSpeed = 6f;
    private float baseScaleX;

    [Header("Dash Settings")]
    public bool enableDash = false;       // bật/tắt dash
    public float dashForce = 15f;        // lực ban đầu khi dash
    public float dashDuration = 0.2f;    // thời gian dash
    public float dashCooldown = 2f;      // hồi chiêu
    private bool isDashing = false;
    private float dashEndTime;
    private float nextDashTime;

    [Header("Dash UI")]
    public Image dashCooldownImage;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D c = GetComponent<Collider2D>();
        if (c == null) c = gameObject.AddComponent<CircleCollider2D>();
        c.isTrigger = true;

        fish = GetComponent<Fish>();
        if (fish == null) fish = gameObject.AddComponent<Fish>();

        mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
        {
            Debug.LogError("Không tìm thấy MapManager trong scene!");
        }

        baseScaleX = Mathf.Abs(transform.localScale.x);
    }

    void Update()
    {
        if (movementLocked)
        {
            // vẫn cập nhật UI cooldown cho dash nếu muốn
            if (dashCooldownImage != null)
            {
                dashCooldownImage.enabled = enableDash;
                if (enableDash)
                {
                    if (Time.time < nextDashTime)
                    {
                        float elapsed = dashCooldown - (nextDashTime - Time.time);
                        dashCooldownImage.fillAmount = elapsed / dashCooldown;
                    }
                    else
                    {
                        dashCooldownImage.fillAmount = 1f;
                    }
                }
            }
            return; // ❌ bỏ qua input khi bị khóa
        }

        // --- Hiển thị/ẩn icon theo enableDash ---
        if (dashCooldownImage != null)
        {
            dashCooldownImage.enabled = enableDash;
        }

        // --- update UI cooldown ---
        if (enableDash && dashCooldownImage != null)
        {
            if (Time.time < nextDashTime)
            {
                float elapsed = dashCooldown - (nextDashTime - Time.time);
                dashCooldownImage.fillAmount = elapsed / dashCooldown;
            }
            else
            {
                dashCooldownImage.fillAmount = 1f;
            }
        }

        // --- Dash input ---
        if (enableDash && !isDashing && Time.time >= nextDashTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return;

                StartDash();
            }
        }
    }





    void FixedUpdate()
    {
        if (mapManager == null) return;

        if (isDashing)
        {
            if (Time.time >= dashEndTime)
            {
                // dash xong thì để lại vận tốc còn sót để giảm dần
                isDashing = false;
            }
        }

        if (!isDashing)
        {
            // --- di chuyển bình thường ---
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0f;

            Vector2 moveDir = (mouseWorldPos - transform.position);
            float distance = moveDir.magnitude;

            if (distance > stopDistance)
            {
                moveDir.Normalize();
                currentVelocity = Vector2.MoveTowards(
                    currentVelocity,
                    moveDir * maxSpeed,
                    acceleration * Time.fixedDeltaTime
                );
            }
            else
            {
                currentVelocity = Vector2.MoveTowards(
                    currentVelocity,
                    Vector2.zero,
                    deceleration * Time.fixedDeltaTime
                );
            }
        }
        else
        {
            // khi đang dash → velocity giữ nguyên (quán tính sau dash sẽ tính sau)
        }

        // clamp trong map
        Vector2 newPos = rb.position + currentVelocity * Time.fixedDeltaTime;
        newPos.x = Mathf.Clamp(newPos.x, mapManager.bottomLeft.x, mapManager.topRight.x);
        newPos.y = Mathf.Clamp(newPos.y, mapManager.bottomLeft.y, mapManager.topRight.y);

        rb.MovePosition(newPos);
        UpdateVisual(currentVelocity);
    }

    void StartDash()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;

        Vector2 dashDir = (mouseWorldPos - transform.position).normalized;
        currentVelocity = dashDir * dashForce;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;

    }

    void UpdateVisual(Vector2 moveDir)
    {
        float currentSize = (fish != null) ? fish.size : 1f;

        if (moveDir.magnitude < 0.01f)
        {
            Quaternion targetRot = Quaternion.Euler(0f, 0f, 0f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRot, tiltLerpSpeed * Time.deltaTime);

            transform.localScale = new Vector3(baseScaleX * currentSize,
                                               currentSize,
                                               currentSize);
            return;
        }

        float signX = Mathf.Sign(moveDir.x);

        transform.localScale = new Vector3(signX * baseScaleX * currentSize,
                                           currentSize,
                                           currentSize);

        float yRatio = moveDir.y / moveDir.magnitude;
        float tiltThreshold = 0.3f;

        float targetTilt = 0f;
        if (Mathf.Abs(yRatio) > tiltThreshold)
        {
            float normalizedY = (Mathf.Abs(yRatio) - tiltThreshold) / (1f - tiltThreshold);
            targetTilt = Mathf.Clamp(normalizedY * maxTiltAngle * Mathf.Sign(moveDir.y), -maxTiltAngle, maxTiltAngle);
        }

        targetTilt *= signX;

        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetTilt);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, tiltLerpSpeed * Time.deltaTime);
    }
    public void LockMovement()
    {
        movementLocked = true;
        currentVelocity = Vector2.zero;
        if (rb != null) rb.velocity = Vector2.zero;
    }

    public void UnlockMovement()
    {
        movementLocked = false;
    }

}
