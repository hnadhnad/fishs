using UnityEngine;

/// <summary>
/// Gắn lên prefab meat (hoặc được add khi spawn).
/// Chức năng:
///  - lưu healAmount (số HP boss sẽ hồi khi boss ăn miếng này)
///  - khi boss chạm (trigger), heal boss và Destroy self (mỗi miếng chỉ dùng 1 lần)
///  - nếu player ăn (vì prefab còn có Fish component), thì Fish.Eat xử lý player hunger; MeatPiece cũng tự Destroy để tránh double.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class MeatPiece : MonoBehaviour
{
    public float healAmount = 0f;
    private bool consumed = false;

    public void SetHealAmount(float v)
    {
        healAmount = v;
    }

    void Start()
    {
        // ensure trigger collider
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        if (other == null) return;

        // Boss eats -> heal boss's HP directly
        Boss boss = other.GetComponent<Boss>();
        if (boss != null)
        {
            consumed = true;
            boss.currentHealth = Mathf.Min(boss.maxHealth, boss.currentHealth + healAmount);
            Destroy(gameObject);
            return;
        }

        // Player or other predators: don't handle here; let Fish.Eat handle scoring/hunger.
        // However, to avoid double-destroy if Fish.Eat also calls Die()/Destroy, we just do nothing.
        // Optionally: if you want player eating to always remove meat immediately:
        Fish f = other.GetComponent<Fish>();
        if (f != null && f.isPlayer)
        {
            consumed = true;
            // Let Fish.Eat handle player score/hunger by collision order; ensure meat GameObject will be destroyed either by Fish.Eat->Die() or here:
            // If the Fish eating code does not destroy the prey (it calls prey.Die), prey.Die will call Destroy(gameObject).
            // To be safe, also schedule destroy after tiny delay if not yet destroyed:
            Destroy(gameObject, 0.05f);
        }
    }
}
