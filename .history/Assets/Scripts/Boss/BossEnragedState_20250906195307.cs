using System.Collections;
using UnityEngine;

public class BossEnragedState : IBossState
{
    private Coroutine routine;

    public void Enter(Boss boss)
    {
        Debug.Log("🔥 Boss vào Phase Enraged");

        if (routine != null) boss.StopCoroutine(routine);
        routine = boss.StartCoroutine(EnragedRoutine(boss));
    }

    public void Update(Boss boss) { }

    public void Exit(Boss boss)
    {
        Debug.Log("❌ Boss rời Phase Enraged");
        if (routine != null)
        {
            boss.StopCoroutine(routine);
            routine = null;
        }
    }

    private IEnumerator EnragedRoutine(Boss boss)
    {
        // 1) Boss ra giữa map
        var map = Object.FindObjectOfType<MapManager>();
        if (map == null) yield break;
        Vector3 center = (map.bottomLeft + map.topRight) / 2f;
        while (Vector3.Distance(boss.transform.position, center) > 0.1f)
        {
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position,
                center,
                boss.moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // 2) Boss vận sức (đứng yên để chuẩn bị skill)
        yield return boss.StartCoroutine(ChargeUp(boss));

        // 3) Hút player
        var playerGo = GameObject.FindWithTag("Player");
        if (playerGo != null)
        {
            var fishMove = playerGo.GetComponent<FishMovement>();
            var fish = playerGo.GetComponent<Fish>();

            if (fishMove != null) fishMove.LockMovement();

            float pullTime = 2.5f; // có thể public ra Boss để chỉnh
            float elapsed = 0f;
            Vector3 startPos = playerGo.transform.position;

            while (elapsed < pullTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / pullTime;
                playerGo.transform.position = Vector3.Lerp(startPos, boss.transform.position, t);
                yield return null;
            }

            // 4) Xử lý khi player đến Boss
            var skillMgr = playerGo.GetComponent<SkillManager>();
            bool hasShield = (skillMgr != null && skillMgr.HasShield);

            if (hasShield)
            {
                Debug.Log("🛡 Player dùng khiên → sống sót và vào bụng boss!");

                skillMgr.ConsumeShield();

                // Đổi background thành trong bụng Boss
                if (boss.enragedBackground != null && map != null)
                {
                    map.ChangeBackground(boss.enragedBackground);
                }

                if (fishMove != null) fishMove.UnlockMovement();
            }
            else
            {
                Debug.Log("💀 Player bị Boss nuốt chết!");
                if (fish != null) fish.Die();
            }
        }
    }

    /// <summary>
    /// Boss đứng yên vận sức (để tiện add animation sau này).
    /// </summary>
    private IEnumerator ChargeUp(Boss boss)
    {
        float chargeTime = 2f; // có thể public ra Boss để chỉnh trong Inspector
        float timer = 0f;
        while (timer < chargeTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
    }
}
