using UnityEngine;

public class FishStraight : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 direction = Vector2.right;

    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);

        // Nếu ra khỏi màn hình thì huỷ
        if (!IsVisible())
            Destroy(gameObject);
    }

    bool IsVisible()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewPos.x > -0.1f && viewPos.x < 1.1f && viewPos.y > -0.1f && viewPos.y < 1.1f);
    }
}
