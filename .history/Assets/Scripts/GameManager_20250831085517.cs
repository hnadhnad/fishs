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
    // Mốc điểm cần đạt để lên cấp (level 0 -> level 1 = 30, level 1 -> level 2 = 50, ...)
    public int[] scoreThresholds = { 0, 30, 50, 150, 300 };

    [Header("Level Growth Settings")]
    // sizePerLevel[0] = size khởi đầu
    // sizePerLevel[1] = size khi đạt mốc 30
    // sizePerLevel[2] = size khi đạt mốc 50 ...
    public float[] sizePerLevel = { 1f,1f, 1.5f, 1.8f, 2f, 2.5f };

    private int currentScore = 0;
    private int currentLevel = 0; // Level hiện tại (0 = bắt đầu)

    [Header("References")]
    public Fish playerFish;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Đặt size khởi đầu theo sizePerLevel[0]
        if (playerFish != null && sizePerLevel.Length > 0)
        {
            playerFish.SetSize(sizePerLevel[0]);
        }

        // Reset UI
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
        // Tăng size theo level tiếp theo
        if (currentLevel + 1 < sizePerLevel.Length)
        {
            playerFish.SetSize(sizePerLevel[currentLevel + 1]);
            Debug.Log($"Player lên cấp {currentLevel + 1} => size = {playerFish.size}");
        }

        // Sau khi set size thì mới tăng level
        currentLevel++;

        // Reset progress bar về 0 cho mốc tiếp theo
        if (progressBar != null)
            progressBar.value = 0;
    }
}
