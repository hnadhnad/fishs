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
    public int[] scoreThresholds = { 30, 50, 150, 300 }; // các mốc điểm

    [Header("Level Growth Settings")]
    public float[] sizePerLevel = { 1f,1.5f, 1.8f, 2f, 2.5f };

    private int currentScore = 0;
    private int currentLevel = 0;

    [Header("References")]
    public Fish playerFish; 


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddScore(int amount)
    {
        currentScore += amount;

        // Cập nhật UI
        if (scoreText != null) scoreText.text = $"Score: {currentScore}";
        if (progressBar != null && currentLevel < scoreThresholds.Length)
        {
            int nextTarget = scoreThresholds[currentLevel];
            float prevTarget = (currentLevel == 0) ? 0 : scoreThresholds[currentLevel - 1];

            // progress = tỉ lệ trong khoảng [prevTarget → nextTarget]
            progressBar.value = Mathf.InverseLerp(prevTarget, nextTarget, currentScore);
        }

        // Check mốc (có thể ăn 1 con cá to khiến vượt nhiều mốc 1 lúc)
        while (currentLevel < scoreThresholds.Length && currentScore >= scoreThresholds[currentLevel])
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;

        // an toàn: tránh out of range
        if (currentLevel < sizePerLevel.Length)
        {
            playerFish.SetSize(sizePerLevel[currentLevel]);
            Debug.Log($"Player lên cấp {currentLevel} => size = {playerFish.size}");
        }
    }



}
