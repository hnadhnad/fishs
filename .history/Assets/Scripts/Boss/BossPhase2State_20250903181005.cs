using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2:
/// - Boss dí theo player với tốc độ = boss.moveSpeed * phase2ChaseMultiplier.
/// - Boss bắn 1 volley gồm phase2BombCount bom (mặc định 5).
/// - Bom rơi từ trên cao về vị trí player (tại thời điểm spawn target).
/// - Khi bắn đến quả thứ 4 & 5, boss "mệt" (stopMovementDuringFinalBombs = true) — đứng yên,
///   player có thể dụ bom nổ trúng boss.
/// - Bom nổ gây damage nếu boss nằm trong bán kính; khi bom nổ sẽ spawn meat pieces (Fish prefab)
///   theo 4 hướng. Meat có script Fish để player/boss có thể ăn.
/// </summary>
public class BossPhase2State : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        routine = boss.StartCoroutine(PhaseRoutine(boss));
    }

    public void Update(Boss boss) { /* logic được xử trong coroutine */ }

    public void Exit(Boss boss)
    {
        if (routine != null)
            boss.StopCoroutine(routine);
    }

    private IEnumerator PhaseRoutine(Boss boss)
    {
        // cache player transform to reduce FindWithTag calls
        Transform playerT = null;

        while (boss.currentHealth > 0)
        {
            // ensure player ref
            if (playerT == null)
            {
                var pgo = GameObject.FindWithTag("Player");
                if (pgo != null) playerT = pgo.transform;
            }

            // 1) chase & shoot volley
            // Boss vừa di chuyển dí player vừa bắn volley periodical
            // Lặp: bắn 5 quả, có interval giữa các quả
            int bombCount = boss.phase2BombCount;
            for (int i = 0; i < bombCount; i++)
            {
                // nếu boss chưa có player, delay 1 frame và continue
                if (playerT == null)
                {
                    yield return null;
                    continue;
                }

                // If this is the 4th or 5th bomb (i index 3,4) we make the boss exhausted
                bool isFinalBombsPhase = (i >= bombCount - boss.phase2FinalBombsThatExhaust); // usually last 2

                // 1.a movement: if not exhausted boss moves toward player at chase speed (frame by frame)
                float shootDelay = boss.phase2BombInterval; // we will still wait this long between bombs
                float elapsed = 0f;

                // We spawn the bomb immediately (so it falls to player's current pos), but during the interval boss moves.
                Vector3 playerPosAtSpawn = playerT.position;
                SpawnBomb(boss, playerPosAtSpawn);

                // if final bombs, set exhausted (boss stops moving) for the duration of this interval
                float moveSpeedThisPhase = isFinalBombsPhase ? 0f : boss.moveSpeed * boss.phase2ChaseMultiplier;

                // Move toward player during the waiting interval (so boss appears to chase while bombs fall)
                while (elapsed < shootDelay)
                {
                    // update playerT maybe moves, but bomb already targeted at player's spawn pos
                    if (moveSpeedThisPhase > 0f && playerT != null)
                    {
                        boss.transform.position = Vector3.MoveTowards(
                            boss.transform.position,
                            playerT.position,
                            moveSpeedThisPhase * Time.deltaTime
                        );
                    }
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // after each bomb, small processing (loop continues)
            }

            // After volley, small pause before next volley
            yield return new WaitForSeconds(boss.phase2VolleyPause);
        }
    }

    private void SpawnBomb(Boss boss, Vector3 targetPosition)
    {
        if (boss.phase2BombPrefab == null) return;

        // Spawn X above target
        var map = Object.FindObjectOfType<MapManager>();
        float topY = (map != null) ? map.topRight.y + boss.phase2BombSpawnHeight : boss.transform.position.y + boss.phase2BombSpawnHeight;
        Vector3 spawnPos = new Vector3(targetPosition.x, topY, targetPosition.z);

        GameObject bomb = Object.Instantiate(boss.phase2BombPrefab, spawnPos, Quaternion.identity);

        // configure bomb
        if (bomb.TryGetComponent<FallingBomb>(out var fb))
        {
            fb.Initialize(
                targetPosition,
                boss.phase2BombFallDuration,
                boss.phase2BombExplodeRadius,
                boss.phase2BombDamage,
                boss.phase2MeatPrefab,
                boss.phase2MeatCount,
                boss // pass boss to allow explosion to damage it
            );
        }
    }
}
