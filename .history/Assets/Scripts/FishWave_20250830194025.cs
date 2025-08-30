using UnityEngine;

[RequireComponent(typeof(Fish))]
public class FishWave : MonoBehaviour
{
    public float speed = 2f;
    public float amplitude = 0.5f;
    public float frequency = 2f;
    public float initialSize = 1.0f;

    private Vector3 startPos;
    Fish fish;

    void Start()
    {
        startPos = transform.position;
        fish = GetComponent<Fish>();
        if (fish != null) fish.SetSize(initialSize);
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position += Vector3.right * speed * Time.deltaTime;
        transform.position = new Vector3(transform.position.x, startPos.y + yOffset, transform.position.z);

        if (!IsVisible())
            Destroy(gameObject);
    }

    bool IsVisible()
    {
        Vector3 viewPos = Camera.main.WorldToViewportPoint(transform.position);
        return (viewPos.x > -0.1f && viewPos.x < 1.1f && viewPos.y > -0.1f && viewPos.y < 1.1f);
    }
}
