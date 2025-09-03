using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2:
/// - Boss rượt player với tốc độ = moveSpeed * phase2ChaseMultiplier (trừ khi đang mệt / bị choáng).
/// - Mỗi loạt bắn 5 bomb.
/// - Bomb là vùng AOE: sau delay thì nổ (kill Player trong vùng, làm Boss mất máu).
/// - Nếu bomb TRÚNG boss: boss bị choáng, rơi thịt quanh boss.
///   Mỗi miếng = bombDamage / meatCount.
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

            // bắn bomb
            for (int i = 0; i < boss.phase2BombPerCycle; i++)
            {
                Vector3 targetPos = (playerT != null) ? playerT.position : boss.transform.position;

                SpawnBomb(boss, targetPos);

                float wait = boss.phase2BombInterval;
                float elapsed = 0f;

                while (elapsed < wait)
                {
                    if (!boss.IsStunned && playerT != null)
                    {
                        float speed = boss.moveSpeed * boss.phase2ChaseMultiplier;
                        boss.transform.position = Vector3.MoveTowards(
                            boss.transform.position,
                            playerT.position,
                            speed * Time.deltaTime
                        );
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // nghỉ 1 nhịp giữa loạt
            yield return new WaitForSeconds(1f);
        }
    }

    private void SpawnBomb(Boss boss, Vector3 targetPosition)
    {
        if (boss.phase2BombPrefab == null) return;

        GameObject bombGo = Object.Instantiate(boss.phase2BombPrefab, targetPosition, Quaternion.identity);

        if (!bombGo.TryGetComponent<FallingBomb>(out var bomb))
            bomb = bombGo.AddComponent<FallingBomb>();

        bomb.Configure(
            boss.phase2BombDelay,
            boss.phase2BombRadius,
            boss.phase2BombDamage,
            boss.meatPrefab,
            boss.phase2MeatCount,
            boss.phase2MeatScatterRadius,
            boss.phase2BossStunDuration
        );
    }
}
