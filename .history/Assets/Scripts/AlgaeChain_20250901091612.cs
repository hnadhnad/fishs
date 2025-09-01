using UnityEngine;

public class AlgaeChain : MonoBehaviour
{
    [Header("Algae Settings")]
    public GameObject segmentPrefab;   // prefab từng đốt tảo
    public int maxSegments = 5;        // số đốt tối đa
    public float growInterval = 0.5f;  // thời gian chờ giữa mỗi lần mọc đốt
    public float segmentSpacing = 0.5f; // khoảng cách giữa 2 đốt
    public float antiGravity = -0.2f;   // trọng lực ngược (âm để lơ lửng)

    private int currentSegments = 0;
    private Transform lastSegment;
    private float growTimer = 0f;

    void Start()
    {
        // tạo segment gốc tại vị trí spawn (cắm vào đáy)
        AddSegment(transform.position, isRoot: true);
    }

    void Update()
    {
        if (currentSegments < maxSegments)
        {
            growTimer += Time.deltaTime;
            if (growTimer >= growInterval)
            {
                growTimer = 0f;

                // mỗi đốt mọc lên trên theo khoảng cách
                Vector3 spawnPos = lastSegment.position + Vector3.up * segmentSpacing;
                AddSegment(spawnPos);
            }
        }
    }

    void AddSegment(Vector3 pos, bool isRoot = false)
    {
        GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);

        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.gravityScale = antiGravity;

        if (isRoot)
        {
            // đốt đầu tiên cố định tại chỗ (cắm vào đáy)
            rb.bodyType = RigidbodyType2D.Static;
        }
        else
        {
            // nối vào đốt trước
            DistanceJoint2D joint = seg.AddComponent<DistanceJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = segmentSpacing;
            joint.enableCollision = false;
        }

        lastSegment = seg.transform;
        currentSegments++;
    }
}
