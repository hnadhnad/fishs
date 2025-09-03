using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2:
/// - Boss rượt player với tốc độ = moveSpeed * phase2ChaseMultiplier (trừ khi đang mệt / bị choáng).
/// - Mỗi loạt bắn 5 bomb. 2 quả cuối boss đứng yên (mệt).
/// - Bomb là vùng AOE tròn: sau fallDuration thì nổ (kill Player trong vùng, làm Boss mất máu).
/// - Nếu bomb TRÚNG boss: boss bị choáng, rơi thịt xung quanh boss (4 hướng hoặc đều theo meatCount).
///   Mỗi miếng = bombDamage / meatCount. Boss ăn lại thịt sẽ hồi đúng tổng damage đã mất nếu ăn hết.
/// </summary>
public class BossPhase2State : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        if (routine != null) boss.StopCoroutine(routine);
        routine = boss.StartCoroutine(PhaseRoutine(boss));
    }

    public void Update(Boss boss)
    {
        // Toàn bộ logic nằm trong coroutine
    }

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

            // volley bombs
            for (int i = 0; i < boss.phase2BombCount; i++)
            {
                // khóa vị trí player tại thời điểm bắn
                Vector3 targetPos = (playerT != null) ? playerT.position : boss.transform.position;

                // spawn bomb (fallDuration sau đó tự nổ)
                SpawnBomb(boss, targetPos);

                bool isExhaustPhase = (i >= boss.phase2BombCount - boss.phase2FinalBombsThatExhaust);
                float wait = boss.phase2BombInterval;
                float elapsed = 0f;

                // Trong khoảng chờ tới quả kế tiếp: boss rượt player (trừ khi mệt hoặc bị stun)
                while (elapsed < wait)
                {
                    if (!isExhaustPhase && !boss.IsStunned && playerT != null)
                    {
                        float speed = boss.moveSpeed * boss.phase2ChaseMultiplier;
                        boss.transform.position = Vector3.MoveTowards(
                            boss.transform.position,
                            playerT.position,
                            speed * Time.deltaTime
                        );
                    }
                    // exhaust phase: đứng yên (mệt), stun thì cũng đứng yên luôn
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // nghỉ giữa 2 loạt
            yield return new WaitForSeconds(boss.phase2VolleyPause);
        }
    }

    private void SpawnBomb(Boss boss, Vector3 targetPosition)
    {
        if (boss.phase2BombPrefab == null) return;

        var map = Object.FindObjectOfType<MapManager>();
        float spawnY = (map != null)
            ? map.topRight.y + boss.phase2BombSpawnHeight
            : boss.transform.position.y + boss.phase2BombSpawnHeight;

        Vector3 spawnPos = new Vector3(targetPosition.x, spawnY, targetPosition.z);

        GameObject bombGo = Object.Instantiate(boss.phase2BombPrefab, spawnPos, Quaternion.identity);

        // cấu hình bomb
        if (!bombGo.TryGetComponent<FallingBomb>(out var bomb))
            bomb = bombGo.AddComponent<FallingBomb>();

        bomb.Configure(
            boss.phase2BombFallDuration,
            boss.phase2BombExplodeRadius,
            boss.phase2BombDamage,
            boss.phase2MeatPrefab,
            boss.phase2MeatCount,
            boss.phase2MeatSpawnOffset,
            boss.phase2MeatScatterSpeed,
            boss.phase2BossStunDuration
        );
    }
}
