using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class AlgaeChain : MonoBehaviour
{
    [Header("Chain Settings")]
    public GameObject segmentPrefab;
    public int maxSegments = 5;
    public float initialGrowSpacing = 0.45f;
    public float growInterval = 0.25f;
    public float regrowDelay = 1.0f;
    public float antiGravityMin = -0.18f;
    public float antiGravityMax = -0.25f;

    [Header("Physics Joint")]
    public float jointDistance = 0.45f;
    public bool enableSegmentCollision = true;

    [Header("Sway Settings")]
    public float swayForce = 1f;   // lực đung đưa
    public float swaySpeed = 2.5f;     // tốc độ dao động

    private List<GameObject> segments = new List<GameObject>();
    private List<float> swayOffsets = new List<float>(); // offset cho mỗi segment
    private Transform anchor;
    private float growTimer = 0f;
    private float regrowTimer = 0f;
    private bool needRegrow = false;

    void Awake()
    {
        if (segmentPrefab == null)
        {
            Debug.LogError("AlgaeChain: segmentPrefab chưa gán!");
            enabled = false;
            return;
        }

        // anchor cố định
        GameObject anchorObj = new GameObject("AlgaeAnchor");
        anchorObj.transform.parent = transform;
        anchorObj.transform.localPosition = Vector3.zero;
        anchor = anchorObj.transform;
        Rigidbody2D arb = anchorObj.AddComponent<Rigidbody2D>();
        arb.bodyType = RigidbodyType2D.Kinematic;

        // spawn đầy đủ ban đầu
        for (int i = 0; i < maxSegments; i++)
        {
            Vector3 pos = anchor.position + Vector3.up * initialGrowSpacing * (i + 1);
            SpawnSegmentAt(pos, connectTo: (i == 0 ? anchor : segments[i - 1].transform));
        }
    }

    void Update()
    {
        // mọc lại nếu thiếu
        if (segments.Count < maxSegments)
        {
            if (!needRegrow)
            {
                regrowTimer += Time.deltaTime;
                if (regrowTimer >= regrowDelay)
                {
                    needRegrow = true;
                    growTimer = 0f;
                    regrowTimer = 0f;
                }
            }
            else
            {
                growTimer += Time.deltaTime;
                if (growTimer >= growInterval)
                {
                    growTimer = 0f;
                    Transform last = (segments.Count > 0) ? segments[segments.Count - 1].transform : anchor;
                    Vector3 spawn = last.position + Vector3.up * jointDistance + new Vector3(Random.Range(-0.05f, 0.05f), 0f, 0f);
                    SpawnSegmentAt(spawn, connectTo: last);
                }
            }
        }
        else
        {
            needRegrow = false;
            regrowTimer = 0f;
            growTimer = 0f;
        }
    }

    void FixedUpdate()
    {
        // tạo dao động cho từng segment
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;

            Rigidbody2D rb = segments[i].GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            float sway = Mathf.Sin(Time.time * swaySpeed + swayOffsets[i]) * swayForce;
            rb.AddForce(new Vector2(sway, 0f));
        }
    }

    private void SpawnSegmentAt(Vector3 worldPos, Transform connectTo)
    {
        GameObject seg = Instantiate(segmentPrefab, worldPos, Quaternion.identity, transform);
        if (seg == null) return;

        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Random.Range(antiGravityMin, antiGravityMax);
        rb.drag = 0.8f;
        rb.angularDrag = 1.8f;

        Collider2D col = seg.GetComponent<Collider2D>();
        if (col == null)
        {
            CircleCollider2D c = seg.AddComponent<CircleCollider2D>();
            c.radius = 0.2f;
        }

        AlgaeSegment aSeg = seg.GetComponent<AlgaeSegment>();
        if (aSeg == null) aSeg = seg.AddComponent<AlgaeSegment>();
        aSeg.chain = this;

        Fish fishComp = seg.GetComponent<Fish>();
        if (fishComp == null) fishComp = seg.AddComponent<Fish>();
        fishComp.isPlayer = false;
        fishComp.scoreValue = 0;
        fishComp.size = 0.2f;
        fishComp.spawnGraceTime = 0f;

        DistanceJoint2D dj = seg.GetComponent<DistanceJoint2D>();
        if (dj == null) dj = seg.AddComponent<DistanceJoint2D>();
        Rigidbody2D connectedRb = connectTo.GetComponent<Rigidbody2D>();
        if (connectedRb == null)
        {
            connectedRb = connectTo.gameObject.AddComponent<Rigidbody2D>();
            connectedRb.bodyType = RigidbodyType2D.Kinematic;
        }

        dj.connectedBody = connectedRb;
        dj.autoConfigureDistance = false;
        dj.distance = jointDistance;
        dj.enableCollision = enableSegmentCollision;

        segments.Add(seg);
        swayOffsets.Add(Random.Range(0f, Mathf.PI * 2f)); // offset riêng cho segment này
    }

    public bool TryEatSegment(GameObject segObj, Fish eater)
    {
        if (segObj == null) return false;

        int idx = segments.IndexOf(segObj);
        if (idx == -1) return false;
        if (idx != segments.Count - 1) return false; // chỉ ăn từ trên xuống

        segments.RemoveAt(idx);
        swayOffsets.RemoveAt(idx);
        Destroy(segObj);

        StartRegrowDelay();
        return true;
    }

    private void StartRegrowDelay()
    {
        needRegrow = false;
        regrowTimer = 0f;
    }

    public void ClearChain()
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i] != null) Destroy(segments[i]);
        }
        segments.Clear();
        swayOffsets.Clear();
    }
}
