using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Fish))]   // ✅ để dùng chung cơ chế Fish
public class InsideHeart : MonoBehaviour
{
    private Fish selfFish;

    void Awake()
    {
        selfFish = GetComponent<Fish>();

        // Collider luôn là trigger
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // Không cần hồi Hunger → để 0
        selfFish.hungerValue = 0;
        selfFish.scoreValue = 0;   // không cộng điểm
        selfFish.size = 0.1f;      // cho nhỏ hơn player để đảm bảo player luôn ăn được
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Fish eater = other.GetComponent<Fish>();
        if (eater == null) return;

        // chỉ Player mới ăn được tim
        if (eater.isPlayer && eater.size > selfFish.size)
        {
            // Player ăn tim → dùng cơ chế Eat bình thường
            eater.Eat(selfFish);

            // Sau đó giết Boss
            var boss = FindObjectOfType<Boss>();
            if (boss != null) boss.Die();
        }
    }
}
