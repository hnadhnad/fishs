using UnityEngine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("Giới hạn map (tọa độ thế giới)")]
    public Vector2 minBounds = new Vector2(-8f, -4.5f); 
    public Vector2 maxBounds = new Vector2(8f, 4.5f);

    [Header("Background")]
    public SpriteRenderer background;

    void Awake()
    {
        Instance = this;

        if (background != null)
        {
            Vector3 size = background.bounds.size;

            if (size.x > 0 && size.y > 0)
            {
                float scaleX = (maxBounds.x - minBounds.x) / size.x;
                float scaleY = (maxBounds.y - minBounds.y) / size.y;
                background.transform.localScale = new Vector3(scaleX, scaleY, 1);
            }
            else
            {
                Debug.LogError("Background sprite có size = 0! Hãy kiểm tra SpriteRenderer.");
            }
        }
    }


    // Check 1 vị trí có trong map không
    public bool IsInsideMap(Vector3 pos)
    {
        return (pos.x >= minBounds.x && pos.x <= maxBounds.x &&
                pos.y >= minBounds.y && pos.y <= maxBounds.y);
    }

    // Trả về 1 vị trí random trong map
    public Vector2 GetRandomPosition()
    {
        return new Vector2(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y)
        );
    }
}
