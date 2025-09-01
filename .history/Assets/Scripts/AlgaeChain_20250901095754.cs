using UnityEngine;
using System.Collections.Generic;

public class AlgaeChain : MonoBehaviour
{
    [Header("Algae Settings")]
    public GameObject segmentPrefab;   // prefab từng đốt tảo
    public int maxSegments = 5;        // số đốt tối đa
    public float growInterval = 1f;    // thời gian chờ mọc lại
    public float segmentDistance = 0.5f;
    public float antiGravity = -0.2f;

    private List<GameObject> segments = new List<GameObject>();
    private Transform anchor;
    private float growTimer = 0f;

    void Start()
    {
        // Anchor (gốc cố định)
        GameObject anchorObj = new GameObject("AlgaeAnchor");
        anchorObj.transform.parent = transform;
        anchorObj.transform.position = transform.position;

        Rigidbody2D rb = anchorObj.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        anchor = anchorObj.transform;

        // mọc ban đầu cho đủ maxSegments
        for (int i = 0; i < maxSegments; i++)
        {
            Vector3 spawnPos = anchor.position + Vector3.up * (segmentDistance * (i + 1));
            AddSegment(spawnPos);
        }
    }

    void Update()
    {
        // Nếu thiếu segment (do bị ăn) thì mọc lại
        if (segments.Count < maxSegments)
        {
            growTimer += Time.deltaTime;
            if (growTimer >= growInterval)
            {
                growTimer = 0f;

                Transform last = (segments.Count > 0) ? segments[segments.Count - 1].transform : anchor;
                Vector3 spawnPos = last.position + Vector3.up * segmentDistance;

                AddSegment(spawnPos);
            }
        }
    }

    void AddSegment(Vector3 pos)
    {
        GameObject seg = Instantiate(segmentPrefab, pos, Quaternion.identity, transform);

        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = antiGravity;
        rb.drag = 1f;
        rb.angularDrag = 2f;

        // Joint với segment trước (hoặc anchor)
        Rigidbody2D connectedRb = (segments.Count == 0) ? anchor.GetComponent<Rigidbody2D>() : segments[segments.Count - 1].GetComponent<Rigidbody2D>();
        DistanceJoint2D joint = seg.AddComponent<DistanceJoint2D>();
        joint.connectedBody = connectedRb;
        joint.autoConfigureDistance = false;
        joint.distance = segmentDistance;
        joint.enableCollision = true;

        // thêm vào list
        segments.Add(seg);

        // gắn script ăn
        AlgaeSegment algaeSeg = seg.AddComponent<AlgaeSegment>();
        algaeSeg.chain = this;
    }

    // gọi khi 1 segment bị ăn
    public bool TryEatSegment(GameObject seg)
    {
        // chỉ cho ăn segment trên cùng
        if (segments.Count > 0 && seg == segments[segments.Count - 1])
        {
            segments.RemoveAt(segments.Count - 1);
            Destroy(seg);
            return true;
        }
        return false;
    }
}
