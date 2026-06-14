using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeAnimator : MonoBehaviour
{
    [SerializeField] private float _fadeInDelay = 0f;
    [SerializeField] private float _fadeInDuration = 0.75f;
    [SerializeField] private float _displayDuration = 3f;
    [SerializeField] private float _fadeOutDuration = 0.75f;
    [SerializeField] private EasingCurves.EasingType _fadeInEasing = EasingCurves.EasingType.EaseInSine;
    [SerializeField] private EasingCurves.EasingType _fadeOutEasing = EasingCurves.EasingType.EaseOutSine;
    [SerializeField] private bool _playOnEnable = true;
    [SerializeField] private bool _disableWhenComplete = true;

    private CanvasGroup _canvasGroup;
    private Coroutine _animationRoutine;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        if (_playOnEnable)
            Play();
    }

    void OnDisable()
    {
        if (_animationRoutine != null)
        {
            StopCoroutine(_animationRoutine);
            _animationRoutine = null;
        }
    }

    public void Play()
    {
        if (_animationRoutine != null)
            StopCoroutine(_animationRoutine);

        _animationRoutine = StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;

        if (_fadeInDelay > 0f)
            yield return new WaitForSeconds(_fadeInDelay);

        _canvasGroup.blocksRaycasts = true;

        yield return Fade(0f, 1f, _fadeInDuration, _fadeInEasing);

        if (_displayDuration > 0f)
        {
            yield return new WaitForSeconds(_displayDuration);
            yield return Fade(1f, 0f, _fadeOutDuration, _fadeOutEasing);
            _canvasGroup.blocksRaycasts = false;
        }

        if (_disableWhenComplete)
            gameObject.SetActive(false);

        _animationRoutine = null;
    }

    IEnumerator Fade(float from, float to, float duration, EasingCurves.EasingType easingType)
    {
        if (duration <= 0f)
        {
            _canvasGroup.alpha = to;
            yield break;
        }

        var ease = EasingCurves.GetEasingFunction(easingType);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            _canvasGroup.alpha = ease(from, to, t);
            yield return null;
        }

        _canvasGroup.alpha = to;
    }
}
