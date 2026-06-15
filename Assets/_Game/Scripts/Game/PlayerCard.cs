using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class PlayerCard : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private GameObject _frontRoot;
    [SerializeField] private GameObject _backRoot;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _dealDuration = 0.45f;
    [SerializeField] private float _playDuration = 0.55f;
    [SerializeField] private float _playSpinDegrees = 360f;
    [SerializeField] private float _hoverDuration = 0.2f;
    [SerializeField] private float _hoverScale = 1.12f;
    [SerializeField] private Vector3 _hoverLocalOffset = new Vector3(0f, 48f, -40f);
    [SerializeField] private EasingCurves.EasingType _moveEasing = EasingCurves.EasingType.EaseOutCubic;
    [SerializeField] private EasingCurves.EasingType _hoverEasing = EasingCurves.EasingType.EaseOutCubic;

    [SerializeField] private GameObject _playCardButton;

    private RectTransform _rectTransform;
    private bool _inputEnabled;
    private bool _isHovered;
    private bool _isStorySelected;
    private Vector3 _restLocalPosition = Vector3.zero;
    private Vector3 _restLocalScale = Vector3.one;
    private Coroutine _hoverRoutine;

    public string Label { get; private set; }
    [SerializeField] private EnemyCard _perfectTarget;
    [SerializeField] [TextArea(3, 8)] private string _story;
    public bool IsInHand { get; private set; } = true;
    public bool IsOnTable { get; private set; }

    public string Story => _story ?? string.Empty;

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();
            
        if(_playCardButton != null)
            _playCardButton.SetActive(false);
    }

    public void Initialize()
    {
        SetFaceUp(true);
        IsInHand = true;
        IsOnTable = false;
        SetInputEnabled(false);
        gameObject.SetActive(true);
        CaptureRestTransform();
    }

    public bool IsPerfectTarget(EnemyCard enemyCard)
    {
        if(_perfectTarget == null)
            return false;
        
        EnemyCard perfectTargetCard = _perfectTarget.GetComponent<EnemyCard>();
        if(perfectTargetCard == null)
            return false;

        return perfectTargetCard.GetLabelText() == enemyCard.GetLabelText();
    }

    public void SetFaceUp(bool faceUp)
    {
        if (_frontRoot != null)
            _frontRoot.SetActive(faceUp);

        if (_backRoot != null)
            _backRoot.SetActive(!faceUp);
    }

    public void SetInputEnabled(bool enabled, bool resetHover = true)
    {
        _inputEnabled = enabled;

        if (_canvasGroup != null)
        {
            _canvasGroup.interactable = enabled;
            _canvasGroup.blocksRaycasts = enabled;
        }

        if (!enabled && resetHover && !_isStorySelected)
            ResetHoverImmediate();
    }

    public void PrepareForStoryDisplay()
    {
        StopHoverAnimation();
        _isStorySelected = true;
        _isHovered = true;
        transform.SetAsLastSibling();
        _rectTransform.localPosition = _restLocalPosition + _hoverLocalOffset;
        _rectTransform.localScale = _restLocalScale * _hoverScale;

        if (_playCardButton != null)
            _playCardButton.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!CanHover())
            return;

        _isHovered = true;
        transform.SetAsLastSibling();
        AnimateHover(_restLocalPosition + _hoverLocalOffset, _restLocalScale * _hoverScale);

        if(_playCardButton != null)
            _playCardButton.SetActive(true);

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isHovered || _isStorySelected)
            return;

        _isHovered = false;
        AnimateHover(_restLocalPosition, _restLocalScale);

        if(_playCardButton != null)
            _playCardButton.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_inputEnabled || !IsInHand)
            return;

        if (GameManager.Instance != null)
            GameManager.Instance.OnPlayerCardSelected(this);

        
        if(_playCardButton != null)
            _playCardButton.SetActive(false);
    }

    public IEnumerator AnimateDeal(Transform handSlot, Vector3 deckWorldPosition)
    {
        IsInHand = true;
        IsOnTable = false;
        SetFaceUp(true);

        transform.SetParent(handSlot, true);
        _rectTransform.position = deckWorldPosition;
        _rectTransform.localScale = Vector3.one;

        yield return CardAnimation.MoveWorld(_rectTransform, handSlot.position, _dealDuration, _moveEasing);
        _rectTransform.localPosition = Vector3.zero;
        CaptureRestTransform();
    }

    public IEnumerator AnimatePlayToTable(Transform tableSlot)
    {
        StopHoverAnimation();
        _isStorySelected = false;
        _isHovered = false;

        IsInHand = false;
        IsOnTable = true;
        SetInputEnabled(false, resetHover: false);

        transform.SetParent(tableSlot.parent, true);

        yield return CardAnimation.MoveWorldWithSpin(
            _rectTransform,
            tableSlot.position,
            _playSpinDegrees,
            _playDuration,
            _moveEasing,
            Vector3.one);

        transform.SetParent(tableSlot, false);
        _rectTransform.localPosition = Vector3.zero;
        _rectTransform.localRotation = Quaternion.identity;
        _rectTransform.localScale = Vector3.one;
    }

    public void ResetForPool()
    {
        _isStorySelected = false;
        ResetHoverImmediate();
        IsInHand = true;
        IsOnTable = false;
        SetInputEnabled(false);
        SetFaceUp(true);
        gameObject.SetActive(false);
    }

    bool CanHover()
    {
        return _inputEnabled
            && IsInHand
            && GameManager.Instance != null
            && GameManager.Instance.CanPlayCard(this);
    }

    void CaptureRestTransform()
    {
        _restLocalPosition = _rectTransform.localPosition;
        _restLocalScale = _rectTransform.localScale;
        _isHovered = false;
    }

    void AnimateHover(Vector3 targetLocalPosition, Vector3 targetLocalScale)
    {
        if (_hoverRoutine != null)
            StopCoroutine(_hoverRoutine);

        _hoverRoutine = StartCoroutine(HoverRoutine(targetLocalPosition, targetLocalScale));
    }

    IEnumerator HoverRoutine(Vector3 targetLocalPosition, Vector3 targetLocalScale)
    {
        yield return CardAnimation.AnimateLocalState(
            _rectTransform,
            targetLocalPosition,
            targetLocalScale,
            _hoverDuration,
            _hoverEasing);

        _hoverRoutine = null;
    }

    void ResetHoverImmediate()
    {
        StopHoverAnimation();
        _rectTransform.localPosition = _restLocalPosition;
        _rectTransform.localScale = _restLocalScale;
    }

    void StopHoverAnimation()
    {
        if (_hoverRoutine != null)
        {
            StopCoroutine(_hoverRoutine);
            _hoverRoutine = null;
        }

        _isHovered = false;
    }
}
