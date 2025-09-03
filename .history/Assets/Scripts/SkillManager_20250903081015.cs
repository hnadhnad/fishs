using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    private FishMovement fishMovement;
    private bool[] skillUsed = new bool[4]; // skill 1..3 (index = skillId), skip = 0

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        fishMovement = FindObjectOfType<FishMovement>();

        // đăng ký lắng nghe event chọn skill từ UI
        if (SkillDraftUI.Instance != null)
        {
            SkillDraftUI.Instance.OnSkillChosen += UseSkill;
        }
    }

    public void UseSkill(int skillId)
    {
        if (skillId == 0) // skip
        {
            Debug.Log("Skip skill.");
            return;
        }

        if (skillUsed[skillId])
        {
            Debug.Log($"Skill {skillId} đã được chọn trước đó.");
            return;
        }

        switch (skillId)
        {
            case 1:
                if (fishMovement != null)
                    fishMovement.maxSpeed *= 1.5f;
                Debug.Log("Skill 1: Tăng tốc vĩnh viễn!");
                break;

            case 2:
                if (fishMovement != null)
                    fishMovement.enableDash = true;
                Debug.Log("Skill 2: Bật Dash!");
                break;

            case 3:
                if (PlayerHunger.Instance != null)
                    PlayerHunger.Instance.hungerDecayRate *= 0.5f;
                Debug.Log("Skill 3: Giảm tốc độ đói!");
                break;
        }

        // đánh dấu skill đã chọn
        skillUsed[skillId] = true;
    }

    // Hàm phụ để UI biết skill nào còn khả dụng
    public bool IsSkillAvailable(int skillId)
    {
        return !skillUsed[skillId];
    }
}
