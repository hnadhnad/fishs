using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Limits")]
    public Vector2 bottomLeft = Vector2.zero;      // Giới hạn dưới trái
    public Vector2 topRight = new Vector2(20, 10); // Giới hạn trên phải

    public Vector2 MapSize { get; private set; }

    void Start()
    {
        SetupMap();
    }

    void SetupMap()
    {
        // Tính size từ 2 giới hạn
        MapSize = topRight - bottomLeft;

        // Tạo 1 GameObject mới để làm nền
        GameObject bg = new GameObject("Background");
        bg.transform.parent = this.transform;

        // Thêm SpriteRenderer và dùng sprite mặc định "Square"
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.GetBuiltinResource<Sprite>("Sprites/Square.psd");
        sr.color = Color.black;

        // Scale background để phủ kín map
        Vector2 spriteSize = sr.sprite.bounds.size;
        float scaleX = MapSize.x / spriteSize.x;
        float scaleY = MapSize.y / spriteSize.y;

        bg.transform.localScale = new Vector3(scaleX, scaleY, 1);

        // Đặt background ở giữa map
        bg.transform.position = new Vector3(
            (bottomLeft.x + topRight.x) / 2f,
            (bottomLeft.y + topRight.y) / 2f,
            0
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
