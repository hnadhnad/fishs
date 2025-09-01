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
    private Transform lastSegment;  // mắt xích cuối cùng
    private float growTimer = 0f;

    void Start()
    {
        // Anchor (gốc giả) luôn tồn tại
        GameObject anchor = new GameObject("AlgaeAnchor");
        anchor.transform.parent = transform;
        anchor.transform.position = transform.position;

        Rigidbody2D rb = anchor.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // gốc cố định

        lastSegment = anchor.transform;

        // mọc luôn segment đầu tiên khi bắt đầu
        AddSegment(anchor.transform.position + Vector3.up * segmentDistance);
    }

    void Update()
    {
        if (currentSegments < maxSegments && lastSegment != null)
        {
            growTimer += Time.deltaTime;
            if (growTimer >= growInterval)
            {
                growTimer = 0f;

                // 👉 spawn hơi lệch trái/phải để có chuyển động vật lý
                Vector3 spawnPos = lastSegment.position 
                                   + Vector3.up * segmentDistance 
                                   + new Vector3(Random.Range(-0.1f, 0.1f), 0, 0);

                AddSegment(spawnPos);
            }
        }
    }

    void AddSegment(Vector3 pos)
    {
        if (segmentPrefab == null) return;

        GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);

        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = antiGravity;   // nổi lên
        rb.drag = 1f;                   // giảm trượt
        rb.angularDrag = 2f;            // giảm xoay

        DistanceJoint2D joint = seg.AddComponent<DistanceJoint2D>();
        joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
        joint.autoConfigureDistance = false;
        joint.distance = segmentDistance;
        joint.enableCollision = true;

        lastSegment = seg.transform;
        currentSegments++;
    }
}
