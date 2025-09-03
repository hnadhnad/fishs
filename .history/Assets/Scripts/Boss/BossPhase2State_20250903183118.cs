using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2: boss dí theo player với tốc độ = boss.moveSpeed * phase2ChaseMultiplier,
/// spawn volley bombs; bombs khi nổ tạo vùng sát thương (circle).
/// Bomb explosion:
///  - nếu Player trong vùng -> Player.Die() (hoặc prey.Die() tuỳ hệ thống của bạn)
///  - nếu Boss trong vùng -> Boss.TakeDamage(bombDamage)
///  - spawn meat pieces (prefab có Fish) ; mỗi miếng chứa heal = bombDamage / meatCount
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
        // logic chính nằm trong coroutine
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
        // cache player transform
        Transform playerT = null;

        while (boss.currentHealth > 0)
        {
            if (playerT == null)
            {
                var pgo = GameObject.FindWithTag("Player");
                if (pgo != null) playerT = pgo.transform;
            }

            // spawn a volley of bombs
            for (int i = 0; i < boss.phase2BombCount; i++)
            {
                // ensure we have player's current position for target
                Vector3 targetPos = (playerT != null) ? playerT.position : boss.transform.position;

                // spawn bomb which will after fallDuration perform overlap/explode
                SpawnBomb(boss, targetPos);

                // Movement: if we are in the "final bombs that exhaust" phase, boss stands still
                bool isExhaustPhase = (i >= boss.phase2BombCount - boss.phase2FinalBombsThatExhaust);

                float interval = boss.phase2BombInterval;
                float elapsed = 0f;

                while (elapsed < interval)
                {
                    if (!isExhaustPhase && playerT != null)
                    {
                        // boss chases player at scaled speed while waiting for next bomb
                        float moveSpeed = boss.moveSpeed * boss.phase2ChaseMultiplier;
                        boss.transform.position = Vector3.MoveTowards(
                            boss.transform.position,
                            playerT.position,
                            moveSpeed * Time.deltaTime
                        );
                    }
                    // if isExhaustPhase => boss stands still (exhausted) for the interval
                    elapsed += Time.deltaTime;
                    yield return null;
                }
            }

            // after volley wait a bit
            yield return new WaitForSeconds(boss.phase2VolleyPause);
        }
    }

    private void SpawnBomb(Boss boss, Vector3 targetPosition)
    {
        if (boss.phase2BombPrefab == null) return;

        // Spawn the bomb prefab at spawn height above the map top (or above boss if map missing)
        var map = Object.FindObjectOfType<MapManager>();
        float spawnY = (map != null) ? map.topRight.y + boss.phase2BombSpawnHeight : boss.transform.position.y + boss.phase2BombSpawnHeight;
        Vector3 spawnPos = new Vector3(targetPosition.x, spawnY, targetPosition.z);

        GameObject bombGo = Object.Instantiate(boss.phase2BombPrefab, spawnPos, Quaternion.identity);

        // configure bomb component
        if (bombGo.TryGetComponent<FallingBomb>(out var bomb))
        {
            bomb.Configure(
                boss.phase2BombFallDuration,
                boss.phase2BombExplodeRadius,
                boss.phase2BombDamage,
                boss.phase2MeatPrefab,
                boss.phase2MeatCount
            );
        }
        else
        {
            // fallback: if prefab doesn't have FallingBomb, add one
            var added = bombGo.AddComponent<FallingBomb>();
            added.Configure(
                boss.phase2BombFallDuration,
                boss.phase2BombExplodeRadius,
                boss.phase2BombDamage,
                boss.phase2MeatPrefab,
                boss.phase2MeatCount
            );
        }
    }
}
