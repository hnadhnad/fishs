using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;
    private FishMovement fishMovement; // tham chiếu tới script FishMovement của player


    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        // tìm player trong scene
        fishMovement = FindObjectOfType<FishMovement>();
    }

    // Hàm gọi khi bấm skill
    public void UseSkill(int skillId)
    {
        switch (skillId)
        {
            case 0:
                Debug.Log("Skip");
                // // ví dụ: tăng tốc cho cá
                // FindObjectOfType<FishMovement>().maxSpeed *= 1.5f;
                break;
            case 1:
                if (fishMovement != null)
                {
                    fishMovement.maxSpeed *= 1.5f; // tăng 50% vĩnh viễn
                    Debug.Log("Skill 1: Tăng tốc vĩnh viễn!");
                }
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
