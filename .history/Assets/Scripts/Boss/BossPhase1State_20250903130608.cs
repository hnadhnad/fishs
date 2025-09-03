using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 1 behaviour:
/// - Luân phiên: dash vào player (config số lần) rồi dừng bắn (config số lần). Lặp lại.
/// - Khi hunger <= retreatHungerFraction * maxHunger (mình hiểu là "còn 1 nửa"): boss ra rìa gần nhất, đi ra khỏi map,
///   rồi quay lại cùng 1 prefab cá (lure). Player có thể ăn con lure trước boss. Nếu boss ăn lure -> boss hồi máu.
/// - Dù ăn được hay không, boss sau đó dash (postLureDashCount) và bắn (postLureShootCount) rồi mới xét lại thanh đói.
/// </summary>
public class BossPhase1State : IBossState
{
    private Coroutine runningRoutine;

    public void Enter(Boss boss)
    {
        Debug.Log("Boss vào Phase 1");
        // Start main behaviour loop
        runningRoutine = boss.StartCoroutine(PhaseRoutine(boss));
    }

    public void Update(Boss boss)
    {
        // Behaviour handled by coroutine
    }

    public void Exit(Boss boss)
    {
        Debug.Log("Boss rời Phase 1");
        if (runningRoutine != null)
        {
            boss.StopCoroutine(runningRoutine);
            runningRoutine = null;
        }
    }

