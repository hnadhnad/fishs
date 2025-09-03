using UnityEngine;

public class Bullet : MonoBehaviour
{
    private MapManager map;

    void Start()
    {
        map = FindObjectOfType<MapManager>();
    }

    void Update()
    {
        if (map == null) return;

        Vector2 pos = transform.position;
        if (pos.x < map.bottomLeft.x - 1f || pos.x > map.topRight.x + 1f ||
            pos.y < map.bottomLeft.y - 1f || pos.y > map.topRight.y + 1f)
        {
            Destroy(gameObject);
        }
    }
}
