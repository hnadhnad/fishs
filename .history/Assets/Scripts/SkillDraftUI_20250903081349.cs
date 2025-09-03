using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillDraftUI : MonoBehaviour
{
    public static SkillDraftUI Instance { get; private set; }

    [Header("Root UI")]
    public GameObject panelRoot;
    public CanvasGroup canvasGroup;

    [Header("Buttons")]
    public Button option1Button;
    public Button option2Button;
    public Button option3Button;
    public Button skipButton;

    private int optionsToShow = 3; // số lượng lựa chọn sẽ hiển thị (1–3)
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    // Event: int = skillId (1..3 = chọn skill, 0 = skip)
    public event Action<int> OnSkillChosen;

    // lưu mapping runtime: nút i đang đại diện cho skillId nào
    private readonly List<Button> optionButtons = new();
    private readonly List<int> runtimeOptionIds = new(); // song song với optionButtons

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (panelRoot != null) panelRoot.SetActive(false);

        // Gom các button option thành list để dễ xử lý
        optionButtons.Clear();
        if (option1Button) optionButtons.Add(option1Button);
        if (option2Button) optionButtons.Add(option2Button);
        if (option3Button) optionButtons.Add(option3Button);

        // Chỉ gắn listener cố định cho Skip
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(() => InternalChoose(0));
        }
    }

    public void Show(int numberOfOptions)
    {
        if (IsOpen) return;
        if (panelRoot == null) { Debug.LogWarning("SkillDraftUI: panelRoot not set"); return; }

        optionsToShow = Mathf.Clamp(numberOfOptions, 1, 3);

        // Bật UI
        panelRoot.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        // 1) Lấy danh sách skill còn khả dụng
        List<int> availableIds = new List<int>();
        if (SkillManager.Instance == null)
        {
            // fallback: cho hiện hết 1..3
            availableIds.Add(1);
            availableIds.Add(2);
            availableIds.Add(3);
        }
        else
        {
            for (int id = 1; id <= 3; id++)
            {
                if (SkillManager.Instance.IsSkillAvailable(id))
                    availableIds.Add(id);
            }
        }

        // 2) Chọn tối đa optionsToShow skill để hiển thị
        int showCount = Mathf.Min(optionsToShow, availableIds.Count);

        // 3) Reset và map nút theo danh sách trên
        runtimeOptionIds.Clear();

        for (int i = 0; i < optionButtons.Count; i++)
        {
            var btn = optionButtons[i];
            if (btn == null) continue;

            btn.onClick.RemoveAllListeners();

            if (i < showCount)
            {
                int skillId = availableIds[i]; // map skill thực vào nút i
                runtimeOptionIds.Add(skillId);

                btn.gameObject.SetActive(true);
                btn.onClick.AddListener(() => InternalChoose(skillId));

                // (tuỳ chọn) cập nhật label nếu bạn có Text/TMP trên nút
                // var txt = btn.GetComponentInChildren<TMPro.TMP_Text>();
                // if (txt) txt.text = GetSkillName(skillId);
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }

        if (skipButton != null) skipButton.gameObject.SetActive(true);

        // Dừng game
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (!IsOpen) return;

        if (panelRoot != null) panelRoot.SetActive(false);
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0f;
        }

        // Resume game
        Time.timeScale = 1f;
    }

    private void InternalChoose(int skillId)
    {
        OnSkillChosen?.Invoke(skillId);
        Hide();
    }

    // (tuỳ chọn) tên hiển thị
    // private string GetSkillName(int id) =>
    //     id switch { 1 => "Tăng tốc", 2 => "Dash", 3 => "Giảm đói", _ => "?" };
}
