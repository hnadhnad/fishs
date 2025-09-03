using UnityEngine;

/// <summary>
/// Miếng thịt rơi ra khi bomb trúng boss.
/// - GẮN kèm Fish để Player có thể "ăn" (huỷ miếng thịt, ngăn boss hồi).
/// - Nếu Boss chạm vào => hồi máu cho Boss bằng healAmount và huỷ miếng.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MeatPiece : MonoBehaviour
{
    [SerializeField] public float healAmount = 25f; // sẽ được set = bombDamage / meatCount

    public void SetHealAmount(float amount)
    {
        healAmount = Mathf.Max(0f, amount);
    }

    void Awake()
    {
        if (TryGetComponent<Collider2D>(out var col))
            col.isTrigger = true; // để bắt va chạm "ăn"
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // Player ăn thịt => chỉ cần huỷ miếng (Fish của player xử lý hunger/score nếu muốn)
        var f = other.GetComponent<Fish>();
        if (f != null && f.isPlayer)
        {
            Destroy(gameObject);
            return;
        }

        // Boss ăn thịt => hồi máu đúng số healAmount
        var boss = other.GetComponent<Boss>();
        if (boss != null)
        {
            boss.currentHealth = Mathf.Min(boss.maxHealth, boss.currentHealth + healAmount);
            Destroy(gameObject);
        }
    }
}
