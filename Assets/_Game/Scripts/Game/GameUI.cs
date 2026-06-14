using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TextMeshProUGUI _gameOverTitleText;
    [SerializeField] private TextMeshProUGUI _gameOverDetailText;
    [SerializeField] private Button _replayButton;

    [Header("Game Over Text")]
    [SerializeField] private string _perfectScoreDetailText = "You scored {0}/{1}. Now you know exactly what Neil would do, but there's so much more that he can bring to the table! Schedule an interview to learn more!";
    [SerializeField] private string _belowPerfectScoreDetailText = "You scored {0}/{1}. Review the matchups and try again, or schedule an interview with Neil to learn more about his unique perspective.";

    void Awake()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        if (_replayButton != null)
            _replayButton.onClick.AddListener(OnReplayClicked);
    }

    void OnDestroy()
    {
        if (_replayButton != null)
            _replayButton.onClick.RemoveListener(OnReplayClicked);
    }

    public void SetStatus(string message)
    {
        if (_statusText != null)
            _statusText.text = message;
    }

    public void UpdateScore(int score, int maxScore)
    {
        if (_scoreText != null)
            _scoreText.text = $"Score: {score}/{maxScore}";
    }

    public void ShowGameOver(int score, int maxScore, bool isPerfect)
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(true);

        if (_gameOverTitleText != null)
            _gameOverTitleText.text = isPerfect ? "Perfect Score!" : "You can do better!";

        if (_gameOverDetailText != null)
        {
            _gameOverDetailText.text = isPerfect
                ? string.Format(_perfectScoreDetailText, score, maxScore)
                : string.Format(_belowPerfectScoreDetailText, score, maxScore);
        }
    }

    public void HideGameOver()
    {
        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    void OnReplayClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }
}
