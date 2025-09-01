using UnityEngine;

public class AlgaeChain : MonoBehaviour
{
    [Header("Algae Settings")]
    public GameObject segmentPrefab;    // prefab từng đốt tảo
    public int maxSegments = 5;         // số đốt tối đa
    public float growInterval = 0.5f;   // thời gian chờ giữa mỗi lần mọc
    public float segmentSpacing = 0.5f; // khoảng cách mong muốn giữa 2 đốt
    public float antiGravity = -0.2f;   // trọng lực ngược

    private int currentSegments = 0;
    private Transform lastSegment;
    private float growTimer = 0f;
    private Rigidbody2D anchorRb;

    void Start()
    {
        // Anchor ở gốc
        GameObject anchor = new GameObject("AlgaeAnchor");
        anchor.transform.position = transform.position;
        anchor.transform.SetParent(transform);
        anchorRb = anchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Static;

        // Segment đầu tiên
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

                // spawn gần lastSegment để joint tự kéo ra khoảng cách
                Vector3 spawnPos = lastSegment.position; 
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
        rb.drag = 1f;
        rb.angularDrag = 2f;

        if (isRoot)
        {
            SpringJoint2D joint = seg.AddComponent<SpringJoint2D>();
            joint.connectedBody = anchorRb;
            joint.autoConfigureDistance = false;
            joint.distance = segmentSpacing;
            joint.dampingRatio = 0.7f;
            joint.frequency = 1.5f;
        }
        else if (lastSegment != null)
        {
            SpringJoint2D joint = seg.AddComponent<SpringJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = segmentSpacing;
            joint.dampingRatio = 0.7f;
            joint.frequency = 1.5f;
        }

        lastSegment = seg.transform;
        currentSegments++;
    }
}
