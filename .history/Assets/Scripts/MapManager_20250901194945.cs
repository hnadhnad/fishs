using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("Map Limits")]
    public Vector2 bottomLeft = Vector2.zero;      // Giới hạn dưới trái
    public Vector2 topRight = new Vector2(20, 10); // Giới hạn trên phải

    public Vector2 MapSize { get; private set; }

    [Header("Boss Settings")]
    public GameObject bossPrefab;     // Kéo prefab Boss vào đây
    public Vector2 bossSpawnPos = new Vector2(5, 5); // ✅ Đúng

    void Start()
    {
        SetupMap();

        // Triệu hồi boss ngay khi bắt đầu game
        SpawnBoss();
    }

    void SetupMap()
    {
        // Tính size từ 2 giới hạn
        MapSize = topRight - bottomLeft;

        // Tạo 1 GameObject mới để làm nền
        GameObject bg = new GameObject("Background");
        bg.transform.parent = this.transform;

        // Tạo 1 sprite trắng từ Texture2D
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
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
            5   // đẩy nó xuống dưới cùng
        );

        Debug.Log($"Map setup xong! bottomLeft = {bottomLeft}, topRight = {topRight}, size = {MapSize}");
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("⚠️ Chưa gán Boss Prefab trong Inspector!");
            return;
        }

        // Nếu chưa set vị trí thì mặc định ở giữa map
        Vector2 spawnPos = bossSpawnPos == Vector2.zero
            ? new Vector2((bottomLeft.x + topRight.x) / 2f, (bottomLeft.y + topRight.y) / 2f)
            : bossSpawnPos;

        Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Boss đã được triệu hồi tại: " + spawnPos);
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
