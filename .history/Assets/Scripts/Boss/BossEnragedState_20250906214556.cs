using System.Collections;
using UnityEngine;

/// <summary>
/// Final enraged phase:
/// - Boss đi về giữa map, đứng vận sức (windup).
/// - Boss hút Player từ từ vào (player bị LockMovement).
/// - Nếu player có shield (SkillManager.HasShield()) -> consume shield, đổi background bằng boss.enragedBackground và cho player sống.
/// - Nếu không -> player chết (Fish.Die()).
/// - Sau cùng unlock movement và (tuỳ bạn) cho phép chuyển phase lại.
/// </summary>
public class BossEnragedState : IBossState
{
    private Coroutine routine;
    private Coroutine insideLoopRoutine;


    public void Enter(Boss boss)
    {
        Debug.Log("[BossEnragedState] Enter");
        boss.inEnragedPhase = true;
        if (routine != null) boss.StopCoroutine(routine);
        routine = boss.StartCoroutine(EnragedRoutine(boss));
        // 🔥 Hồi đầy hunger và khóa không giảm
        boss.currentHunger = boss.maxHunger;
        boss.hungerDecayRate = 0f;

        // 🔥 Tắt UI hunger
        if (boss.hungerBar != null) boss.hungerBar.gameObject.SetActive(false);
    }

    public void Update(Boss boss) { }

    public void Exit(Boss boss)
    {
        Debug.Log("[BossEnragedState] Exit");
        boss.inEnragedPhase = false;
        if (routine != null)
        {
            boss.StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator EnragedRoutine(Boss boss)
    {
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null)
        {
            Debug.LogWarning("[BossEnragedState] No MapManager found, abort.");
            yield break;
        }

        // 1) Move boss to center
        Vector3 center = (map.bottomLeft + map.topRight) / 2f;
        center.z = boss.transform.position.z;
        yield return MoveTo(boss, center, boss.moveSpeed);

        // 2) Windup (vận sức)
        float windup = Mathf.Max(0f, boss.enragedWindupDuration);
        float t0 = 0f;
        while (t0 < windup)
        {
            t0 += Time.deltaTime;
            yield return null;
        }

        // block phase transition
        boss.allowPhaseTransition = false;

        // 3) Find player
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogWarning("[BossEnragedState] No player found.");
            boss.allowPhaseTransition = true;
            yield break;
        }

        var fm = playerGO.GetComponent<FishMovement>();
        var fish = playerGO.GetComponent<Fish>();

        if (fm != null) fm.LockMovement();

        // 4) Suck player
        float suckDuration = Mathf.Max(0.01f, boss.enragedSuckDuration);
        float elapsed = 0f;
        Vector3 startPos = playerGO.transform.position;
        Vector3 bossPos = boss.transform.position;

        while (elapsed < suckDuration)
        {
            if (playerGO == null) yield break; // player chết trong lúc bị hút

            elapsed += Time.deltaTime;
            float tt = Mathf.Clamp01(elapsed / suckDuration);
            playerGO.transform.position = Vector3.Lerp(startPos, bossPos, tt);
            yield return null;
        }

        // 5) Decide fate (nếu player vẫn còn)
        if (playerGO != null && fish != null)
        {
            bool shieldSaved = false;
            if (SkillManager.Instance != null && SkillManager.Instance.HasShield())
            {
                shieldSaved = true;
                SkillManager.Instance.ConsumeShield();
            }

            // Nếu fish đã được cứu trước đó (Fish.Die() có shield)
            if (fish.wasSavedByShield)
            {
                shieldSaved = true;
                fish.wasSavedByShield = false; // reset flag
                Debug.Log("[BossEnragedState] fish.wasSavedByShield -> treat as saved");
            }
            if (shieldSaved)
            {
                Debug.Log("[BossEnragedState] Player saved by shield. Changing background to inside-boss.");

                if (boss.enragedBackground != null)
                {
                    map.ChangeBackground(boss.enragedBackground, boss.enragedMapScale);
                }

                // dịch chuyển player tới mép trái map
                Vector3 spawnPos = new Vector3(
                    map.bottomLeft.x + 1f,
                    (map.bottomLeft.y + map.topRight.y) / 2f,
                    playerGO.transform.position.z
                );
                playerGO.transform.position = spawnPos;

                // 🔥 Bắt đầu loop spawn cột dọc
                if (insideLoopRoutine == null)
                    insideLoopRoutine = boss.StartCoroutine(StartInsideLoop(boss));

                if (fm != null) fm.UnlockMovement();
                boss.allowPhaseTransition = true;
                routine = null;
                yield break;
            }
            else
            {
                Debug.Log("[BossEnragedState] Player has no shield -> die.");
                fish.Die();

                boss.allowPhaseTransition = true;
                routine = null;
                yield break;
            }
        }
    }

