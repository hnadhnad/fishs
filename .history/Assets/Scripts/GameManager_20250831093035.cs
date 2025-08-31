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

    [Header("Progress Settings")]

    [Header("UI Elements")]
    public Slider progressBar;
    public TMP_Text scoreText;
    public RectTransform milestoneContainer;
    public GameObject milestonePrefab;

    private List<GameObject> milestoneMarkers = new List<GameObject>();



    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // PlayerFish khởi đầu giữ nguyên size mặc định (set trong prefab / inspector)
        if (scoreText != null) scoreText.text = $"Score: {currentScore}";
        if (progressBar != null) progressBar.value = 0;

        SetupMilestones();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";

        if (progressBar != null && currentLevel < scoreThresholds.Length)
        {
            int nextTarget = scoreThresholds[currentLevel];
            float prevTarget = (currentLevel == 0) ? 0 : scoreThresholds[currentLevel - 1];

            // % tiến độ trong mốc hiện tại (0 → 1)
            float localProgress = Mathf.InverseLerp(prevTarget, nextTarget, currentScore);

            // Mỗi mốc chiếm 1/ tổng số mốc
            float stepSize = 1f / scoreThresholds.Length;

            // Thanh = số mốc đã qua + tiến độ trong mốc hiện tại
            progressBar.value = currentLevel * stepSize + localProgress * stepSize;
        }

        // Kiểm tra nếu vượt qua mốc thì LevelUp
        while (currentLevel < scoreThresholds.Length &&
            currentScore >= scoreThresholds[currentLevel])
        {
            LevelUp();
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

        currentLevel++;
        if (progressBar != null) progressBar.value = 0;
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


}
