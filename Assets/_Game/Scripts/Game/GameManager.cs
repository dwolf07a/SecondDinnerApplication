using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum GameState
    {
        Setup,
        AITurn,
        PlayerTurn,
        Scoring,
        GameOver
    }

    public static GameManager Instance { get; private set; }

    private const int PerfectMatchScore = 3;
    private const int PartialMatchScore = 1;
    private int PerfectTotalScore =0;


    [Header("Board Areas")]
    [SerializeField] private Transform _deckOrigin;
    [SerializeField] private Transform _enemyHand;
    [SerializeField] private Transform _playerHand;
    [SerializeField] private Transform _enemyTableSlot;
    [SerializeField] private Transform _playerTableSlot;

    [Header("UI")]
    [SerializeField] private GameUI _gameUI;

    [Header("Status Text")]
    [SerializeField] private string _setupStatus = "Dealing cards...";
    [SerializeField] private string _aiChoosingStatus = "AI is choosing a card...";
    [SerializeField] private string _aiPlayedStatus = "AI played {0}. Choose your response.";
    [SerializeField] private string _playerTurnStatus = "Select the best response from your hand.";
    [SerializeField] private string _playingCardStatus = "Playing your card...";
    [SerializeField] private string _scoringStatus = "Scoring the round...";
    [SerializeField] private string _perfectMatchStatus = "Perfect match! +{0} points.";
    [SerializeField] private string _partialMatchSingularStatus = "Less than ideal. +{0} point.";
    [SerializeField] private string _partialMatchPluralStatus = "Less than ideal. +{0} points.";
    [SerializeField] private string _gameOverStatus = "Game over.";

    [Header("Card Prefabs")]
    [SerializeField] private GameObject[] _enemyCards;
    [SerializeField] private GameObject[] _playerCards;