    private IEnumerator MoveTo(Boss boss, Vector3 target, float speed)
    {
        while (boss != null && Vector3.Distance(boss.transform.position, target) > 0.05f)
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
    /// Gọi cái này sau khi player teleport vào bụng boss
    private IEnumerator StartInsideLoop(Boss boss)
    {
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null) yield break;

        while (boss != null && boss.currentHealth > 0f)
        {
            SpawnColumn(boss, map);
            yield return new WaitForSeconds(boss.insideColumnSpawnInterval);
        }
    }

    private void SpawnColumn(Boss boss, MapManager map)
    {
        float spawnX = map.topRight.x + boss.insideColumnMargin;
        float minY = map.bottomLeft.y;
        float maxY = map.topRight.y;

        GameObject column = new GameObject("InsideColumn");
        column.transform.position = new Vector3(spawnX, (minY + maxY) / 2f, 0f);

        // Tính spacing tự động theo chiều cao map
        float spacing = (maxY - minY) / (boss.insideColumnSlots + 1);

        for (int i = 0; i < boss.insideColumnSlots; i++)
        {
            float y = minY + spacing * (i + 1); // dàn đều giữa minY và maxY
            Vector3 pos = new Vector3(spawnX, y, 0f);

            float r = Random.value;
            GameObject prefab = null;
            float scale = 1f;

            if (r < 0.65f && boss.insideEdiblePrefab != null)
            {
                prefab = boss.insideEdiblePrefab;
                scale = boss.insideEdibleScale;
            }
            else if (boss.insideHazardPrefab != null)
            {
                prefab = boss.insideHazardPrefab;
                scale = boss.insideHazardScale;
            }

            if (prefab != null)
            {
                var go = Object.Instantiate(prefab, pos, Quaternion.identity, column.transform);
                go.transform.localScale = Vector3.one * scale;
            }
        }

        boss.StartCoroutine(MoveColumn(column, boss, map));

        // ⭐ Spawn heart riêng, không theo cột
        if (boss.insideHeartPrefab != null)
        {
            Vector3 heartPos = new Vector3(
                map.topRight.x - boss.insideHeartOffsetFromRight,
                (minY + maxY) / 2f,
                0f
            );
            var heart = Object.Instantiate(boss.insideHeartPrefab, heartPos, Quaternion.identity);
            heart.transform.localScale = Vector3.one * boss.insideHeartScale;
        }
    }




    private IEnumerator MoveColumn(GameObject column, Boss boss, MapManager map)
    {
        float leftLimit = map.bottomLeft.x - 2f;

        while (column != null && boss != null && boss.currentHealth > 0f)
        {
            column.transform.position += Vector3.left * boss.insideColumnSpeed * Time.deltaTime;

            if (column.transform.position.x < leftLimit)
            {
                Object.Destroy(column);
                yield break;
            }

            yield return null;
        }

        if (column != null) Object.Destroy(column);
    }

}
