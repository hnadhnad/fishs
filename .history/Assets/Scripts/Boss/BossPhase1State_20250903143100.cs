using System.Collections;
using UnityEngine;

public class BossPhase1State : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        routine = boss.StartCoroutine(PhaseRoutine(boss));
    }

    public void Update(Boss boss)
    {
        // toàn bộ logic nằm trong coroutine
    }

    public void Exit(Boss boss)
    {
        if (routine != null)
            boss.StopCoroutine(routine);
    }

    private IEnumerator PhaseRoutine(Boss boss)
    {
        while (boss.currentHealth > 0)
        {
            // Dash
            for (int i = 0; i < boss.phase1DashCount; i++)
            {
                yield return DashToPlayer(boss);
                yield return new WaitForSeconds(boss.phase1DashInterval);
            }

            // Shoot
            for (int i = 0; i < boss.phase1ShootCount; i++)
            {
                ShootAtPlayer(boss);
                yield return new WaitForSeconds(boss.phase1ShootInterval);
            }

            yield return new WaitForSeconds(boss.phase1CyclePause);

            // hunger <= X → retreat
            if (boss.currentHunger <= boss.maxHunger * boss.phase1RetreatHungerFraction)
            {
                yield return RetreatAndLure(boss);

                // Sau lure → dash 2 lần, bắn 5 lần
                for (int i = 0; i < boss.phase1PostLureDashCount; i++)
                {
                    yield return DashToPlayer(boss);
                    yield return new WaitForSeconds(boss.phase1DashInterval);
                }
                for (int i = 0; i < boss.phase1PostLureShootCount; i++)
                {
                    ShootAtPlayer(boss);
                    yield return new WaitForSeconds(boss.phase1ShootInterval);
                }

                yield return new WaitForSeconds(boss.phase1AfterLurePause);
            }
        }
    }

    private IEnumerator DashToPlayer(Boss boss)
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        Vector3 start = boss.transform.position;
        Vector3 dir = (player.transform.position - start).normalized;
        Vector3 end = start + dir * boss.phase1DashDistance;

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime / boss.phase1DashDuration;
            boss.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        yield return new WaitForSeconds(boss.phase1DashImpactPause);
    }

    private void ShootAtPlayer(Boss boss)
    {
        if (boss.phase1BulletPrefab == null) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        // spawn tại vị trí boss
        Vector3 spawnPos = boss.transform.position;
        Vector3 dir = (player.transform.position - spawnPos).normalized;

        GameObject bullet = GameObject.Instantiate(boss.phase1BulletPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = dir * boss.phase1BulletSpeed;
        }
    }

    private IEnumerator RetreatAndLure(Boss boss)
    {
        MapManager map = GameObject.FindObjectOfType<MapManager>();
        if (map == null) yield break;

        Vector3 pos = boss.transform.position;

        // xác định rìa gần hơn
        float distLeft = Mathf.Abs(pos.x - map.bottomLeft.x);
        float distRight = Mathf.Abs(pos.x - map.topRight.x);
        bool goLeft = distLeft < distRight;

        // boss đi ra ngoài map (để spawn lure)
        float edgeX = goLeft ? map.bottomLeft.x - boss.phase1ExitDistance
                            : map.topRight.x + boss.phase1ExitDistance;
        Vector3 exitPos = new Vector3(edgeX, pos.y, pos.z);

        while (Vector3.Distance(boss.transform.position, exitPos) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                exitPos,
                boss.moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        yield return new WaitForSeconds(boss.phase1OutsideWait);

        // Spawn lure trước mặt boss, tránh dính boss
        GameObject lure = null;
        if (boss.phase1LurePrefab != null)
        {
            Vector3 forward = goLeft ? Vector3.left : Vector3.right;
            Vector3 lurePos = exitPos + forward * boss.phase1LureSpawnForward;

            lure = GameObject.Instantiate(boss.phase1LurePrefab, lurePos, Quaternion.identity);

            // lure bơi ngang map
            Vector3 targetPos = goLeft
                ? new Vector3(map.topRight.x, pos.y, pos.z)
                : new Vector3(map.bottomLeft.x, pos.y, pos.z);

            Rigidbody2D lureRb = lure.GetComponent<Rigidbody2D>();
            if (lureRb != null)
            {
                Vector3 dir = (targetPos - lurePos).normalized;
                lureRb.velocity = dir * boss.phase1LureSpeed;
            }
        }

        float timer = 0f;
        // Boss chase lure trong suốt thời gian claim
        while (timer < boss.phase1LureClaimTime)
        {
            if (lure == null) break; // player đã ăn lure

            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                lure.transform.position,
                boss.chaseLureSpeed * Time.deltaTime
            );

            timer += Time.deltaTime;
            yield return null;
        }

        // Nếu lure còn thì boss ăn
        if (lure != null)
        {
            GameObject.Destroy(lure);
            boss.currentHunger = Mathf.Min(boss.currentHunger + boss.phase1LureHealAmount, boss.maxHunger);
        }

        // boss đảm bảo vào lại trong map
        float reentryX = goLeft ? map.bottomLeft.x + boss.phase1ExitOffset
                                : map.topRight.x - boss.phase1ExitOffset;
        Vector3 reentryPos = new Vector3(reentryX, pos.y, pos.z);

        while (Vector3.Distance(boss.transform.position, reentryPos) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                reentryPos,
                boss.moveSpeed * Time.deltaTime
            );
            yield return null;
        }
    }
}
