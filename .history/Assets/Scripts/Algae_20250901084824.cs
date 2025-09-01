using UnityEngine;
using System.Collections;

public class AlgaeChain : MonoBehaviour
{
    [Header("Chain Settings")]
    public int segmentCount = 5;             // số cục tối đa
    public float segmentLength = 0.5f;       // chiều dài mỗi đoạn
    public float growDelay = 0.5f;           // thời gian delay mọc thêm mỗi cục
    public GameObject segmentPrefab;         // prefab cho 1 cục tảo

    [Header("Physics Settings")]
    public float segmentMass = 0.2f;
    public float angularDrag = 2f;

    private Transform root;                  // gốc cố định
    private int currentSegments = 0;

    void Start()
    {
        // tạo object gốc cố định (đáy)
        root = new GameObject("AlgaeRoot").transform;
        root.position = transform.position;

        // bắt đầu mọc
        StartCoroutine(GrowChain());
    }

    IEnumerator GrowChain()
    {
        Rigidbody2D prevRb = null;

        while (currentSegments < segmentCount)
        {
            // tạo segment mới
            GameObject seg = Instantiate(segmentPrefab, root.position + Vector3.up * (segmentLength * currentSegments), Quaternion.identity, transform);
            seg.name = "Segment_" + currentSegments;

            Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
            if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // không bị rơi
            rb.mass = segmentMass;
            rb.angularDrag = angularDrag;

            // nối với cục trước
            if (currentSegments == 0)
            {
                // nối vào root
                HingeJoint2D joint = seg.AddComponent<HingeJoint2D>();
                joint.connectedBody = null; // nối vào điểm cố định
                joint.connectedAnchor = root.position;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector2.down * (segmentLength / 2f);
                joint.connectedAnchor = root.position;
            }
            else
            {
                HingeJoint2D joint = seg.AddComponent<HingeJoint2D>();
                joint.connectedBody = prevRb;
                joint.autoConfigureConnectedAnchor = false;
                joint.anchor = Vector2.down * (segmentLength / 2f);
                joint.connectedAnchor = Vector2.up * (segmentLength / 2f);
            }

            prevRb = rb;
            currentSegments++;

            yield return new WaitForSeconds(growDelay);
        }
    }
}
