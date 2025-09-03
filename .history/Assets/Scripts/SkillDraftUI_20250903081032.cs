using System;
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

    // Event: int = option index (1..3 = chọn skill, 0 = skip)
    public event Action<int> OnSkillChosen;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (panelRoot != null) panelRoot.SetActive(false);

        if (option1Button != null) option1Button.onClick.AddListener(() => InternalChoose(1));
        if (option2Button != null) option2Button.onClick.AddListener(() => InternalChoose(2));
        if (option3Button != null) option3Button.onClick.AddListener(() => InternalChoose(3));
        if (skipButton != null) skipButton.onClick.AddListener(() => InternalChoose(0));
    }

    public void Show(int numberOfOptions)
    {
        if (IsOpen) return;
        if (panelRoot == null) { Debug.LogWarning("SkillDraftUI: panelRoot not set"); return; }

        optionsToShow = Mathf.Clamp(numberOfOptions, 1, 3);

        panelRoot.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1f;
        }

        // kiểm tra skill nào còn dùng được thì mới bật
        if (option1Button != null) option1Button.gameObject.SetActive(
            optionsToShow >= 1 && SkillManager.Instance.IsSkillAvailable(1));

        if (option2Button != null) option2Button.gameObject.SetActive(
            optionsToShow >= 2 && SkillManager.Instance.IsSkillAvailable(2));

        if (option3Button != null) option3Button.gameObject.SetActive(
            optionsToShow >= 3 && SkillManager.Instance.IsSkillAvailable(3));

        if (skipButton != null) skipButton.gameObject.SetActive(true);

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

    private void InternalChoose(int optionIndex)
    {
        OnSkillChosen?.Invoke(optionIndex);
        Hide();
    }
}
