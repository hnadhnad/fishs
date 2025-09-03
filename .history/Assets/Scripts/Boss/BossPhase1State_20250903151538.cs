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
            // Dash loop
            yield return RepeatAction(boss.phase1DashCount, () => DashToPlayer(boss), boss.phase1DashInterval);

            // Shoot loop
            yield return RepeatAction(boss.phase1ShootCount, () => { ShootAtPlayer(boss); return null; }, boss.phase1ShootInterval);

            yield return new WaitForSeconds(boss.phase1CyclePause);

            // hunger thấp → retreat
            if (boss.currentHunger <= boss.maxHunger * boss.phase1RetreatHungerFraction)
            {
                yield return RetreatAndLure(boss);

                // Dash + shoot sau lure
                yield return RepeatAction(boss.phase1PostLureDashCount, () => DashToPlayer(boss), boss.phase1DashInterval);
                yield return RepeatAction(boss.phase1PostLureShootCount, () => { ShootAtPlayer(boss); return null; }, boss.phase1ShootInterval);

                yield return new WaitForSeconds(boss.phase1AfterLurePause);
            }
        }
    }

    /// <summary>
    /// Lặp lại hành động N lần, có delay giữa các lần.
    /// </summary>
    private IEnumerator RepeatAction(int count, System.Func<IEnumerator> action, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            IEnumerator routine = action?.Invoke();
            if (routine != null) yield return routine;
            if (interval > 0) yield return new WaitForSeconds(interval);
        }
    }

    private IEnumerator DashToPlayer(Boss boss)
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        Vector3 start = boss.transform.position;
        Vector3 dir = (player.transform.position - start).normalized;
        Vector3 end = start + dir * boss.phase1DashDistance;

        float t = 0f;
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

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        Vector3 dir = (player.transform.position - boss.transform.position).normalized;
        Vector3 spawnPos = boss.transform.position + dir * boss.phase1BulletSpawnOffset;

        GameObject bullet = Object.Instantiate(boss.phase1BulletPrefab, spawnPos, Quaternion.identity);
        if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = dir * boss.phase1BulletSpeed;
        }
    }

    private IEnumerator RetreatAndLure(Boss boss)
    {
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null) yield break;

        Vector3 pos = boss.transform.position;

        // 1. chọn hướng ngắn hơn
        float distLeft = Mathf.Abs(pos.x - map.bottomLeft.x);
        float distRight = Mathf.Abs(pos.x - map.topRight.x);
        bool goLeft = distLeft < distRight;

        // 2. boss đi ra ngoài map
        float edgeX = goLeft ? map.bottomLeft.x - boss.phase1ExitDistance
                             : map.topRight.x + boss.phase1ExitDistance;
        Vector3 outsidePos = new Vector3(edgeX, pos.y, pos.z);
        yield return MoveTo(boss, outsidePos, boss.moveSpeed);

        // 3. boss di chuyển xuống Y giữa
        float midY = (map.bottomLeft.y + map.topRight.y) / 2f;
        Vector3 midPos = new Vector3(outsidePos.x, midY, pos.z);
        yield return MoveTo(boss, midPos, boss.moveSpeed);

        // 4. spawn lure
        GameObject lure = null;
        if (boss.phase1LurePrefab != null)
        {
            Vector3 forward = goLeft ? Vector3.right : Vector3.left;
            Vector3 lurePos = midPos + forward * boss.phase1LureSpawnForward;

            lure = Object.Instantiate(boss.phase1LurePrefab, lurePos, Quaternion.identity);

            if (lure.TryGetComponent<LureFish>(out var lf))
            {
                lf.moveDirection = forward;
                lf.speed = boss.phase1LureSpeed;
            }
        }

        // 5. đuổi lure cho đến khi ăn
        while (lure != null)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                lure.transform.position,
                boss.chaseLureSpeed * Time.deltaTime
            );

            if (Vector3.Distance(boss.transform.position, lure.transform.position) < 0.5f)
            {
                Object.Destroy(lure);
                lure = null;
                boss.currentHunger = boss.maxHunger;
                break;
            }

            yield return null;
        }
    }

    private IEnumerator MoveTo(Boss boss, Vector3 target, float speed)
    {
        while (Vector3.Distance(boss.transform.position, target) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                target,
                speed * Time.deltaTime
            );
            yield return null;
        }
    }
}
