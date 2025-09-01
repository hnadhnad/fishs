using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class AlgaeSegment : MonoBehaviour
{
    [HideInInspector] public AlgaeChain chain;

    private Quaternion initialRotation;
    public float uprightLerpSpeed = 2f; // tốc độ hồi về hướng ban đầu

    void Awake()
    {
        // lưu lại rotation ban đầu của prefab
        initialRotation = transform.rotation;
    }

    void LateUpdate()
    {
        // quay dần về hướng ban đầu (sử dụng Lerp để mượt)
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            initialRotation,
            Time.deltaTime * uprightLerpSpeed
        );
    }
}
