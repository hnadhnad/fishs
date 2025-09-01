using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]

    [Header("Progress Settings")]
    // Mốc điểm cần đạt để lên cấp
    public int[] scoreThresholds = { 30, 50, 150, 300 };

    [Header("Level Growth Settings")]
    // sizePerLevel[0] = size khi đạt mốc 30
    // sizePerLevel[1] = size khi đạt mốc 50 ...
    public float[] sizePerLevel = { 1.5f, 1.8f, 2f, 2.5f };

    private int currentScore = 0;
    private int currentLevel = 0; // Level hiện tại (0 = chưa đạt mốc nào)

    [Header("References")]
    public Fish playerFish;

    public Slider progressBar;
    public TMP_Text scoreText;
    public RectTransform milestoneContainer;
    public GameObject milestonePrefab;

    private List<GameObject> milestoneMarkers = new List<GameObject>();

    [Header("UI Skilldrafts")]

    public int maxSkillDrafts = 3;       // số lần cho phép draft skill
    private int skillDraftsUsed = 0;     // đã dùng bao nhiêu lần
    private int choicesRemaining = 3;    // số lượng lựa chọn cho lần kế tiếp (ban đầu là 3)

    [Header("Boss Settings")]
    public GameObject bossPrefab;     // Kéo prefab Boss vào đây
    public Vector2 bossSpawnPos = new Vector2(5, 5); // ✅ Đúng



    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        SpawnBoss();

        // PlayerFish khởi đầu giữ nguyên size mặc định (set trong prefab / inspector)
        if (scoreText != null) scoreText.text = $"Score: {currentScore}";
        if (progressBar != null) progressBar.value = 0;

        if (SkillDraftUI.Instance != null)
        {
            SkillDraftUI.Instance.OnSkillChosen += HandleSkillChosenFromUI;
        }

        SetupMilestones();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";

        // Xử lý lên cấp trước
        while (currentLevel < scoreThresholds.Length &&
            currentScore >= scoreThresholds[currentLevel])
        {
            LevelUp();
        }

        // Cập nhật thanh tiến trình
        if (progressBar != null)
        {
            float stepSize = 1f / scoreThresholds.Length;

            if (currentLevel >= scoreThresholds.Length)
            {
                // Đã đạt mốc cuối cùng
                progressBar.value = 1f;
            }
            else
            {
                float prevTarget = (currentLevel == 0) ? 0 : scoreThresholds[currentLevel - 1];
                float nextTarget = scoreThresholds[currentLevel];

                // % trong mốc hiện tại (0 → 1)
                float localProgress = Mathf.InverseLerp(prevTarget, nextTarget, currentScore);

                // Tổng tiến trình = số mốc đã qua + phần trăm trong mốc hiện tại
                progressBar.value = (currentLevel * stepSize) + (localProgress * stepSize);
            }
        }
    }




    void LevelUp()
    {
        if (currentLevel < sizePerLevel.Length)
        {
            playerFish.SetSize(sizePerLevel[currentLevel]);
            Debug.Log($"Player đạt mốc {scoreThresholds[currentLevel]} => size = {playerFish.size}");
        }

        // Highlight milestone đã đạt
        if (currentLevel < milestoneMarkers.Count)
        {
            milestoneMarkers[currentLevel].GetComponent<Image>().color = Color.green;
        }

        // 🔹 Gọi UI skill draft tại đúng mốc
        OnReachedThreshold(currentLevel);

        currentLevel++;
    }


    void SetupMilestones()
    {
        if (milestoneContainer == null || milestonePrefab == null) return;

        int total = scoreThresholds.Length;

        for (int i = 0; i < total; i++)
        {
            GameObject marker = Instantiate(milestonePrefab, milestoneContainer);
            marker.name = $"Milestone_{scoreThresholds[i]}";

            // Vị trí milestone theo chia đều (i+1)/total
            float normalized = (i + 1f) / total;

            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(normalized, 0f);
            rt.anchorMax = new Vector2(normalized, 1f);
            rt.anchoredPosition = Vector2.zero;

            milestoneMarkers.Add(marker);
        }
    }

    // Gọi hàm này khi vượt qua 1 mốc ăn (thresholdIndex là index mốc trong scoreThresholds)
    private void OnReachedThreshold(int thresholdIndex)
    {
        if (skillDraftsUsed >= maxSkillDrafts) return;
        if (thresholdIndex >= maxSkillDrafts) return;

        if (SkillDraftUI.Instance != null)
        {
            SkillDraftUI.Instance.Show(choicesRemaining);
        }
    }

    private void HandleSkillChosenFromUI(int optionIndex)
    {
        skillDraftsUsed++;

        if (optionIndex == 0)
        {
            // Skip → giữ nguyên số lựa chọn
            Debug.Log("Player skipped skill choice");
        }
        else
        {
            // Chọn skill → giảm số lựa chọn cho lần sau (tối thiểu còn 1)
            Debug.Log($"Player picked skill option: {optionIndex}");
            choicesRemaining = Mathf.Max(1, choicesRemaining - 1);
        }
    }
    public void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("⚠️ Chưa gán Boss Prefab trong Inspector!");
            return;
        }

        // Nếu chưa set vị trí thì mặc định ở giữa map
        Vector2 spawnPos = bossSpawnPos == Vector2.zero
            ? new Vector2((bottomLeft.x + topRight.x) / 2f, (bottomLeft.y + topRight.y) / 2f)
            : bossSpawnPos;

        Instantiate(bossPrefab, spawnPos, Quaternion.identity);
        Debug.Log("Boss đã được triệu hồi tại: " + spawnPos);
    }


}
