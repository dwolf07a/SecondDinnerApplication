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

    [Header("Story Panel")]
    [SerializeField] private GameObject _storyPanel;
    [SerializeField] private RectTransform _storyPanelRect;
    [SerializeField] private TextMeshProUGUI _storyText;
    [SerializeField] private RectTransform _storyTextRect;
    [SerializeField] private Button _storyDoneButton;
    [SerializeField] private float _storyPanelWidth = 760f;
    [SerializeField] private float _storyPanelPadding = 24f;
    [SerializeField] private float _storyPanelSpacing = 16f;
    [SerializeField] private float _storyDoneButtonHeight = 44f;
    [SerializeField] private float _storyDoneButtonWidth = 180f;
    [SerializeField] private float _storyPanelMinHeight = 160f;
    [SerializeField] private float _storyPanelMaxHeight = 480f;

    [Header("Game Over Text")]
    [SerializeField] private string _perfectScoreDetailText = "You scored {0}/{1}. Now you know exactly what Neil would do, but there's so much more that he can bring to the table! Schedule an interview to learn more!";
    [SerializeField] private string _belowPerfectScoreDetailText = "You scored {0}/{1}. Review the matchups and try again, or schedule an interview with Neil to learn more about his unique perspective.";

    void Awake()
    {
        CacheStoryPanelReferences();

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        HideStory();

        if (_replayButton != null)
            _replayButton.onClick.AddListener(OnReplayClicked);

        if (_storyDoneButton != null)
            _storyDoneButton.onClick.AddListener(OnStoryDoneClicked);
    }

    void OnDestroy()
    {
        if (_replayButton != null)
            _replayButton.onClick.RemoveListener(OnReplayClicked);

        if (_storyDoneButton != null)
            _storyDoneButton.onClick.RemoveListener(OnStoryDoneClicked);
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

    public void ShowStory(string story)
    {
        if (_storyText != null)
            _storyText.text = story;

        LayoutStoryPanel(story);

        if (_storyPanel != null)
            _storyPanel.SetActive(true);
    }

    public void HideStory()
    {
        if (_storyText != null)
            _storyText.text = string.Empty;

        if (_storyPanel != null)
            _storyPanel.SetActive(false);
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

    void OnStoryDoneClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStoryDone();
    }

    void CacheStoryPanelReferences()
    {
        if (_storyPanel != null && _storyPanelRect == null)
            _storyPanelRect = _storyPanel.GetComponent<RectTransform>();

        if (_storyText != null && _storyTextRect == null)
            _storyTextRect = _storyText.rectTransform;
    }

    void LayoutStoryPanel(string story)
    {
        if (_storyPanelRect == null || _storyText == null || _storyTextRect == null)
            return;

        float contentWidth = _storyPanelWidth - _storyPanelPadding * 2f;
        float maxTextHeight = _storyPanelMaxHeight
            - _storyPanelPadding * 2f
            - _storyPanelSpacing
            - _storyDoneButtonHeight;

        _storyText.ForceMeshUpdate(true, true);
        float preferredTextHeight = _storyText.GetPreferredValues(story ?? string.Empty, contentWidth, 0).y;
        float textHeight = Mathf.Min(preferredTextHeight, maxTextHeight);
        bool useScroll = preferredTextHeight > maxTextHeight;

        float panelHeight = _storyPanelPadding * 2f + textHeight + _storyPanelSpacing + _storyDoneButtonHeight;
        panelHeight = Mathf.Clamp(panelHeight, _storyPanelMinHeight, _storyPanelMaxHeight);

        _storyPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _storyPanelWidth);
        _storyPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelHeight);

        _storyTextRect.anchorMin = new Vector2(0.5f, 1f);
        _storyTextRect.anchorMax = new Vector2(0.5f, 1f);
        _storyTextRect.pivot = new Vector2(0.5f, 1f);
        _storyTextRect.anchoredPosition = new Vector2(0f, -_storyPanelPadding);
        _storyTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, contentWidth);
        _storyTextRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);

        _storyText.textWrappingMode = TextWrappingModes.Normal;
        _storyText.overflowMode = useScroll ? TextOverflowModes.Masking : TextOverflowModes.Overflow;

        if (_storyDoneButton != null)
        {
            RectTransform doneButtonRect = _storyDoneButton.GetComponent<RectTransform>();
            doneButtonRect.anchorMin = new Vector2(0.5f, 0f);
            doneButtonRect.anchorMax = new Vector2(0.5f, 0f);
            doneButtonRect.pivot = new Vector2(0.5f, 0f);
            doneButtonRect.anchoredPosition = new Vector2(0f, _storyPanelPadding);
            doneButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _storyDoneButtonWidth);
            doneButtonRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _storyDoneButtonHeight);
        }
    }
}
