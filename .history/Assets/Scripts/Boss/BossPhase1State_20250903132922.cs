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
        // không cần code, toàn bộ logic trong coroutine
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

            // Nếu hunger <= ngưỡng → retreat + lure
            if (boss.currentHunger <= boss.maxHunger * boss.phase1RetreatHungerFraction)
            {
                yield return RetreatAndLure(boss);

                // Sau lure → dash + shoot bắt buộc
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

        Transform spawn = boss.phase1BulletSpawn != null ? boss.phase1BulletSpawn : boss.transform;
        Vector3 dir = (player.transform.position - spawn.position).normalized;

        GameObject bullet = GameObject.Instantiate(boss.phase1BulletPrefab, spawn.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = dir * boss.phase1BulletSpeed;
        }
        // BossSkillHitbox trên prefab tự xử lý va chạm
    }

    private IEnumerator RetreatAndLure(Boss boss)
    {
        MapManager map = GameObject.FindObjectOfType<MapManager>();
        if (map == null) yield break;

        Vector3 pos = boss.transform.position;

        // Xác định rìa trái/phải gần hơn
        float distLeft = Mathf.Abs(pos.x - map.bottomLeft.x);
        float distRight = Mathf.Abs(pos.x - map.topRight.x);

        bool goLeft = distLeft < distRight;
        float edgeX = goLeft ? map.bottomLeft.x - boss.phase1ExitDistance : map.topRight.x + boss.phase1ExitDistance;
        Vector3 exitPos = new Vector3(edgeX, pos.y, pos.z);

        // Đi ra ngoài map
        while (Vector3.Distance(boss.transform.position, exitPos) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, exitPos, boss.phase1ExitSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(boss.phase1OutsideWait);

        // Spawn lure
        if (boss.phase1LurePrefab != null)
        {
            Vector3 lurePos = exitPos + new Vector3(goLeft ? -boss.phase1LureSpawnForward : boss.phase1LureSpawnForward, 0, 0);
            GameObject lure = GameObject.Instantiate(boss.phase1LurePrefab, lurePos, Quaternion.identity);

            // Cho lure chạy vào map
            Vector3 targetPos = new Vector3(goLeft ? map.topRight.x : map.bottomLeft.x, pos.y, pos.z);
            Rigidbody2D lureRb = lure.GetComponent<Rigidbody2D>();
            if (lureRb != null)
            {
                Vector3 dir = (targetPos - lurePos).normalized;
                lureRb.velocity = dir * boss.phase1LureSpeed;
            }

            // Cho player có thời gian ăn lure
            yield return new WaitForSeconds(boss.phase1LureClaimTime);

            if (lure != null) // boss ăn
            {
                Fish lureFish = lure.GetComponent<Fish>();
                if (lureFish != null)
                {
                    boss.currentHunger = Mathf.Min(boss.currentHunger + lureFish.hungerValue, boss.maxHunger);
                }
                GameObject.Destroy(lure);
            }
        }

        // Boss quay lại map
        Vector3 reentryPos = pos;
        while (Vector3.Distance(boss.transform.position, reentryPos) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, reentryPos, boss.phase1ReturnSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
