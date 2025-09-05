using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phase 3:
/// - Boss ra giữa map, spaw n bomb units theo 2 hàng (trái/phải) → di chuyển vào tạo thành vòng tròn cố định (radius configurable).
/// - Bomb units là "tường cứng" (collider không trigger) giữ thành vòng. Boss dash liên tục (dùng thông số phase3Dash*).
/// - Nếu boss đâm vào bomb (khoảng cách <= collisionThreshold) — coi là "đâm trúng":
///     - spawn meatCount miếng thịt (phải spawn **bên trong** vòng bôm).
///     - stun boss (boss.Stun), boss không dash, sau đó boss đi ăn meat như Phase2 trước khi tiếp tục.
/// - Sau ăn hết meat, boss nghỉ phase3RestAfterMeat rồi quay lại dash loop.
/// </summary>
public class BossPhase3State : IBossState
{
    private Coroutine routine;
    // lưu bombs để kiểm tra collision / dọn khi thoát
    private List<GameObject> spawnedBombs = new List<GameObject>();

    public void Enter(Boss boss)
    {
        if (routine != null) boss.StopCoroutine(routine);
        // start coroutine
        routine = boss.StartCoroutine(Phase3Routine(boss));
    }

    public void Update(Boss boss) { }

    public void Exit(Boss boss)
    {
        if (routine != null)
        {
            boss.StopCoroutine(routine);
            routine = null;
        }

        // dọn bombs khi rời phase
        foreach (var b in spawnedBombs)
            if (b != null) Object.Destroy(b);
        spawnedBombs.Clear();
    }

    private IEnumerator Phase3Routine(Boss boss)
    {
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null) yield break;

        // vị trí giữa map
        Vector3 center = (Vector3)((map.bottomLeft + map.topRight) / 2f);
        center.z = boss.transform.position.z;

        // 1) Boss đi về giữa map
        yield return MoveTo(boss, center, boss.moveSpeed);

        // 2) Spawn và move bombs từ 2 bên vào vị trí vòng tròn
        SpawnAndArrangeBombs(boss, center);

        // đợi cho bombs vào vị trí (chuyển động riêng) → trong hàm SpawnAndArrangeBombs đã chèn coroutine
        // nhưng cần chờ cho tất cả bombs đã reach target
        yield return WaitForBombsInPosition();

        // 3) Main loop: dash liên tục, kiểm tra đâm trúng bomb
        while (boss != null && boss.currentHealth > 0f)
        {
            // dash liên tục (vô hạn) cho tới khi đâm trúng bomb -> break để xử lý ăn thịt
            bool hitBomb = false;
            // vô hạn dash loop: dash → delay → dash ...
            while (!hitBomb && boss != null && boss.currentHealth > 0f)
            {
                // dash towards player
                var player = GameObject.FindWithTag("Player");
                Vector3 target = (player != null) ? player.transform.position : boss.transform.position;
                // compute dash end point
                Vector3 start = boss.transform.position;
                Vector3 dir = (target - start).normalized;
                Vector3 end = start + dir * boss.phase3DashDistance;

                float t = 0f;
                while (t < 1f)
                {
                    // sprint interpolation
                    t += Time.deltaTime / Mathf.Max(0.0001f, boss.phase3DashDuration);
                    Vector3 newPos = Vector3.Lerp(start, end, t);
                    // check collision with any bomb BEFORE moving (distance threshold)
                    foreach (var b in spawnedBombs)
                    {
                        if (b == null) continue;
                        float d = Vector3.Distance(newPos, b.transform.position);
                        if (d <= boss.phase3BombCollisionThreshold)
                        {
                            // hit bomb
                            hitBomb = true;
                            // move boss to slightly inside circle (so meat spawn inside)
                            boss.transform.position = newPos;
                            break;
                        }
                    }
                    if (hitBomb) break;

                    boss.transform.position = newPos;
                    yield return null;
                }

                if (hitBomb)
                {
                    // spawn meat inside circle at collision point (we pick boss position)
                    SpawnMeatOnBombHit(boss);
                    // boss stunned
                    boss.Stun(boss.phase3BombStunDuration);
                    // wait until stun over (and possibly eat meat in EatAllMeat)
                    yield break; // break out so outer loop can handle (we will let Phase3 re-enter after EatAllMeat)
                }

                // pause between dashes
                yield return new WaitForSeconds(boss.phase3DashInterval);
            }

            yield return null;
        }
    }

    // helper: move boss to a target position with speed
    private IEnumerator MoveTo(Boss boss, Vector3 target, float speed)
    {
        while (boss != null && Vector3.Distance(boss.transform.position, target) > 0.05f)
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }

    // Spawns bombs at two vertical columns (left/right) then moves them to circle positions.
