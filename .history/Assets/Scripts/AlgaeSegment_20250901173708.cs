using UnityEngine;

[RequireComponent(typeof(Fish))]
public class AlgaeSegment : MonoBehaviour
{
    [HideInInspector] public AlgaeChain chain;

    private Quaternion initialRotation;
    public float uprightLerpSpeed = 2f;

    void Awake()
    {
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            initialRotation,
            Time.deltaTime * uprightLerpSpeed
        );
    }
}
