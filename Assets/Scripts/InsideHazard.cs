using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InsideHazard : MonoBehaviour
{
    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var fish = other.GetComponent<Fish>();
        if (fish == null || !fish.isPlayer) return;

        fish.Die();
    }
}
