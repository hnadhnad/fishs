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

        // Kiểm tra skill còn khả dụng thì hiện nút, không thì ẩn
        if (option1Button != null)
        {
            option1Button.onClick.RemoveAllListeners();
            option1Button.onClick.AddListener(() => InternalChoose(1));
            option1Button.gameObject.SetActive(SkillManager.Instance.IsSkillAvailable(1));
        }

        if (option2Button != null)
        {
            option2Button.onClick.RemoveAllListeners();
            option2Button.onClick.AddListener(() => InternalChoose(2));
            option2Button.gameObject.SetActive(SkillManager.Instance.IsSkillAvailable(2));
        }

        if (option3Button != null)
        {
            option3Button.onClick.RemoveAllListeners();
            option3Button.onClick.AddListener(() => InternalChoose(3));
            option3Button.gameObject.SetActive(SkillManager.Instance.IsSkillAvailable(3));
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