// Spawns bombs at two vertical columns (left/right) then moves them to circle positions.
// Spawns bombs at two vertical columns (left/right) then moves them to circle positions.
// Spawns bombs at two vertical columns (left/right) then moves them into a circle around center
    private void SpawnAndArrangeBombs(Boss boss, Vector3 center)
    {
        spawnedBombs.Clear();

        int total = Mathf.Max(4, boss.phase3BombCount);
        int half = total / 2;

        // === target positions thành vòng tròn ===
        Vector3[] circleTargets = new Vector3[total];
        float stepDeg = 360f / total;
        for (int i = 0; i < total; i++)
        {
            float ang = (stepDeg * i) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            circleTargets[i] = center + dir * boss.phase3CircleRadius;
        }

        var map = Object.FindObjectOfType<MapManager>();
        float mapHalfHeight = map.MapSize.y / 2f;
        float columnSpacingY = Mathf.Min(mapHalfHeight * 0.9f, boss.phase3ColumnHeight);

        float leftX = map.bottomLeft.x - boss.phase3BombSpawnOffscreen;
        float rightX = map.topRight.x + boss.phase3BombSpawnOffscreen;

        // === Spawn cột trái (nửa vòng bên trái: 90° → 270°) ===
        for (int i = 0; i < half; i++)
        {
            float y = center.y + Mathf.Lerp(columnSpacingY * 0.5f, -columnSpacingY * 0.5f, (float)i / (half - 1));
            Vector3 spawnPos = new Vector3(leftX, y, center.z);

            GameObject b = Object.Instantiate(boss.phase3BombPrefab, spawnPos, Quaternion.identity);
            spawnedBombs.Add(b);

            if (b.TryGetComponent<Phase3Bomb>(out var bombComp))
                bombComp.bombRadius = boss.phase3BombRadius;

            // mapping vào nửa vòng bên trái
            int targetIndex = i; // 0..half-1
            Vector3 target = circleTargets[targetIndex];

            // ✅ giữ nguyên Y spawn, chỉ đổi X theo target
            Vector3 finalTarget = new Vector3(target.x, spawnPos.y, spawnPos.z);

            boss.StartCoroutine(MoveBombTo(b, finalTarget, boss.phase3BombMoveDuration));
        }

        // === Spawn cột phải (nửa vòng bên phải: 270° → 90°) ===
        for (int i = 0; i < half; i++)
        {
            float y = center.y + Mathf.Lerp(columnSpacingY * 0.5f, -columnSpacingY * 0.5f, (float)i / (half - 1));
            Vector3 spawnPos = new Vector3(rightX, y, center.z);

            GameObject b = Object.Instantiate(boss.phase3BombPrefab, spawnPos, Quaternion.identity);
            spawnedBombs.Add(b);

            if (b.TryGetComponent<Phase3Bomb>(out var bombComp))
                bombComp.bombRadius = boss.phase3BombRadius;

            // mapping vào nửa vòng bên phải
            int targetIndex = i + half; // half..total-1
            Vector3 target = circleTargets[targetIndex];

            // ✅ giữ nguyên Y spawn, chỉ đổi X theo target
            Vector3 finalTarget = new Vector3(target.x, spawnPos.y, spawnPos.z);

            boss.StartCoroutine(MoveBombTo(b, finalTarget, boss.phase3BombMoveDuration));
        }
    }










    private IEnumerator MoveBombTo(GameObject bomb, Vector3 target, float duration)
    {
        if (bomb == null) yield break;
        Vector3 start = bomb.transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            if (bomb == null) yield break;
            bomb.transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        // ensure final
        if (bomb != null) bomb.transform.position = target;
    }

    // wait until all bombs are roughly in final circle positions (check small velocity)
    private IEnumerator WaitForBombsInPosition()
    {
        // simple wait short time (bomb move duration already fired) — safety: wait 1s
        yield return new WaitForSeconds(0.6f);
    }

    // Spawn meat around boss position (but inside the circle) when boss hits bomb
    public void SpawnMeatOnBombHit(Boss boss)
    {
        Vector3 spawnCenter = boss.transform.position; // we use boss current pos (should be inside circle)
        int count = Mathf.Max(1, boss.phase3MeatCount);
        float step = 360f / count;
        float offset = boss.phase3MeatSpawnOffset;
        float healPerPiece = boss.phase3BombHitDamage / Mathf.Max(1, count);

        for (int i = 0; i < count; i++)
        {
            float ang = (step * i) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            Vector3 pos = spawnCenter + dir * offset;
            GameObject piece = Object.Instantiate(boss.meatPrefab, pos, Quaternion.identity);
            if (!piece.TryGetComponent<MeatPiece>(out var mp))
                mp = piece.AddComponent<MeatPiece>();
            mp.SetHealAmount(healPerPiece);
        }

        // optionally destroy all bombs? we keep bombs (they stay as walls). If you want bombs gone, uncomment:
        // foreach (var b in spawnedBombs) if (b != null) Object.Destroy(b);
        // spawnedBombs.Clear();
    }
}
