using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
   [SerializeField]  public int skillId;   // gán 0,1,2 cho từng nút trong Inspector
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => OnClickSkill());
    }

    void OnClickSkill()
    {
        SkillManager.Instance.UseSkill(skillId);
    }
}
