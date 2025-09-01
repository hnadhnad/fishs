using UnityEngine;

public class AlgaeChain : MonoBehaviour
{
    [Header("Algae Settings")]
    public GameObject segmentPrefab;   // prefab từng đốt tảo
    public int maxSegments = 5;        // số đốt tối đa
    public float growInterval = 2f;  // thời gian chờ giữa mỗi lần mọc đốt

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
                Vector3 spawnPos = lastSegment.position + Vector3.up * 0.5f; // mỗi đốt mọc lên trên
                AddSegment(spawnPos);
            }
        }
    }

    void AddSegment(Vector3 pos)
    {
        GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);
        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.gravityScale = -0.2f; // lơ lửng, ngược trọng lực

        if (lastSegment != null)
        {
            DistanceJoint2D joint = seg.AddComponent<DistanceJoint2D>();
            joint.connectedBody = lastSegment.GetComponent<Rigidbody2D>();
            joint.autoConfigureDistance = false;
            joint.distance = 0.5f; // khoảng cách giữa 2 đốt
        }

        lastSegment = seg.transform;
        currentSegments++;
    }
}
