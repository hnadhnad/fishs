using UnityEngine;

/// <summary>
/// Gắn trên mỗi segment (được tạo tự động).
/// Chỉ chứa tham chiếu tới chain chủ quản.
/// </summary>
public class AlgaeSegment : MonoBehaviour
{
    [HideInInspector] public AlgaeChain chain;
}
