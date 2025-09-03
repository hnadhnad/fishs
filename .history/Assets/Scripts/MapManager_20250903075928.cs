using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Background Settings")]
    public Sprite backgroundSprite;   // Kéo sprite .png vào Inspector
    public Color backgroundColor = Color.white;
    public float mapScale = 1f;       // Hệ số scale (bé/lớn)

    [Header("Map Limits (auto tính)")]
    private Vector2 bottomLeft;  
    private Vector2 topRight;  
    public Vector2 MapSize { get; private set; }

    void Start()
    {
        SetupMap();
    }

    void SetupMap()
    {
        if (backgroundSprite == null)
        {
            Debug.LogError("⚠️ Chưa gán Background Sprite!");
            return;
        }

        // Tạo background object
        GameObject bg = new GameObject("Background");
        bg.transform.parent = this.transform;

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = backgroundSprite;
        sr.color = backgroundColor;

        // Lấy kích thước gốc của sprite
        Vector2 spriteSize = sr.sprite.bounds.size;

        // Tính toán map size dựa theo sprite gốc * scale
        MapSize = spriteSize * mapScale;

        // Scale object theo mapScale (giữ nguyên tỷ lệ gốc ảnh)
        bg.transform.localScale = new Vector3(mapScale, mapScale, 1);

        // Đặt background ở giữa map
        bg.transform.position = Vector3.zero + new Vector3(0, 0, 5);

        // Tính toán giới hạn map
        bottomLeft = new Vector2(-MapSize.x / 2f, -MapSize.y / 2f);
        topRight = new Vector2(MapSize.x / 2f, MapSize.y / 2f);

        Debug.Log($"✅ Map setup xong! bottomLeft={bottomLeft}, topRight={topRight}, size={MapSize}");
    }

    // Vẽ Gizmos để nhìn khung map trong Scene
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bottomLeft, new Vector3(topRight.x, bottomLeft.y));
        Gizmos.DrawLine(bottomLeft, new Vector3(bottomLeft.x, topRight.y));
        Gizmos.DrawLine(topRight, new Vector3(topRight.x, bottomLeft.y));
        Gizmos.DrawLine(topRight, new Vector3(bottomLeft.x, topRight.y));
    }
}
