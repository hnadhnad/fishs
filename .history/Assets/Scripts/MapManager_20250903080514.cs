using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Background Settings")]
    public Sprite backgroundSprite;   // PNG để làm nền
    public float mapScale = 1f;       // scale toàn bộ map (phóng to/thu nhỏ)

    [HideInInspector] public Vector2 bottomLeft;
    [HideInInspector] public Vector2 topRight;
    public Vector2 MapSize { get; private set; }

    void Start()
    {
        SetupMap();
    }

    void SetupMap()
    {
        if (backgroundSprite == null)
        {
            Debug.LogError("Chưa gán backgroundSprite!");
            return;
        }

        // tạo object nền
        GameObject bg = new GameObject("Background");
        bg.transform.parent = this.transform;

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;

        // ✅ đảm bảo luôn nằm dưới cá
        sr.sortingLayerName = "Default"; // hoặc "Background" nếu bạn tạo riêng
        sr.sortingOrder = -100;

        // scale theo mapScale
        bg.transform.localScale = Vector3.one * mapScale;

        // đặt chính giữa
        bg.transform.position = Vector3.zero;

        // tính kích thước map
        Vector2 spriteSize = sr.sprite.bounds.size * mapScale;
        MapSize = spriteSize;

        bottomLeft = new Vector2(-spriteSize.x / 2f, -spriteSize.y / 2f);
        topRight   = new Vector2( spriteSize.x / 2f,  spriteSize.y / 2f);

        Debug.Log($"Map setup xong! bottomLeft = {bottomLeft}, topRight = {topRight}, size = {MapSize}");
    }

    void OnDrawGizmos()
    {
        if (backgroundSprite == null) return;

        // lấy kích thước sprite (dùng trực tiếp vì Gizmos vẽ ở editor)
        Vector2 spriteSize = backgroundSprite.bounds.size * mapScale;
        Vector2 bl = new Vector2(-spriteSize.x / 2f, -spriteSize.y / 2f);
        Vector2 tr = new Vector2( spriteSize.x / 2f,  spriteSize.y / 2f);

        Gizmos.color = Color.green;

        // vẽ hình chữ nhật
        Gizmos.DrawLine(new Vector3(bl.x, bl.y), new Vector3(tr.x, bl.y));
        Gizmos.DrawLine(new Vector3(bl.x, bl.y), new Vector3(bl.x, tr.y));
        Gizmos.DrawLine(new Vector3(tr.x, tr.y), new Vector3(tr.x, bl.y));
        Gizmos.DrawLine(new Vector3(tr.x, tr.y), new Vector3(bl.x, tr.y));
    }
}
