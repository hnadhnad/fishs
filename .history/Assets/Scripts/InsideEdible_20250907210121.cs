using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Fish))]   // ✅ đảm bảo luôn có Fish để dùng chung cơ chế
public class InsideEdible : MonoBehaviour
{
    public float hungerRestore = 30f;

    private Fish selfFish;

    void Awake()
    {
        selfFish = GetComponent<Fish>();

        // đảm bảo collider là trigger
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // gán thêm giá trị Hunger để player nhận khi ăn
        selfFish.hungerValue = hungerRestore;

        // tuỳ bạn có muốn điểm không, nếu không thì set 0
        selfFish.scoreValue = 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Fish eater = other.GetComponent<Fish>();
        if (eater == null) return;

        // chỉ cho Player ăn
        if (eater.isPlayer && eater.size > selfFish.size)
        {
            eater.Eat(selfFish);
        }
    }
}
