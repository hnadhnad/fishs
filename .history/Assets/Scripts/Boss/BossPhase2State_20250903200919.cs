using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2:
/// - Boss rượt player với tốc độ = moveSpeed * phase2ChaseMultiplier.
/// - Né các bomb đang tồn tại (không chạy vô radius).
/// - Mỗi loạt bắn 5 bomb, 2 quả cuối boss đứng yên (mệt).
/// - Nếu bomb trúng boss → stun, rơi thịt, boss đi ăn thịt rồi mới quay lại loop.
/// </summary>
public class BossPhase2State : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        if (routine != null) boss.StopCoroutine(routine);

        // 🔥 Hồi đầy hunger và khóa không giảm
        boss.currentHunger = boss.maxHunger;
        boss.hungerDecayRate = 0f;

        // 🔥 Tắt UI hunger
        if (boss.hungerBar != null)
            boss.hungerBar.gameObject.SetActive(false);

        routine = boss.StartCoroutine(PhaseRoutine(boss));
    }


    public void Update(Boss boss) { }

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
        var centerPos = Vector3.zero; // toạ độ giữa map (bạn có thể chỉnh thủ công trong Inspector hoặc MapManager)

        while (boss != null && boss.currentHealth > 0f)
        {
            if (playerT == null)
            {
                var go = GameObject.FindWithTag("Player");
                if (go != null) playerT = go.transform;
            }

            // 🔥 Boss giữ vị trí giữa map (di chuyển về nếu bị lệch sau khi đi ăn thịt)
            while (Vector3.Distance(boss.transform.position, centerPos) > 0.1f && !boss.IsStunned)
            {
                boss.transform.position = Vector3.MoveTowards(
                    boss.transform.position,
                    centerPos,
                    boss.moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            // 🔁 Pattern loop
            yield return BombThenShoot(boss, playerT, 0); // thả bomb
            yield return BombThenShoot(boss, playerT, 3); // bắn 3 viên
            yield return BombThenShoot(boss, playerT, 0); // thả bomb
            yield return BombThenShoot(boss, playerT, 2); // bắn 2 viên
            yield return BombThenShoot(boss, playerT, 0); // thả bomb
            yield return BombThenShoot(boss, playerT, 1); // bắn 1 viên
            yield return BombThenShoot(boss, playerT, 0); // thả bomb
            yield return BombThenShoot(boss, playerT, 0); // thả bomb

            // ✅ Sau pattern → boss đi ăn thịt (nếu có)
            yield return EatAllMeat(boss);
        }
    }
    private IEnumerator BombThenShoot(Boss boss, Transform playerT, int shootCount)
    {
        // Spawn bomb vào vị trí player hiện tại
        Vector3 targetPos = (playerT != null) ? playerT.position : boss.transform.position;
        SpawnBomb(boss, targetPos);

        yield return new WaitForSeconds(boss.phase2BombInterval);

        // Nếu có bắn → dùng skill bắn của phase 1
        if (shootCount > 0 && playerT != null && boss.phase1BulletPrefab != null)
        {
            for (int i = 0; i < shootCount; i++)
            {
                Vector3 dir = (playerT.position - boss.transform.position).normalized;
                Vector3 spawnPos = boss.transform.position + dir * boss.phase1BulletSpawnOffset;

                GameObject bullet = Object.Instantiate(boss.phase1BulletPrefab, spawnPos, Quaternion.identity);
                if (bullet.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.velocity = dir * boss.phase1BulletSpeed;

                yield return new WaitForSeconds(boss.phase1ShootInterval);
            }
        }
    }



    private void SpawnBomb(Boss boss, Vector3 targetPosition)
    {
        if (boss.phase2BombPrefab == null) return;

        GameObject bombGo = Object.Instantiate(boss.phase2BombPrefab, targetPosition, Quaternion.identity);

        if (!bombGo.TryGetComponent<FallingBomb>(out var bomb))
            bomb = bombGo.AddComponent<FallingBomb>();

        bomb.Configure(
            boss.phase2BombDelay,        // float fallDuration
            boss.phase2BombRadius,       // float explodeRadius
            boss.phase2BombDamage,       // float damage
            boss.meatPrefab,             // GameObject meatPrefab
            boss.phase2MeatCount,        // int meatCount
            boss.phase2MeatSpawnOffset,  // float meatSpawnOffset
            boss.phase2MeatScatterSpeed, // float meatScatterSpeed
            boss.phase2BossStunDuration  // float bossStunDuration
        );
    }

    /// <summary>
    /// Boss chạy về player nhưng tránh vùng bomb.
    /// </summary>
    private void MoveTowardsPlayerAvoidingBombs(Boss boss, Vector3 playerPos)
    {
        Vector3 moveDir = (playerPos - boss.transform.position).normalized;

        // Check từng bomb đang tồn tại
        var bombs = Object.FindObjectsOfType<FallingBomb>();
        foreach (var bomb in bombs)
        {
            float dist = Vector3.Distance(boss.transform.position, bomb.transform.position);
            if (dist < bomb.explodeRadius + 1f) // 1f = margin tránh
            {
                // Né sang hướng vuông góc
                Vector3 away = (boss.transform.position - bomb.transform.position).normalized;
                moveDir += away * 1.5f; // cộng vector né
            }
        }

        moveDir.Normalize();
        boss.transform.position += moveDir * boss.moveSpeed * boss.phase2ChaseMultiplier * Time.deltaTime;
    }

    /// <summary>
    /// Boss ăn hết thịt trên map trước khi quay lại loop.
    /// </summary>
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

}
