using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class InsideEdible : MonoBehaviour
{
    private float hungerRestore = 30f;

    public void Init(float hunger)
    {
        hungerRestore = hunger;
    }

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
