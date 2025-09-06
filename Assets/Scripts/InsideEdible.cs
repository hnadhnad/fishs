using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InsideEdible : MonoBehaviour
{
    public float hungerRestore = 30f;

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var fish = other.GetComponent<Fish>();
        if (fish == null || !fish.isPlayer) return;

        if (PlayerHunger.Instance != null)
            PlayerHunger.Instance.GainHunger(hungerRestore);

        Destroy(gameObject);
    }
}
