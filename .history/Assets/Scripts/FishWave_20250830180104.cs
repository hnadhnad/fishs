using UnityEngine;

public class FishWave : MonoBehaviour
{
    public float speed = 2f;
    public float amplitude = 0.5f; // biên độ sóng
    public float frequency = 2f;   // tần số sóng

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
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
