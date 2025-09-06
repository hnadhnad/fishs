using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InsideHeart : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var fish = other.GetComponent<Fish>();
        if (fish == null || !fish.isPlayer) return;

        var boss = FindObjectOfType<Boss>();
        if (boss != null) boss.Die();

        Destroy(gameObject);
    }
}
