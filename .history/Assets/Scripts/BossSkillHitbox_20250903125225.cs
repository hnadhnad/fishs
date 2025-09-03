using UnityEngine;

/// <summary>
/// Script gắn vào các chiêu (projectile, vùng đòn, laser...) của Boss.
/// Khi Player dính phải sẽ gọi Fish.Die().
/// </summary>
public class BossSkillHitbox : MonoBehaviour
{
    [Tooltip("Đánh dấu đây là hitbox của chiêu Boss")]
    public bool isChieu = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isChieu) return;

        Fish fish = other.GetComponent<Fish>();
        if (fish != null && fish.isPlayer)
        {
            fish.Die(); // ✅ gọi hàm chết của player
            Debug.Log("Player bị Boss giết bởi chiêu!");
        }
    }
}
