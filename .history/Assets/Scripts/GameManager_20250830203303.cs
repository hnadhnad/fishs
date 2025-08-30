using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elements")]
    public Slider progressBar;   // thanh tiến trình
    public Text scoreText;       // hiển thị điểm số

    [Header("Progress Settings")]
    public int targetScore = 100; // điểm cần để đầy thanh (lên cấp, hoặc spawn boss)
    private int currentScore = 0;

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
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01((float)currentScore / targetScore);

        // kiểm tra đầy thanh
        if (currentScore >= targetScore)
        {
            Debug.Log("Đầy thanh rồi! Có thể trigger boss hoặc tăng cấp.");
            // TODO: gọi hàm spawn boss hoặc level up
        }
    }
}
