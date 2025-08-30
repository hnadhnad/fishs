using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Limits")]
    public Vector2 bottomLeft = Vector2.zero; // Giới hạn dưới trái
    public Vector2 topRight = new Vector2(20, 10); // Giới hạn trên phải

    [Header("Background")]
    public SpriteRenderer background;

    public Vector2 MapSize { get; private set; }

    void Start()
    {
        SetupMap();
    }

    void SetupMap()
    {
        // Tính size từ 2 giới hạn
        MapSize = topRight - bottomLeft;

        if (background != null)
        {
            Vector2 spriteSize = background.sprite.bounds.size;

            // Scale background
            float scaleX = MapSize.x / spriteSize.x;
            float scaleY = MapSize.y / spriteSize.y;

            background.transform.localScale = new Vector3(scaleX, scaleY, 1);

            // Đặt background đúng giữa map
            background.transform.position = new Vector3(
                (bottomLeft.x + topRight.x) / 2f,
                (bottomLeft.y + topRight.y) / 2f,
                0
            );
        }
        else
        {
            Debug.LogError("Chưa gán background SpriteRenderer!");
        }

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
