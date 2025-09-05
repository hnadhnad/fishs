using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Phase3Bomb : MonoBehaviour
{
    [HideInInspector] public float bombRadius = 1f;
    private bool consumed = false;

    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.bounds.size.x > 0.01f)
        {
            float spriteSize = sr.bounds.size.x;
            float scale = (bombRadius * 2f) / spriteSize;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        var circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.isTrigger = false;
            circle.radius = bombRadius;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Bomb collision với: {collision.gameObject.name}, layer={collision.gameObject.layer}, rb={collision.rigidbody}");
        if (consumed) return;

        var player = collision.gameObject.GetComponentInParent<Fish>();
        if (player != null && player.isPlayer)
        {
            Debug.Log("Player va chạm Bomb → Die()");
            player.Die();
            consumed = true; // đánh dấu bomb đã xử lý
        }
    }

    public void MarkConsumed()
    {
        consumed = true;
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
