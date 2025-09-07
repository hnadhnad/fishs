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

    // lock để tránh spawn meat nhiều lần liên tiếp nếu nhiều bomb va chạm gần nhau
    private bool meatSpawnLock = false;

    public void Enter(Boss boss)
    {
        // hunger reset...
        boss.currentHunger = boss.maxHunger;
        boss.hungerDecayRate = 0f;

        if (boss.hungerBar != null)
            boss.hungerBar.gameObject.SetActive(false);

        if (routine != null) boss.StopCoroutine(routine);

        // ✅ chỉ làm setup 1 lần ở Enter
        routine = boss.StartCoroutine(Phase3Setup(boss));
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

    private IEnumerator Phase3DashLoop(Boss boss)
    {
        while (boss != null && boss.currentHealth > 0f)
        {
            var player = GameObject.FindWithTag("Player");
            Vector3 target = (player != null) ? player.transform.position : boss.transform.position;

            // Gọi dash routine chung (boss.DashRoutine là phương thức chung do bạn đã thêm)
            yield return boss.StartCoroutine(
                boss.DashRoutine(target, boss.phase3DashDistance, boss.phase3DashDuration)
            );

            // Kiểm tra xem Boss có bị stun do đâm bomb không
            if (boss.IsStunned)
            {
                yield break; // Boss bị choáng → nhường control cho Phase3AfterStun
            }

            // Nếu không bị stun → nghỉ 1 chút rồi dash tiếp
            yield return new WaitForSeconds(boss.phase3DashInterval);
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

    // Spawns bombs at two vertical columns (left/right) then moves them into a circle around center
    private void SpawnAndArrangeBombs(Boss boss, Vector3 center)
    {
        spawnedBombs.Clear();

        int total = Mathf.Max(4, boss.phase3BombCount);
        float stepDeg = 360f / total;

        // 1) Tạo tất cả target trên vòng tròn
        List<Vector3> allTargets = new List<Vector3>(total);
        for (int i = 0; i < total; i++)
        {
            float angDeg = 90f - stepDeg * i;
            float ang = angDeg * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            allTargets.Add(center + dir * boss.phase3CircleRadius);
        }

        // 2) Phân tách trái / phải
        List<Vector3> leftTargets = new List<Vector3>();
        List<Vector3> rightTargets = new List<Vector3>();
        foreach (var t in allTargets)
        {
            if (t.x < center.x) leftTargets.Add(t);
            else rightTargets.Add(t);
        }

        // 3) Sort từ trên xuống
        leftTargets.Sort((a, b) => b.y.CompareTo(a.y));
        rightTargets.Sort((a, b) => b.y.CompareTo(a.y));

        // 4) Defensive split nếu lệch
        if (leftTargets.Count == 0 || rightTargets.Count == 0)
        {
            leftTargets.Clear();
            rightTargets.Clear();
            for (int i = 0; i < total; i++)
            {
                if (i < total / 2) leftTargets.Add(allTargets[i]);
                else rightTargets.Add(allTargets[i]);
            }
            leftTargets.Sort((a, b) => b.y.CompareTo(a.y));
            rightTargets.Sort((a, b) => b.y.CompareTo(a.y));
        }

        var map = Object.FindObjectOfType<MapManager>();
        float leftX = map.bottomLeft.x - boss.phase3BombSpawnOffscreen;
        float rightX = map.topRight.x + boss.phase3BombSpawnOffscreen;

        int leftCount = leftTargets.Count;
        int rightCount = rightTargets.Count;

        // ✅ Spawn left column (trải full chiều cao map)
        for (int i = 0; i < leftCount; i++)
        {
            float tNorm = (leftCount == 1) ? 0.5f : (float)i / (leftCount - 1);
            float y = Mathf.Lerp(map.topRight.y, map.bottomLeft.y, tNorm); // full map height
            Vector3 spawnPos = new Vector3(leftX, y, center.z);

            GameObject b = Object.Instantiate(boss.phase3BombPrefab, spawnPos, Quaternion.identity);
            spawnedBombs.Add(b);

            if (b.TryGetComponent<Phase3Bomb>(out var bombComp))
            {
                bombComp.ApplyRadius(boss.phase3BombRadius);
            }

            boss.StartCoroutine(MoveBombTo(b, leftTargets[i], boss.phase3BombMoveDuration));
        }

        // ✅ Spawn right column (trải full chiều cao map)
        for (int i = 0; i < rightCount; i++)
        {
            float tNorm = (rightCount == 1) ? 0.5f : (float)i / (rightCount - 1);
            float y = Mathf.Lerp(map.topRight.y, map.bottomLeft.y, tNorm); // full map height
            Vector3 spawnPos = new Vector3(rightX, y, center.z);

            GameObject b = Object.Instantiate(boss.phase3BombPrefab, spawnPos, Quaternion.identity);
            spawnedBombs.Add(b);

            if (b.TryGetComponent<Phase3Bomb>(out var bombComp))
            {
                bombComp.ApplyRadius(boss.phase3BombRadius);
            }

            boss.StartCoroutine(MoveBombTo(b, rightTargets[i], boss.phase3BombMoveDuration));
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
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null) return;
        Vector3 center = (Vector3)((map.bottomLeft + map.topRight) / 2f);

        Vector3 bossPos = boss.transform.position;
        float R = boss.phase3CircleRadius;            // bán kính vòng bomb
        float r = boss.phase3MeatSpawnRadius;         // bán kính spawn quanh boss
        int count = Mathf.Max(1, boss.phase3MeatCount);
        float healPerPiece = boss.phase3BombHitDamage / Mathf.Max(1, count);
        float paddingFraction = boss.phase3MeatArcPadding; // phần trăm cung bỏ qua 2 bên

        float d = Vector2.Distance(new Vector2(center.x, center.y), new Vector2(bossPos.x, bossPos.y));

        // Nếu boss nằm hoàn toàn trong vòng → spawn full circle
        if (d + r <= R)
        {
            float step = 2f * Mathf.PI / count;
            for (int i = 0; i < count; i++)
            {
                float ang = step * i;
                Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
                Vector3 pos = bossPos + dir * r;

                var piece = Object.Instantiate(boss.meatPrefab, pos, Quaternion.identity);
                if (!piece.TryGetComponent<MeatPiece>(out var mp)) mp = piece.AddComponent<MeatPiece>();
                mp.SetHealAmount(healPerPiece);
            }
            return;
        }

        // Nếu không giao nhau → bỏ
        if (d >= R + r) return;

        // Circle-circle intersection
        float a = (r * r - R * R + d * d) / (2f * d);
        float h2 = r * r - a * a;
        if (h2 < 0f) h2 = 0f; // ép về 0 thay vì return
        float h = Mathf.Sqrt(h2);

        Vector3 dirSC = (center - bossPos) / d;
        Vector3 p0 = bossPos + dirSC * a;
        Vector3 perp = new Vector3(-dirSC.y, dirSC.x, 0f);

        Vector3 p1 = p0 + perp * h;
        Vector3 p2 = p0 - perp * h;

        float a1 = Mathf.Atan2(p1.y - bossPos.y, p1.x - bossPos.x);
        float a2 = Mathf.Atan2(p2.y - bossPos.y, p2.x - bossPos.x);

        if (a1 < 0f) a1 += 2f * Mathf.PI;
        if (a2 < 0f) a2 += 2f * Mathf.PI;
        if (a2 < a1) a2 += 2f * Mathf.PI;

        float span = a2 - a1;
        float midAngle = a1 + span * 0.5f;
        Vector3 midPoint = bossPos + new Vector3(Mathf.Cos(midAngle), Mathf.Sin(midAngle), 0f) * r;
        bool midInside = Vector2.Distance(new Vector2(midPoint.x, midPoint.y), new Vector2(center.x, center.y)) <= R;

        float startAngle, arcSpan;
        if (midInside)
        {
            startAngle = a1;
            arcSpan = span;
        }
        else
        {
            startAngle = a2;
            arcSpan = 2f * Mathf.PI - span;
        }

        // ✅ padding
        float pad = arcSpan * paddingFraction;
        float effectiveArc = arcSpan - 2f * pad;

        for (int i = 0; i < count; i++)
        {
            float t = (count == 1) ? 0.5f : (float)i / (count - 1); // đảm bảo luôn đủ count
            float ang = startAngle + pad + effectiveArc * t;

            Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
            Vector3 pos = bossPos + dir * r;

            var piece = Object.Instantiate(boss.meatPrefab, pos, Quaternion.identity);
            if (!piece.TryGetComponent<MeatPiece>(out var mp)) mp = piece.AddComponent<MeatPiece>();
            mp.SetHealAmount(healPerPiece);
        }
    }

    /// <summary>
    /// API an toàn để được gọi bởi Phase3Bomb khi bomb chạm boss.
    /// Bảo đảm chỉ spawn meat 1 lần trong khoảng nhỏ (debounce).
    /// </summary>
    public void TryHandleBombHit(Boss boss)
    {
        if (meatSpawnLock) return;

        meatSpawnLock = true;

        // spawn meat
        SpawnMeatOnBombHit(boss);

        // start Phase3AfterStun (boss sẽ ăn thịt, sau đó mới reset lock)
        boss.StartCoroutine(Phase3AfterStun(boss));
    }


    private IEnumerator ResetMeatSpawnLockCoroutine()
    {
        // 0.15s đủ ngắn để ngăn double-spawn từ các bomb gần nhau
        yield return new WaitForSeconds(0.15f);
        meatSpawnLock = false;
    }

    public IEnumerator Phase3AfterStun(Boss boss)
    {
        // 🚫 khóa phase chuyển trong lúc stun/ăn thịt
        boss.allowPhaseTransition = false;

        // đợi choáng hết
        while (boss.IsStunned)
            yield return null;

        // ăn hết thịt
        yield return EatAllMeat(boss);

        // nghỉ 1 chút trước khi dash lại
        yield return new WaitForSeconds(boss.phase3RestAfterMeat);

        // ✅ mở khóa phase chuyển sau khi ăn xong
        boss.allowPhaseTransition = true;

        // 🔑 mở khóa nhận dame/bomb lại
        meatSpawnLock = false;

        // tiếp tục dash player
        routine = boss.StartCoroutine(Phase3DashLoop(boss));
    }


    private IEnumerator EatAllMeat(Boss boss)
    {
        // 🔥 Chờ boss hết stun trước khi ăn thịt
        while (boss.IsStunned)
            yield return null;

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

    private IEnumerator Phase3Setup(Boss boss)
    {
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null) yield break;

        Vector3 center = (Vector3)((map.bottomLeft + map.topRight) / 2f);
        center.z = boss.transform.position.z;

        // Boss đi về giữa map
        yield return MoveTo(boss, center, boss.moveSpeed);

        // Spawn bombs 1 lần
        SpawnAndArrangeBombs(boss, center);

        // Chờ bomb vào vị trí
        yield return WaitForBombsInPosition();

        // ✅ Sau đó chỉ còn dash loop
        routine = boss.StartCoroutine(Phase3DashLoop(boss));
    }
}
