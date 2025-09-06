using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    private FishMovement fishMovement;
    private bool[] skillUsed = new bool[4]; // 1..3

    [Header("Debug / Testing")]
    [Tooltip("Nếu tick thì luôn mở skill khiên ngay từ đầu mà không cần farm")]
    public bool alwaysUnlockShield = false;

    // Shield state (skill 1)
    private bool shieldAvailable = false;
    private bool shieldConsumed = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        fishMovement = FindObjectOfType<FishMovement>();
        if (SkillDraftUI.Instance != null)
            SkillDraftUI.Instance.OnSkillChosen += UseSkill;

        // ✅ Nếu bật auto unlock shield trong editor
        if (alwaysUnlockShield)
        {
            shieldAvailable = true;
            shieldConsumed = false;
            skillUsed[1] = true; // đánh dấu skill 1 đã dùng để tránh draft lại
            Debug.Log("[SkillManager] Shield auto unlocked for testing!");
        }
    }

    private void OnDestroy()
    {
        if (SkillDraftUI.Instance != null)
            SkillDraftUI.Instance.OnSkillChosen -= UseSkill;
    }

    public bool IsSkillAvailable(int skillId)
    {
        if (skillId < 1 || skillId > 3) return false;
        return !skillUsed[skillId];
    }

    public void UseSkill(int skillId)
    {
        if (skillId == 0) { Debug.Log("Skip skill."); return; }
        if (skillId < 1 || skillId > 3) return;
        if (skillUsed[skillId]) { Debug.Log($"Skill {skillId} đã dùng."); return; }

        switch (skillId)
        {
            case 1:
                // Shield: one-time extra life (consumable)
                shieldAvailable = true;
                shieldConsumed = false;
                Debug.Log("Skill 1: Shield (one-time extra life).");
                break;

            case 2:
                if (fishMovement != null) fishMovement.enableDash = true;
                Debug.Log("Skill 2: Bật Dash!");
                break;

            case 3:
                if (PlayerHunger.Instance != null) PlayerHunger.Instance.hungerDecayRate *= 0.5f;
                Debug.Log("Skill 3: Giảm tốc độ đói!");
                break;
        }

        skillUsed[skillId] = true;
    }

    // API for BossEnragedState
    public bool HasShield()
    {
        return shieldAvailable && !shieldConsumed;
    }

    public void ConsumeShield()
    {
        if (shieldAvailable && !shieldConsumed)
        {
            shieldConsumed = true;
            Debug.Log("Shield consumed (player saved from boss belly).");
        }
    }
}
