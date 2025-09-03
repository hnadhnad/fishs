using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2:
/// - Boss rượt player với tốc độ = moveSpeed * phase2ChaseMultiplier.
/// - Né các bomb đang tồn tại (không chạy vô radius).
/// - Mỗi loạt bắn 5 bomb, 2 quả cuối boss đứng yên (mệt).
/// - Nếu bomb trúng boss → stun, rơi thịt, boss đi ăn thịt rồi mới quay lại loop.
/// </summary>
public class BossPhase2State : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        if (routine != null) boss.StopCoroutine(routine);
        routine = boss.StartCoroutine(PhaseRoutine(boss));
    }

    public void Update(Boss boss) { }

    public void Exit(Boss boss)
    {
        if (routine != null)
        {
            boss.StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator PhaseRoutine(Boss boss)
    {
        Transform playerT = null;

        while (boss != null && boss.currentHealth > 0f)
        {
            if (playerT == null)
            {
                var go = GameObject.FindWithTag("Player");
                if (go != null) playerT = go.transform;
            }

            // 🔥 Bắn bomb
            for (int i = 0; i < boss.phase2BombPerCycle; i++)
            {
                Vector3 targetPos = (playerT != null) ? playerT.position : boss.transform.position;
                SpawnBomb(boss, targetPos);

                float wait = boss.phase2BombInterval;
                float elapsed = 0f;

                bool isExhaustPhase = (i >= boss.phase2BombPerCycle - 2);

                while (elapsed < wait)
                {
                    if (!boss.IsStunned && playerT != null)
                    {
                        if (!isExhaustPhase) // 🔹 chỉ di chuyển khi chưa mệt
                        {
                            MoveTowardsPlayerAvoidingBombs(boss, playerT.position);
                        }
                    }

                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // nghỉ ngắn giữa 2 loạt
            yield return new WaitForSeconds(1f);

            // ✅ Sau loạt bomb → boss đi ăn thịt (nếu có)
            yield return EatAllMeat(boss);
        }
    }

    private void SpawnBomb(Boss boss, Vector3 targetPosition)
    {
        if (boss.phase2BombPrefab == null) return;

        GameObject bombGo = Object.Instantiate(boss.phase2BombPrefab, targetPosition, Quaternion.identity);

        if (!bombGo.TryGetComponent<FallingBomb>(out var bomb))
            bomb = bombGo.AddComponent<FallingBomb>();

        bomb.Configure(
            boss.phase2BombDelay,        // float fallDuration
            boss.phase2BombRadius,       // float explodeRadius
            boss.phase2BombDamage,       // float damage
            boss.meatPrefab,             // GameObject meatPrefab
            boss.phase2MeatCount,        // int meatCount
            boss.phase2MeatSpawnOffset,  // float meatSpawnOffset
            boss.phase2MeatScatterSpeed, // float meatScatterSpeed
            boss.phase2BossStunDuration  // float bossStunDuration
        );
    }

    /// <summary>
    /// Boss chạy về player nhưng tránh vùng bomb.
    /// </summary>
    private void MoveTowardsPlayerAvoidingBombs(Boss boss, Vector3 playerPos)
    {
        Vector3 moveDir = (playerPos - boss.transform.position).normalized;

        // Check từng bomb đang tồn tại
        var bombs = Object.FindObjectsOfType<FallingBomb>();
        foreach (var bomb in bombs)
        {
            float dist = Vector3.Distance(boss.transform.position, bomb.transform.position);
            if (dist < bomb.explodeRadius + 1f) // 1f = margin tránh
            {
                // Né sang hướng vuông góc
                Vector3 away = (boss.transform.position - bomb.transform.position).normalized;
                moveDir += away * 1.5f; // cộng vector né
            }
        }

        moveDir.Normalize();
        boss.transform.position += moveDir * boss.moveSpeed * boss.phase2ChaseMultiplier * Time.deltaTime;
    }

    /// <summary>
    /// Boss ăn hết thịt trên map trước khi quay lại loop.
    /// </summary>
    private IEnumerator EatAllMeat(Boss boss)
    {
        while (true)
        {
            MeatPiece meat = GameObject.FindObjectOfType<MeatPiece>();
            if (meat == null) yield break; // hết thịt → thoát

            // boss move tới thịt
            while (meat != null && Vector3.Distance(boss.transform.position, meat.transform.position) > 0.1f)
            {
                boss.transform.position = Vector3.MoveTowards(
                    boss.transform.position,
                    meat.transform.position,
                    boss.phase2EatMeatSpeed * Time.deltaTime
                );
                yield return null;
            }

            // boss ăn thịt
            if (meat != null)
            {
                boss.currentHealth = Mathf.Min(boss.maxHealth, boss.currentHealth + meat.healAmount);
                Object.Destroy(meat.gameObject);
            }

            yield return null;
        }
    }
}
