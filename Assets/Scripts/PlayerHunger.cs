using UnityEngine;
using UnityEngine.UI;

public class PlayerHunger : MonoBehaviour
{
    public static PlayerHunger Instance; // Singleton để Fish gọi vào dễ dàng

    [Header("Hunger Settings")]
    public float maxHunger = 100f;
    public float hungerDecayRate = 5f; // tốc độ giảm đói theo giây
    public Slider hungerSlider;        // tham chiếu tới UI thanh đói

    private float currentHunger;
    private Fish playerFish;

    void Awake()
    {
        Instance = this;
        playerFish = GetComponent<Fish>();
        currentHunger = maxHunger;
    }

    void Update()
    {
        currentHunger -= hungerDecayRate * Time.deltaTime;
        currentHunger = Mathf.Clamp(currentHunger, 0, maxHunger);

        if (hungerSlider != null)
            hungerSlider.value = currentHunger / maxHunger;

        if (currentHunger <= 0)
        {
            Destroy(playerFish.gameObject); // chết đói
        }
    }

    public void GainHunger(float amount)
    {
        currentHunger = Mathf.Clamp(currentHunger + amount, 0, maxHunger);
    }
}