/*
    [Header("Card Labels")]
    [SerializeField] private string[] _enemyLabels =
    {
        "Outage",
        "Ban Wave",
        "Economy Bug",
        "Server Crash",
        "Cheater Flood"
    };

    [SerializeField] private string[] _playerLabels =
    {
        "Rollback Plan",
        "Comms Template",
        "Hotfix Patch",
        "Scale Up",
        "Moderation Tool"
    };
    */

    [Header("Timing")]
    [SerializeField] private float _setupPause = 0.35f;
    [SerializeField] private float _turnPause = 0.5f;
    [SerializeField] private float _scoringPause = 0.75f;
    [SerializeField] private float _destroyDuration = 0.35f;

    private readonly List<EnemyCard> _enemyHandCards = new();
    private readonly List<PlayerCard> _playerHandCards = new();
    private readonly List<EnemyCard> _spawnedEnemyCards = new();
    private readonly List<PlayerCard> _spawnedPlayerCards = new();

    private GameState _currentState;
    private EnemyCard _activeEnemyCard;
    private PlayerCard _activePlayerCard;
    private int _score;
    private Coroutine _stateRoutine;
    private bool _isProcessingPlayerInput;

    public GameState CurrentState => _currentState;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        BeginSetup();
    }

    public bool CanPlayCard(PlayerCard card)
    {
        return _currentState == GameState.PlayerTurn
            && !_isProcessingPlayerInput
            && card != null
            && card.IsInHand;
    }

    public void OnPlayerCardSelected(PlayerCard card)
    {
        if (!CanPlayCard(card))
            return;

        _activePlayerCard = card;
        _isProcessingPlayerInput = true;
        DisablePlayerInput();
        StartStateRoutine(PlayerTurnRoutine());
    }

    public void RestartGame()
    {
        StopStateRoutine();
        ClearBoard();
        _score = 0;
        _activeEnemyCard = null;
        _activePlayerCard = null;
        _isProcessingPlayerInput = false;

        if (_gameUI != null)
        {
            _gameUI.HideGameOver();
            _gameUI.UpdateScore(_score, PerfectTotalScore);
        }

        BeginSetup();
    }

    void BeginSetup()
    {
        StartStateRoutine(SetupRoutine());
    }

    void StartStateRoutine(IEnumerator routine)
    {
        StopStateRoutine();
        _stateRoutine = StartCoroutine(routine);
    }

    void StopStateRoutine()
    {
        if (_stateRoutine != null)
        {
            StopCoroutine(_stateRoutine);
            _stateRoutine = null;
        }
    }

    void SetState(GameState state, string statusMessage)
    {
        _currentState = state;

        if (_gameUI != null)
            _gameUI.SetStatus(statusMessage);
    }

    IEnumerator SetupRoutine()
    {
        SetState(GameState.Setup, _setupStatus);
        ClearBoard();
        _score = 0;

        if (_gameUI != null)
            _gameUI.UpdateScore(_score, PerfectTotalScore);

        yield return SpawnAndDealCards();
        yield return new WaitForSeconds(_setupPause);

        StartStateRoutine(AITurnRoutine());
    }

    IEnumerator SpawnAndDealCards()
    {
        if (_enemyCards.Length == 0 || _playerCards.Length == 0 || _enemyCards.Length != _playerCards.Length)
        {
            Debug.LogError("GameManager is missing card prefab references.");
            yield break;
        }

        if (_deckOrigin == null || _enemyHand == null || _playerHand == null)
        {
            Debug.LogError("GameManager is missing board area references.");
            yield break;
        }

        PerfectTotalScore = _enemyCards.Length * PerfectMatchScore;

        Vector3 deckPosition = _deckOrigin.position;

        Canvas.ForceUpdateCanvases();
        yield return null;

        for (int i = 0; i < _enemyCards.Length; i++)
        {
            Transform enemySlot = _enemyHand.GetChild(i);

            EnemyCard enemyCard = Instantiate(_enemyCards[i], enemySlot).GetComponent<EnemyCard>();
            enemyCard.Initialize();
            _spawnedEnemyCards.Add(enemyCard);
            _enemyHandCards.Add(enemyCard);

            yield return enemyCard.AnimateDeal(enemySlot, deckPosition);
        }

        List<EnemyCard> shuffledTargets = new(_spawnedEnemyCards);
        Shuffle(shuffledTargets);

        for (int i = 0; i < _playerCards.Length; i++)
        {
            Transform playerSlot = _playerHand.GetChild(i);

            PlayerCard playerCard = Instantiate(_playerCards[i], deckPosition, Quaternion.identity).GetComponent<PlayerCard>();
            playerCard.Initialize();
            _spawnedPlayerCards.Add(playerCard);
            _playerHandCards.Add(playerCard);

            yield return playerCard.AnimateDeal(playerSlot, deckPosition);
        }
    }

    IEnumerator AITurnRoutine()
    {
        SetState(GameState.AITurn, _aiChoosingStatus);

        if (_enemyHandCards.Count == 0)
        {
            StartStateRoutine(GameOverRoutine());
            yield break;
        }

        yield return new WaitForSeconds(_turnPause);

        int randomIndex = Random.Range(0, _enemyHandCards.Count);
        _activeEnemyCard = _enemyHandCards[randomIndex];
        _enemyHandCards.RemoveAt(randomIndex);

        SetState(GameState.AITurn, string.Format(_aiPlayedStatus, _activeEnemyCard.GetLabelText()));

        if (_enemyTableSlot != null)
            yield return _activeEnemyCard.AnimatePlayToTable(_enemyTableSlot);

        yield return new WaitForSeconds(_turnPause);
        StartStateRoutine(BeginPlayerTurn());
    }

    IEnumerator BeginPlayerTurn()
    {
        SetState(GameState.PlayerTurn, _playerTurnStatus);
        _isProcessingPlayerInput = false;
        EnablePlayerInput();
        yield break;
    }

    IEnumerator PlayerTurnRoutine()
    {
        SetState(GameState.PlayerTurn, _playingCardStatus);

        if (_activePlayerCard == null)
        {
            StartStateRoutine(AITurnRoutine());
            yield break;
        }

        _playerHandCards.Remove(_activePlayerCard);

        if (_playerTableSlot != null)
            yield return _activePlayerCard.AnimatePlayToTable(_playerTableSlot);

        yield return new WaitForSeconds(_turnPause);
        StartStateRoutine(ScoringRoutine());
    }

    IEnumerator ScoringRoutine()
    {
        SetState(GameState.Scoring, _scoringStatus);

        bool isPerfect = _activePlayerCard != null
            && _activePlayerCard.IsPerfectTarget(_activeEnemyCard);

        int roundScore = isPerfect ? PerfectMatchScore : PartialMatchScore;
        _score += roundScore;

        if (_gameUI != null)
            _gameUI.UpdateScore(_score, PerfectTotalScore);

        SetState(
            GameState.Scoring,
            isPerfect
                ? string.Format(_perfectMatchStatus, roundScore)
                : string.Format(roundScore == 1 ? _partialMatchSingularStatus : _partialMatchPluralStatus, roundScore));

        yield return new WaitForSeconds(_scoringPause);
        yield return DestroyPlayedCards();

        _activeEnemyCard = null;
        _activePlayerCard = null;
        _isProcessingPlayerInput = false;

        if (_enemyHandCards.Count == 0 && _playerHandCards.Count == 0)
            StartStateRoutine(GameOverRoutine());
        else
            StartStateRoutine(AITurnRoutine());
    }

    IEnumerator DestroyPlayedCards()
    {
        float elapsed = 0f;
        Vector3 enemyStartScale = _activeEnemyCard != null ? _activeEnemyCard.transform.localScale : Vector3.one;
        Vector3 playerStartScale = _activePlayerCard != null ? _activePlayerCard.transform.localScale : Vector3.one;

        while (elapsed < _destroyDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _destroyDuration);

            if (_activeEnemyCard != null)
                _activeEnemyCard.transform.localScale = Vector3.Lerp(enemyStartScale, Vector3.zero, t);

            if (_activePlayerCard != null)
                _activePlayerCard.transform.localScale = Vector3.Lerp(playerStartScale, Vector3.zero, t);

            yield return null;
        }

        if (_activeEnemyCard != null)
        {
            Destroy(_activeEnemyCard.gameObject);
            _spawnedEnemyCards.Remove(_activeEnemyCard);
        }

        if (_activePlayerCard != null)
        {
            Destroy(_activePlayerCard.gameObject);
            _spawnedPlayerCards.Remove(_activePlayerCard);
        }
    }

    IEnumerator GameOverRoutine()
    {
        SetState(GameState.GameOver, _gameOverStatus);

        bool isPerfect = _score >= PerfectTotalScore;

        if (_gameUI != null)
            _gameUI.ShowGameOver(_score, PerfectTotalScore, isPerfect);

        yield break;
    }

    void EnablePlayerInput()
    {
        foreach (PlayerCard card in _playerHandCards)
            card.SetInputEnabled(true);
    }

    void DisablePlayerInput()
    {
        foreach (PlayerCard card in _playerHandCards)
            card.SetInputEnabled(false);
    }

    void ClearBoard()
    {
        DisablePlayerInput();

        foreach (EnemyCard card in _spawnedEnemyCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        foreach (PlayerCard card in _spawnedPlayerCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        _spawnedEnemyCards.Clear();
        _spawnedPlayerCards.Clear();
        _enemyHandCards.Clear();
        _playerHandCards.Clear();
        _activeEnemyCard = null;
        _activePlayerCard = null;
    }

    static string GetLabel(string[] labels, int index, string fallback)
    {
        if (labels != null && index >= 0 && index < labels.Length && !string.IsNullOrWhiteSpace(labels[index]))
            return labels[index];

        return fallback;
    }

    static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
        }
    }
}
