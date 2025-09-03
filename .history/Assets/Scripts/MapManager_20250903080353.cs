using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Limits (dùng để vẽ viền Gizmos)")]
    public Vector2 bottomLeft = Vector2.zero;      
    public Vector2 topRight = new Vector2(20, 10); 

    [Header("Background Settings")]
    public Sprite backgroundSprite;   // PNG kéo vào đây
    public float backgroundScale = 1f; // chỉnh size lớn/nhỏ

    public Vector2 MapSize { get; private set; }
    private GameObject bg;

    void Start()
    {
        SetupMap();
    }

    void SetupMap()
    {
        // Tính size từ 2 giới hạn (chỉ để vẽ viền preview)
        MapSize = topRight - bottomLeft;

        // Nếu có backgroundSprite thì dùng
        if (backgroundSprite != null)
        {
            bg = new GameObject("Background");
            bg.transform.parent = this.transform;

            SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
            sr.sprite = backgroundSprite;
            sr.color = Color.white;

            // Luôn nằm dưới player/fish
            sr.sortingLayerName = "Background"; // cần tạo layer này trong Unity
            sr.sortingOrder = -10;

            // Scale theo backgroundScale
            bg.transform.localScale = Vector3.one * backgroundScale;

            // Đặt background ở giữa map
            bg.transform.position = new Vector3(
                (bottomLeft.x + topRight.x) / 2f,
                (bottomLeft.y + topRight.y) / 2f,
                0f // z = 0 vẫn được, vì sorting layer quyết định hiển thị
            );
        }

        Debug.Log($"Map setup xong! bottomLeft = {bottomLeft}, topRight = {topRight}, size = {MapSize}");
    }

    // Vẽ Gizmos để nhìn khung map trong Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(bottomLeft, new Vector3(topRight.x, bottomLeft.y));
        Gizmos.DrawLine(bottomLeft, new Vector3(bottomLeft.x, topRight.y));
        Gizmos.DrawLine(topRight, new Vector3(topRight.x, bottomLeft.y));
        Gizmos.DrawLine(topRight, new Vector3(bottomLeft.x, topRight.y));
    }
}
