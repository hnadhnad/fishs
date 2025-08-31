using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public Slider progressBar;   // thanh tiến trình
    public TMP_Text scoreText;

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
    public int[] scoreThresholds = { 30, 50, 150, 300 };   // Mốc điểm
    public float[] sizePerLevel = { 1.5f, 1.8f, 2f, 2.5f }; // Size tương ứng

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
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        // Cập nhật text
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore}";

        // Cập nhật thanh tiến trình cho mốc hiện tại
        if (progressBar != null && currentLevel < scoreThresholds.Length)
        {
            int nextTarget = scoreThresholds[currentLevel];
            float prevTarget = (currentLevel == 0) ? 0 : scoreThresholds[currentLevel - 1];
            progressBar.value = Mathf.InverseLerp(prevTarget, nextTarget, currentScore);
        }

        // Kiểm tra lên cấp (có thể vượt nhiều mốc trong 1 lần ăn)
        while (currentLevel < scoreThresholds.Length &&
               currentScore >= scoreThresholds[currentLevel])
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        // Set size theo mốc hiện tại
        if (currentLevel < sizePerLevel.Length)
        {
            playerFish.SetSize(sizePerLevel[currentLevel]);
            Debug.Log($"Player đạt mốc {scoreThresholds[currentLevel]} => size = {playerFish.size}");
        }

        currentLevel++;

        // Reset progress bar về 0 cho mốc tiếp theo
        if (progressBar != null)
            progressBar.value = 0;
    }
    void SetupMilestones()
    {
        if (milestoneContainer == null || milestonePrefab == null) return;

        float maxScore = scoreThresholds[scoreThresholds.Length - 1];

        foreach (int threshold in scoreThresholds)
        {
            GameObject marker = Instantiate(milestonePrefab, milestoneContainer);
            marker.name = $"Milestone_{threshold}";

            // Tỷ lệ điểm trên thanh [0..1]
            float normalized = threshold / maxScore;

            RectTransform rt = marker.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(normalized, 0f);
            rt.anchorMax = new Vector2(normalized, 1f);
            rt.anchoredPosition = Vector2.zero;

            milestoneMarkers.Add(marker);
        }
    }

}
