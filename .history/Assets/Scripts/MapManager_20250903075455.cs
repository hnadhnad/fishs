using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Limits")]
    public Vector2 bottomLeft = Vector2.zero;      // Giới hạn dưới trái
    public Vector2 topRight = new Vector2(20, 10); // Giới hạn trên phải

    [Header("Background Settings")]
    public Sprite backgroundSprite;   // Kéo sprite .png vào đây trong Inspector
    public Color backgroundColor = Color.white; // Cho phép chỉnh màu overlay (tùy chọn)

    public Vector2 MapSize { get; private set; }

    void Start()
    {
        SetupMap();
        // Triệu hồi boss ngay khi bắt đầu game (nếu cần)
    }

    void SetupMap()
    {
        // Tính size từ 2 giới hạn
        MapSize = topRight - bottomLeft;

        // Tạo 1 GameObject mới để làm nền
        GameObject bg = new GameObject("Background");
        bg.transform.parent = this.transform;

        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();

        if (backgroundSprite != null)
        {
            sr.sprite = backgroundSprite;
        }
        else
        {
            // fallback: nếu không có sprite thì tạo 1 hình trắng
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        sr.color = backgroundColor;

        // Scale background để phủ kín map
        Vector2 spriteSize = sr.sprite.bounds.size;
        float scaleX = MapSize.x / spriteSize.x;
        float scaleY = MapSize.y / spriteSize.y;

        bg.transform.localScale = new Vector3(scaleX, scaleY, 1);

        // Đặt background ở giữa map
        bg.transform.position = new Vector3(
            (bottomLeft.x + topRight.x) / 2f,
            (bottomLeft.y + topRight.y) / 2f,
            5   // đẩy nó xuống dưới cùng
        );

        Debug.Log($"Map setup xong! bottomLeft = {bottomLeft}, topRight = {topRight}, size = {MapSize}");
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
