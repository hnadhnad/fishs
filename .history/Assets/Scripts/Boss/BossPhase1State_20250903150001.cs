using System.Collections;
using UnityEngine;

public class BossPhase1State : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        routine = boss.StartCoroutine(PhaseRoutine(boss));
    }

    public void Update(Boss boss) { }

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

                // Sau lure → dash + shoot
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

        Vector3 dir = (player.transform.position - boss.transform.position).normalized;

        // spawn cách boss 1 đoạn về phía player
        Vector3 spawnPos = boss.transform.position + dir * boss.phase1BulletSpawnOffset;

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

        // 1. Xác định bên gần nhất
        float distLeft = Mathf.Abs(pos.x - map.bottomLeft.x);
        float distRight = Mathf.Abs(pos.x - map.topRight.x);
        bool goLeft = distLeft < distRight;

        // 2. Boss đi ra ngoài map theo X (khuất player không thấy)
        float edgeX = goLeft ? map.bottomLeft.x - boss.phase1ExitDistance
                            : map.topRight.x + boss.phase1ExitDistance;
        Vector3 outsidePos = new Vector3(edgeX, pos.y, pos.z);

        while (Vector3.Distance(boss.transform.position, outsidePos) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                outsidePos,
                boss.moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 3. Boss di chuyển xuống giữa map theo Y
        float midY = (map.bottomLeft.y + map.topRight.y) / 2f;
        Vector3 midPos = new Vector3(outsidePos.x, midY, pos.z);

        while (Vector3.Distance(boss.transform.position, midPos) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                midPos,
                boss.moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 4. Spawn lure ngay trước mặt boss
        GameObject lure = null;
        if (boss.phase1LurePrefab != null)
        {
            Vector3 forward = goLeft ? Vector3.right : Vector3.left;
            Vector3 lurePos = midPos + forward * boss.phase1LureSpawnForward;

            lure = GameObject.Instantiate(boss.phase1LurePrefab, lurePos, Quaternion.identity);

            // setup hướng cho lure
            LureFish lf = lure.GetComponent<LureFish>();
            if (lf != null)
            {
                lf.moveDirection = forward; // bơi ngang map
                lf.speed = boss.phase1LureSpeed;
            }
        }

        // 5. Boss đuổi theo lure cho đến khi ăn
        while (lure != null)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                lure.transform.position,
                boss.chaseLureSpeed * Time.deltaTime
            );

            // Nếu boss chạm lure
            if (Vector3.Distance(boss.transform.position, lure.transform.position) < 0.5f)
            {
                GameObject.Destroy(lure);
                lure = null;
                boss.currentHunger = boss.maxHunger; // hồi đầy hunger
                break;
            }

            yield return null;
        }
    }




}
