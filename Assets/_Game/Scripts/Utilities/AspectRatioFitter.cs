using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class AspectRatioFitter : MonoBehaviour
{
    [SerializeField] private Vector2 _targetAspect = new Vector2(16, 9);
    private Camera _cam;
    private Vector2 _lastResolution;

    void Start()
    {
        _cam = GetComponent<Camera>();
        ApplyAspect();
    }

    void Update()
    {
        // Only update if screen size changed to save performance
        if (_lastResolution.x != Screen.width || _lastResolution.y != Screen.height)
        {
            ApplyAspect();
        }
    }

    void ApplyAspect()
    {
        _lastResolution = new Vector2(Screen.width, Screen.height);

        float target = _targetAspect.x / _targetAspect.y;
        float windowAspect = _lastResolution.x / _lastResolution.y;
        float scaleHeight = windowAspect / target;

        Rect rect = _cam.rect;

        if (scaleHeight < 1.0f) // Letterbox (taller screen)
        {
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else // Pillarbox (wider screen)
        {
            float scaleWidth = 1.0f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
        }

        _cam.rect = rect;
    }
}