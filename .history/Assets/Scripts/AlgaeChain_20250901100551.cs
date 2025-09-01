using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class AlgaeChain : MonoBehaviour
{
    [Header("Chain Settings")]
    public GameObject segmentPrefab;      // prefab 1 mắt xích (sprite + collider). NÊN có: SpriteRenderer, Collider2D, Rigidbody2D
    public int maxSegments = 5;           // tổng số mắt xích mong muốn
    public float initialGrowSpacing = 0.45f; // khoảng cách khi lần đầu spawn (tùy chỉnh theo sprite)
    public float growInterval = 0.25f;    // thời gian giữa mỗi đốt khi mọc lại
    public float regrowDelay = 1.0f;      // delay trước khi bắt đầu mọc trở lại (khi bị ăn)
    public float antiGravityMin = -0.18f; // gravity range min (random để tự nhiên)
    public float antiGravityMax = -0.25f; // gravity range max

    [Header("Physics Joint")]
    public float jointDistance = 0.45f;
    public float jointFrequency = 1.8f;
    public float jointDamping = 0.75f;
    public bool enableSegmentCollision = true;

    // internal
    private List<GameObject> segments = new List<GameObject>();
    private Transform anchor; // anchor transform (luôn tồn tại, không thể ăn)
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

        // tạo anchor (gốc) cố định
        GameObject anchorObj = new GameObject("AlgaeAnchor");
        anchorObj.transform.parent = transform;
        anchorObj.transform.localPosition = Vector3.zero;
        anchor = anchorObj.transform;
        Rigidbody2D arb = anchorObj.AddComponent<Rigidbody2D>();
        arb.bodyType = RigidbodyType2D.Kinematic;

        // spawn ban đầu đầy đủ bằng 1 vòng để tránh chồng cứng
        for (int i = 0; i < maxSegments; i++)
        {
            Vector3 pos = anchor.position + Vector3.up * initialGrowSpacing * (i + 1);
            SpawnSegmentAt(pos, connectTo: (i == 0 ? anchor : segments[i - 1].transform));
        }
    }

    void Update()
    {
        // nếu thiếu segment, thực hiện regrow từng đoạn
        if (segments.Count < maxSegments)
        {
            if (!needRegrow)
            {
                // start counting regrow delay
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
                // mọc từng đốt theo growInterval
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
            // đủ rồi -> reset flags
            needRegrow = false;
            regrowTimer = 0f;
            growTimer = 0f;
        }
    }

    // Spawn segment at world pos and connect to 'connectTo' (transform of anchor or previous segment)
    private void SpawnSegmentAt(Vector3 worldPos, Transform connectTo)
    {
        GameObject seg = Instantiate(segmentPrefab, worldPos, Quaternion.identity, transform);
        if (seg == null) return;

        // ensure Rigidbody2D exists and is dynamic
        Rigidbody2D rb = seg.GetComponent<Rigidbody2D>();
        if (rb == null) rb = seg.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = Random.Range(antiGravityMin, antiGravityMax);
        rb.drag = 0.8f;
        rb.angularDrag = 1.8f;

        // ensure Collider2D exists (if prefab missing)
        Collider2D col = seg.GetComponent<Collider2D>();
        if (col == null)
        {
            // try to add CircleCollider2D as default
            CircleCollider2D c = seg.AddComponent<CircleCollider2D>();
            c.radius = 0.2f;
            col = c;
        }

        // Add AlgaeSegment component and link
        AlgaeSegment aSeg = seg.GetComponent<AlgaeSegment>();
        if (aSeg == null) aSeg = seg.AddComponent<AlgaeSegment>();
        aSeg.chain = this;

        // Ensure this segment has a Fish component (so predator's Fish logic recognizes it)
        Fish fishComp = seg.GetComponent<Fish>();
        if (fishComp == null)
        {
            fishComp = seg.AddComponent<Fish>();
        }
        // Set segment's fish properties: small size, not player, no score
        fishComp.isPlayer = false;
        fishComp.scoreValue = 0;
        fishComp.size = 0.2f; // small prey; adjust if needed
        fishComp.spawnGraceTime = 0f; // no grace required

        // connect joint
        DistanceJoint2D dj = seg.GetComponent<DistanceJoint2D>();
        if (dj == null) dj = seg.AddComponent<DistanceJoint2D>();
        Rigidbody2D connectedRb = connectTo.GetComponent<Rigidbody2D>();
        if (connectedRb == null)
        {
            // if connectTo is anchor but has no rb, add kinematic
            connectedRb = connectTo.gameObject.GetComponent<Rigidbody2D>();
            if (connectedRb == null)
            {
                connectedRb = connectTo.gameObject.AddComponent<Rigidbody2D>();
                connectedRb.bodyType = RigidbodyType2D.Kinematic;
            }
        }

        dj.connectedBody = connectedRb;
        dj.autoConfigureDistance = false;
        dj.distance = jointDistance;
        dj.enableCollision = enableSegmentCollision;

        // small tweaks for natural look (these properties exist on SpringJoint2D not DistanceJoint2D;
        // DistanceJoint2D has limited params — we keep it simple)
        // If you prefer spring behavior, swap to SpringJoint2D here.

        segments.Add(seg);
    }

    /// <summary>
    /// Called by predator (Fish.Eat) via Fish when it detects collision.
    /// chain decides whether the requested segment can be eaten.
    /// If allowed, chain removes and destroys the segment and returns true.
    /// </summary>
    /// <param name="segObj">GameObject of segment (the prey)</param>
    /// <param name="eater">Fish who tries to eat</param>
    /// <returns>true if eaten (was top-most); false otherwise</returns>
    public bool TryEatSegment(GameObject segObj, Fish eater)
    {
        if (segObj == null) return false;

        // Must find index in list and verify it's topmost
        int idx = segments.IndexOf(segObj);
        if (idx == -1)
        {
            // not from this chain (or already removed)
            return false;
        }

        if (idx != segments.Count - 1)
        {
            // not topmost -> cannot eat
            return false;
        }

        // allowed: remove last element and destroy
        segments.RemoveAt(idx);
        Destroy(segObj);

        // start regrow timer
        needRegrow = false;
        regrowTimer = 0f;
        // set flag to wait regrowDelay
        StartRegrowDelay();

        return true;
    }

    private void StartRegrowDelay()
    {
        needRegrow = false;
        regrowTimer = 0f;
    }

    // optional: public helper to clear chain (used by external cleanup)
    public void ClearChain()
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i] != null) Destroy(segments[i]);
        }
        segments.Clear();
    }
}
