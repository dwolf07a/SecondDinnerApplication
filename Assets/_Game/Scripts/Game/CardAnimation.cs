using System.Collections;
using UnityEngine;

public static class CardAnimation
{
    public static IEnumerator MoveWorld(
        RectTransform rectTransform,
        Vector3 targetWorldPosition,
        float duration,
        EasingCurves.EasingType easingType)
    {
        if (rectTransform == null)
            yield break;

        if (duration <= 0f)
        {
            rectTransform.position = targetWorldPosition;
            yield break;
        }

        var ease = EasingCurves.GetEasingFunction(easingType);
        Vector3 start = rectTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = ease(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rectTransform.position = Vector3.LerpUnclamped(start, targetWorldPosition, t);
            yield return null;
        }

        rectTransform.position = targetWorldPosition;
    }

    public static IEnumerator AnimateLocalState(
        RectTransform rectTransform,
        Vector3 targetLocalPosition,
        Vector3 targetLocalScale,
        float duration,
        EasingCurves.EasingType easingType)
    {
        if (rectTransform == null)
            yield break;

        Vector3 startPosition = rectTransform.localPosition;
        Vector3 startScale = rectTransform.localScale;

        if (duration <= 0f)
        {
            rectTransform.localPosition = targetLocalPosition;
            rectTransform.localScale = targetLocalScale;
            yield break;
        }

        var ease = EasingCurves.GetEasingFunction(easingType);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = ease(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rectTransform.localPosition = Vector3.LerpUnclamped(startPosition, targetLocalPosition, t);
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetLocalScale, t);
            yield return null;
        }

        rectTransform.localPosition = targetLocalPosition;
        rectTransform.localScale = targetLocalScale;
    }

    public static IEnumerator MoveWorldWithSpin(
        RectTransform rectTransform,
        Vector3 targetWorldPosition,
        float spinDegrees,
        float duration,
        EasingCurves.EasingType easingType,
        Vector3 targetLocalScale)
    {
        if (rectTransform == null)
            yield break;

        Vector3 startPosition = rectTransform.position;
        float startRotationZ = rectTransform.localEulerAngles.z;
        float endRotationZ = startRotationZ + spinDegrees;
        Vector3 startScale = rectTransform.localScale;

        if (duration <= 0f)
        {
            rectTransform.position = targetWorldPosition;
            rectTransform.localEulerAngles = new Vector3(0f, 0f, endRotationZ);
            rectTransform.localScale = targetLocalScale;
            yield break;
        }

        var ease = EasingCurves.GetEasingFunction(easingType);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = ease(0f, 1f, Mathf.Clamp01(elapsed / duration));
            rectTransform.position = Vector3.LerpUnclamped(startPosition, targetWorldPosition, t);
            rectTransform.localEulerAngles = new Vector3(0f, 0f, Mathf.LerpUnclamped(startRotationZ, endRotationZ, t));
            rectTransform.localScale = Vector3.LerpUnclamped(startScale, targetLocalScale, t);
            yield return null;
        }

        rectTransform.position = targetWorldPosition;
        rectTransform.localEulerAngles = new Vector3(0f, 0f, endRotationZ);
        rectTransform.localScale = targetLocalScale;
    }

    public static IEnumerator Flip(
        RectTransform rectTransform,
        GameObject frontRoot,
        GameObject backRoot,
        bool faceUp,
        float duration,
        EasingCurves.EasingType easingType)
    {
        if (rectTransform == null)
            yield break;

        if (duration <= 0f)
        {
            ApplyFace(frontRoot, backRoot, faceUp);
            yield break;
        }

        var ease = EasingCurves.GetEasingFunction(easingType);
        float elapsed = 0f;
        bool swapped = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = ease(0f, 1f, Mathf.Clamp01(elapsed / duration));
            float scaleX = t <= 0.5f ? 1f - t * 2f : (t - 0.5f) * 2f;
            rectTransform.localScale = new Vector3(Mathf.Max(scaleX, 0.02f), 1f, 1f);

            if (!swapped && t >= 0.5f)
            {
                ApplyFace(frontRoot, backRoot, faceUp);
                swapped = true;
            }

            yield return null;
        }

        rectTransform.localScale = Vector3.one;
        ApplyFace(frontRoot, backRoot, faceUp);
    }

    static void ApplyFace(GameObject frontRoot, GameObject backRoot, bool faceUp)
    {
        if (frontRoot != null)
            frontRoot.SetActive(faceUp);

        if (backRoot != null)
            backRoot.SetActive(!faceUp);
    }
}