    // Main sequence
    private IEnumerator PhaseRoutine(Boss boss)
    {
        // references
        Transform bossT = boss.transform;
        var map = Object.FindObjectOfType<MapManager>();

        while (boss.currentPhase == BossPhase.Phase1)
        {
            // 1) Dash X lần -> then Shoot Y lần (repeat until hunger low)
            while (boss.currentHunger > boss.maxHunger * boss.phase1RetreatHungerFraction)
            {
                // Dash sequence
                for (int d = 0; d < boss.phase1DashCount; d++)
                {
                    yield return boss.StartCoroutine(DoDashTowardsPlayer(boss));
                    yield return new WaitForSeconds(boss.phase1DashInterval);
                }

                // Shoot sequence
                for (int s = 0; s < boss.phase1ShootCount; s++)
                {
                    ShootAtPlayer(boss);
                    yield return new WaitForSeconds(boss.phase1ShootInterval);
                }

                // small pause between cycles
                yield return new WaitForSeconds(boss.phase1CyclePause);
            }

            // Hunger low -> retreat to nearest horizontal edge and go out of map
            // choose left (-x) or right (+x) nearest by distance to center of map or by boss x vs map center
            Vector2 exitTarget = Vector2.zero;
            if (map != null)
            {
                float mapCenterX = (map.bottomLeft.x + map.topRight.x) * 0.5f;
                bool goRight = bossT.position.x >= mapCenterX;
                float exitX = goRight ? map.topRight.x + boss.phase1ExitOffset : map.bottomLeft.x - boss.phase1ExitOffset;
                // keep same y
                exitTarget = new Vector2(exitX, bossT.position.y);
            }
            else
            {
                // fallback: move far to right
                exitTarget = new Vector2(bossT.position.x + (bossT.position.x >= 0 ? 30f : -30f), bossT.position.y);
            }

            // Move to edge (smooth)
            yield return boss.StartCoroutine(MoveToPosition(bossT, exitTarget, boss.phase1ExitSpeed));

            // Move further out (leave map)
            Vector2 outsideTarget = exitTarget + new Vector2((exitTarget.x > bossT.position.x) ? boss.phase1ExitDistance : -boss.phase1ExitDistance, 0f);
            yield return boss.StartCoroutine(MoveToPosition(bossT, outsideTarget, boss.phase1ExitSpeed));

            // Wait a bit outside
            yield return new WaitForSeconds(boss.phase1OutsideWait);

            // Spawn lure fish in front of boss and re-enter slowly
            Vector2 enterStart = outsideTarget;
            Vector2 enterEnd = new Vector2(Mathf.Clamp((map != null ? (map.bottomLeft.x + map.topRight.x) * 0.5f : 0f), -9999f, 9999f), bossT.position.y);
            // Spawn lure in front of boss (a little ahead in x depending on side)
            Vector2 lureSpawnPos = outsideTarget + new Vector2((outsideTarget.x > 0) ? -boss.phase1LureSpawnForward : boss.phase1LureSpawnForward, 0f);
            GameObject lure = null;
            if (boss.phase1LurePrefab != null)
            {
                lure = Object.Instantiate(boss.phase1LurePrefab, lureSpawnPos, Quaternion.identity);
                // try set speed if lure has component controlling movement
                var mover = lure.GetComponent<LureMover>();
                if (mover != null)
                {
                    mover.SetSpeed(boss.phase1LureSpeed);
                    mover.SetDirectionIntoMap(outsideTarget.x > 0 ? Vector2.left : Vector2.right);
                }
                else
                {
                    // If not, try to move it manually with a simple mover
                    var rb = lure.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        float dir = (outsideTarget.x > 0) ? -1f : 1f;
                        rb.velocity = new Vector2(dir * boss.phase1LureSpeed, 0f);
                    }
                }
            }

            // Slowly re-enter map along a short path (boss follows lure behind it)
            Vector2 bossReturnTarget = new Vector2(Mathf.Clamp((map != null ? (map.bottomLeft.x + map.topRight.x) * 0.5f : 0f), -9999f, 9999f), bossT.position.y);
            // we will move boss from outsideTarget toward bossReturnTarget at return speed
            yield return boss.StartCoroutine(MoveToPosition(bossT, bossReturnTarget, boss.phase1ReturnSpeed));

            // Now wait for player to try eat lure within a window (lureClaimTime)
            bool playerAte = false;
            float t = 0f;
            while (t < boss.phase1LureClaimTime)
            {
                if (lure == null) { playerAte = true; break; } // lure destroyed => probably player ate it
                t += Time.deltaTime;
                yield return null;
            }

            // If lure still alive after window -> boss eats it
            if (!playerAte && lure != null)
            {
                // boss eats lure (if boss has a Fish component, use Eat to benefit from hunger/heal handling)
                Fish bossFish = boss.GetComponent<Fish>();
                Fish preyFish = lure.GetComponent<Fish>();
                if (bossFish != null && preyFish != null)
                {
                    bossFish.Eat(preyFish); // this will call prey.Die() internally
                    // optionally heal boss
                    boss.currentHealth = Mathf.Clamp(boss.currentHealth + boss.phase1LureHealAmount, 0f, boss.maxHealth);
                }
                else
                {
                    // fallback: destroy lure
                    Object.Destroy(lure);
                    boss.currentHealth = Mathf.Clamp(boss.currentHealth + boss.phase1LureHealAmount, 0f, boss.maxHealth);
                }
            }

            // regardless of outcome, perform post-lure actions: dash and shoot fixed counts
            for (int d = 0; d < boss.phase1PostLureDashCount; d++)
            {
                yield return boss.StartCoroutine(DoDashTowardsPlayer(boss));
                yield return new WaitForSeconds(boss.phase1DashInterval);
            }

            for (int s = 0; s < boss.phase1PostLureShootCount; s++)
            {
                ShootAtPlayer(boss);
                yield return new WaitForSeconds(boss.phase1ShootInterval);
            }

            // After post-lure sequence, allow gap and continue loop (will check hunger again)
            yield return new WaitForSeconds(boss.phase1AfterLurePause);
        }
    }

    // Moves boss transform to target smoothly at given speed (units/sec). Completes when distance < 0.05.
    private IEnumerator MoveToPosition(Transform t, Vector2 target, float speed)
    {
        while ((Vector2)t.position != (Vector2)target)
        {
            Vector2 pos = Vector2.MoveTowards(t.position, target, speed * Time.deltaTime);
            t.position = pos;
            if (Vector2.Distance(t.position, target) < 0.05f) break;
            yield return null;
        }
        yield return null;
    }

    // Simple dash coroutine: dash toward player a short quick move (telegraphed by setting position directly)
    private IEnumerator DoDashTowardsPlayer(Boss boss)
    {
        // parameters from boss
        float dashDuration = boss.phase1DashDuration;
        float dashDistance = boss.phase1DashDistance;
        Transform bossT = boss.transform;

        // find player
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            yield break;
        }
        Vector2 start = bossT.position;
        Vector2 dir = ((Vector2)playerGO.transform.position - start).normalized;
        Vector2 target = start + dir * dashDistance;

        float elapsed = 0f;
        // Optionally trigger animation
        boss.animator?.SetTrigger("Dash");

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            bossT.position = Vector2.Lerp(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bossT.position = target;

        // small impact time
        yield return new WaitForSeconds(boss.phase1DashImpactPause);
    }

    // Spawn a bullet aimed at player
    private void ShootAtPlayer(Boss boss)
    {
        if (boss.phase1BulletPrefab == null) return;

        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null) return;

        Vector2 spawnPos = boss.phase1BulletSpawn != null ? boss.phase1BulletSpawn.position : boss.transform.position;
        Vector2 dir = ((Vector2)playerGO.transform.position - spawnPos).normalized;

        GameObject b = Object.Instantiate(boss.phase1BulletPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = dir * boss.phase1BulletSpeed;
        }
        // ensure hitbox flag is set on bullet's BossSkillHitbox (optional)
        var hit = b.GetComponent<BossSkillHitbox>();
        if (hit != null) hit.isChieu = true;

        // trigger shoot anim
        boss.animator?.SetTrigger("Shoot");
    }
}
