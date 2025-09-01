using UnityEngine;

[RequireComponent(typeof(Fish))]
public class Algae : MonoBehaviour
{
    [Header("Algae Settings")]
    public float growHeight = 1f;      // chiều cao mọc lên từ đáy
    public float growSpeed = 1f;       // tốc độ mọc
    public float stopDelay = 0.2f;     // delay khi đến vị trí cuối

    private Vector3 targetPos;
    private bool growing = true;

    void Start()
    {
        // target position cao hơn spawn một đoạn
        targetPos = transform.position + Vector3.up * growHeight;
    }

    void Update()
    {
        if (growing)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, growSpeed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                growing = false;
                Invoke(nameof(StopCompletely), stopDelay);
            }
        }
    }

    void StopCompletely()
    {
        // đứng yên vĩnh viễn
        enabled = false;
    }
}
