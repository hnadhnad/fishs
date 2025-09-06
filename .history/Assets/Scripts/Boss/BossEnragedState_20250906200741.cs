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

    public void Enter(Boss boss)
    {
        Debug.Log("[BossEnragedState] Enter");
        if (routine != null) boss.StopCoroutine(routine);
        routine = boss.StartCoroutine(EnragedRoutine(boss));
    }

    public void Update(Boss boss) { }

    public void Exit(Boss boss)
    {
        Debug.Log("[BossEnragedState] Exit");
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

        // 2) Windup (vận sức) — bạn có thể chơi animation ở đây
        float windup = Mathf.Max(0f, boss.enragedWindupDuration);
        float t0 = 0f;
        while (t0 < windup)
        {
            t0 += Time.deltaTime;
            yield return null;
        }

        // block phase transition while doing final attack (optional, safe)
        boss.allowPhaseTransition = false;

        // 3) Find player and lock movement
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

        // 4) Suck player toward boss over duration
        float suckDuration = Mathf.Max(0.01f, boss.enragedSuckDuration);
        float elapsed = 0f;
        Vector3 startPos = playerGO.transform.position;
        Vector3 bossPos = boss.transform.position;

        while (elapsed < suckDuration)
        {
            elapsed += Time.deltaTime;
            float tt = Mathf.Clamp01(elapsed / suckDuration);
            // you can use Lerp or MoveTowards; use Lerp to guarantee reach at end
            playerGO.transform.position = Vector3.Lerp(startPos, bossPos, tt);
            yield return null;
        }

        // 5) Decide fate
        bool shieldSaved = false;
        if (SkillManager.Instance != null && SkillManager.Instance.HasShield())
        {
            shieldSaved = true;
            SkillManager.Instance.ConsumeShield();
        }

        if (shieldSaved)
        {
            Debug.Log("[BossEnragedState] Player saved by shield. Changing background to inside-boss.");

            if (boss.enragedBackground != null)
            {
                map.ChangeBackground(boss.enragedBackground, boss.enragedMapScale);
            }

            float midY = (map.bottomLeft.y + map.topRight.y) / 2f;
            Vector3 newPos = new Vector3(map.bottomLeft.x + 0.5f, midY, playerGO.transform.position.z);
            playerGO.transform.position = newPos;
        }
        else
        {
            Debug.Log("[BossEnragedState] Player has no shield -> die.");
            if (fish != null)
            {
                fish.Die();
            }
        }

        // restore player control if still exists
        if (fm != null)
            fm.UnlockMovement();

        // allow phase transitions again (safe)
        boss.allowPhaseTransition = true;

        // end routine — if you want boss to continue something you can start another coroutine
        routine = null;
    }

    private IEnumerator MoveTo(Boss boss, Vector3 target, float speed)
    {
        while (boss != null && Vector3.Distance(boss.transform.position, target) > 0.05f)
        {
            boss.transform.position = Vector3.MoveTowards(boss.transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
    }
}
