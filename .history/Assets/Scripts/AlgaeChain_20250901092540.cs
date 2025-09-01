using UnityEngine;

public class AlgaeChain : MonoBehaviour
{
    [Header("Algae Settings")]
    public GameObject segmentPrefab;   // prefab từng đốt tảo
    public int maxSegments = 5;        // số đốt tối đa
    public float growInterval = 1f;    // thời gian chờ giữa mỗi lần mọc đốt
    public float segmentDistance = 0.5f; // khoảng cách giữa 2 đốt
    public float antiGravity = -0.2f;  // ngược trọng lực (lơ lửng)

    private int currentSegments = 0;
    private Transform lastSegment;
    private float growTimer = 0f;

    void Start()
    {
        // tạo segment gốc tại vị trí spawn
        AddSegment(transform.position);
    }

    void Update()
    {
        if (currentSegments < maxSegments)
        {
            growTimer += Time.deltaTime;
            if (growTimer >= growInterval)
            {
                growTimer = 0f;
                // spawn cách đốt trước một đoạn
                Vector3 spawnPos = lastSegment.position + Vector3.up * segmentDistance;
                AddSegment(spawnPos);
            }
        }
    }

    void AddSegment(Vector3 pos)
    {
        GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);

        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.gravityScale = antiGravity;
        rb.drag = 1f;
        rb.angularDrag = 2f;

        if (lastSegment != null)
        {
            DistanceJoint2D joint = seg.AddComponent<DistanceJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = segmentDistance;
            joint.enableCollision = true; // để va chạm với nhau, không chồng
        }

        lastSegment = seg.transform;
        currentSegments++;
    }
}
