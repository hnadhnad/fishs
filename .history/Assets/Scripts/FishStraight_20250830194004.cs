using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishStraight : MonoBehaviour
{
    public float speed = 2f;
    public Vector2 direction = Vector2.right;
    public float initialSize = 0.9f;

    Fish fish;

    void Start()
    {
        fish = GetComponent<Fish>();
        if (fish != null) fish.SetSize(initialSize);
    }

    void Update()
    {
        transform.Translate(direction.normalized * speed * Time.deltaTime);

        if (!IsVisible())
            Destroy(gameObject);
    }

    bool IsVisible()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewPos.x > -0.1f && viewPos.x < 1.1f && viewPos.y > -0.1f && viewPos.y < 1.1f);
    }
}
