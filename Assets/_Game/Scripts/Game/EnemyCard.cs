using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class EnemyCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _labelText;
    [SerializeField] private GameObject _frontRoot;
    [SerializeField] private GameObject _backRoot;
    [SerializeField] private float _dealDuration = 0.45f;
    [SerializeField] private float _playDuration = 0.55f;
    [SerializeField] private float _flipDuration = 0.25f;
    [SerializeField] private EasingCurves.EasingType _moveEasing = EasingCurves.EasingType.EaseOutCubic;
    [SerializeField] private EasingCurves.EasingType _flipEasing = EasingCurves.EasingType.EaseInOutQuad;

    private RectTransform _rectTransform;
    private bool _isFaceUp;

    public bool IsInHand { get; private set; } = true;
    public bool IsOnTable { get; private set; }

    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    public void Initialize()
    {
        SetFaceUp(false);
        IsInHand = true;
        IsOnTable = false;
        gameObject.SetActive(true);
        _rectTransform.localScale = Vector3.one;
    }

    public string GetLabelText()
    {
        if(_labelText == null)
            return "";

        return _labelText.text;
    }

    public void SetFaceUp(bool faceUp)
    {
        _isFaceUp = faceUp;

        if (_frontRoot != null)
            _frontRoot.SetActive(faceUp);

        if (_backRoot != null)
            _backRoot.SetActive(!faceUp);
    }

    public IEnumerator AnimateDeal(Transform handSlot, Vector3 deckWorldPosition)
    {
        IsInHand = true;
        IsOnTable = false;
        SetFaceUp(false);

        transform.SetParent(handSlot, true);
        _rectTransform.position = deckWorldPosition;

        yield return CardAnimation.MoveWorld(_rectTransform, handSlot.position, _dealDuration, _moveEasing);
        _rectTransform.localPosition = Vector3.zero;
    }

    public IEnumerator AnimatePlayToTable(Transform tableSlot)
    {
        IsInHand = false;
        IsOnTable = true;

        yield return CardAnimation.MoveWorld(_rectTransform, tableSlot.position, _playDuration, _moveEasing);
        transform.SetParent(tableSlot, true);
        _rectTransform.localPosition = Vector3.zero;

        if (!_isFaceUp)
            yield return CardAnimation.Flip(_rectTransform, _frontRoot, _backRoot, true, _flipDuration, _flipEasing);

        SetFaceUp(true);
    }

    public void ResetForPool()
    {
        IsInHand = true;
        IsOnTable = false;
        SetFaceUp(false);
        gameObject.SetActive(false);
    }
}
