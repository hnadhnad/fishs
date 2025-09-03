using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2:
/// - Boss rượt player với tốc độ = moveSpeed * phase2ChaseMultiplier (trừ khi đang mệt / bị choáng).
/// - Mỗi loạt bắn 5 bomb.
/// - Bomb là vùng AOE: sau delay thì nổ (kill Player trong vùng, làm Boss mất máu).
/// - Nếu bomb TRÚNG boss: boss bị choáng, rơi thịt quanh boss.
///   Mỗi miếng = bombDamage / meatCount.
/// - Sau mỗi loạt bomb, boss đi ăn hết thịt còn lại trên map trước khi quay lại loop.
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

            // 🔥 Boss bắn bomb liên tục
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

            // nghỉ giữa 2 loạt
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

        // Config bomb
bomb.Configure(
    boss.phase2BombDelay,        // float fallDuration
    boss.phase2BombRadius,       // float explodeRadius
    boss.phase2BombDamage,       // float damage
    boss.meatPrefab,             // GameObject meatPrefab
    boss.phase2MeatCount,        // int meatCount
    boss.phase2MeatSpawnOffset,  // float meatSpawnOffset  <-- thêm cái này
    boss.phase2MeatScatterSpeed, // float meatScatterSpeed
    boss.phase2BossStunDuration  // float bossStunDuration
);


    }

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
