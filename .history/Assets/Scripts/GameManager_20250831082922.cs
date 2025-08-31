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
    public int[] scoreThresholds = { 50, 150, 300 }; // các mốc điểm
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

        // cập nhật UI
        if (scoreText != null) scoreText.text = $"Score: {currentScore}";
        if (progressBar != null && currentLevel < scoreThresholds.Length)
        {
            int nextTarget = scoreThresholds[currentLevel];
            progressBar.value = Mathf.Clamp01((float)currentScore / nextTarget);
        }

        // kiểm tra đạt mốc level-up
        if (currentLevel < scoreThresholds.Length && currentScore >= scoreThresholds[currentLevel])
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        currentLevel++;
        if (playerFish != null)
        {
            // tăng size lên theo cấp
            float newSize = playerFish.size + 0.5f; // hoặc array size riêng
            playerFish.SetSize(newSize);
        }
        Debug.Log("Player lên cấp " + currentLevel);
    }

}
