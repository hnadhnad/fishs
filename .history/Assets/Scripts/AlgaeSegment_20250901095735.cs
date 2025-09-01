using UnityEngine;

public class AlgaeSegment : MonoBehaviour
{
    [HideInInspector] public AlgaeChain chain;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // thử ăn
            bool eaten = chain.TryEatSegment(gameObject);
            if (eaten)
            {
                Debug.Log("Ăn được 1 đốt tảo!");
            }
            else
            {
                Debug.Log("Không thể ăn đốt này (chưa ăn từ trên xuống).");
            }
        }
    }
}
