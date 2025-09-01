using UnityEngine;

public class AlgaeChain : MonoBehaviour
{
    [Header("Algae Settings")]
    public GameObject segmentPrefab;    // prefab từng đốt tảo
    public int maxSegments = 5;         // số đốt tối đa
    public float growInterval = 0.5f;   // thời gian chờ giữa mỗi lần mọc
    public float segmentSpacing = 0.5f; // khoảng cách giữa 2 đốt
    public float antiGravity = -0.2f;   // trọng lực ngược (âm để lơ lửng)

    private int currentSegments = 0;
    private Transform lastSegment;
    private float growTimer = 0f;
    private Rigidbody2D anchorRb;       // điểm neo cố định ở gốc

    void Start()
    {
        // tạo Rigidbody2D tĩnh làm anchor tại vị trí gốc
        GameObject anchor = new GameObject("AlgaeAnchor");
        anchor.transform.position = transform.position;
        anchor.transform.SetParent(transform);
        anchorRb = anchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Static;

        // tạo segment đầu tiên nối vào anchor
        AddSegment(transform.position, isRoot: true);
    }

    void Update()
    {
        if (currentSegments < maxSegments && lastSegment != null)
        {
            growTimer += Time.deltaTime;
            if (growTimer >= growInterval)
            {
                growTimer = 0f;

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
            // nối root vào anchor
            DistanceJoint2D joint = seg.AddComponent<DistanceJoint2D>();
            joint.connectedBody = anchorRb;
            joint.autoConfigureDistance = false;
            joint.distance = 0f;
        }
        else if (lastSegment != null)
        {
            // nối segment mới vào segment trước
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
