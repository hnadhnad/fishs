using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Hàm gọi khi bấm skill
    public void UseSkill(int skillId)
    {
        switch (skillId)
        {
            case 1:
                Debug.Log("Skill 1: Tăng tốc bơi");
                // // ví dụ: tăng tốc cho cá
                // FindObjectOfType<FishMovement>().maxSpeed *= 1.5f;
                break;

            case 2:
                Debug.Log("Skill 2: Hồi máu");
                // // ví dụ: tăng máu cho cá
                // FindObjectOfType<Fish>().GainHealth(20);
                break;

            case 3:
                Debug.Log("Skill 3: Tàng hình");
                // // ví dụ: làm cá tàng hình trong 5s
                // FindObjectOfType<SpriteRenderer>().enabled = false;
                // Invoke(nameof(ResetInvisible), 5f);
                break;

            default:
                Debug.Log("Skill chưa định nghĩa");
                break;
        }
    }

    void ResetInvisible()
    {
        FindObjectOfType<SpriteRenderer>().enabled = true;
    }
}
