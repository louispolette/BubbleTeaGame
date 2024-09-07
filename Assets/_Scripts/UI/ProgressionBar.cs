using UnityEngine;

public class ProgressionBar : MonoBehaviour
{
    [Space]

    [Tooltip("Transform that will scale depending on the bar's fill level")]
    [SerializeField] private Transform _scaleTransform;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Space]

    [SerializeField] private bool _scaleX = true;
    [SerializeField] private bool _scaleY = false;

    [Space]

    [Tooltip("Scale of the bar when full")]
    [SerializeField] private float _fullBarScale = 1f;

    [Space]

    [SerializeField] private bool _useGradient = true;
    [SerializeField, ConditionalHide("_useGradient", hideInInspector:true)] private Gradient _colorGradient;

    [Space]

    [SerializeField, Range(0f, 1f)] private float _fillLevel = 1f;

    [Header("Debug")]

    [SerializeField] private bool _alwaysUpdate = false;

    public float FillLevel
    {
        get
        {
            return _fillLevel;
        }

        set
        {
            _fillLevel = Mathf.Max(0f, value);
            UpdateBar();
        }
    }

    private void Update()
    {
        if (!_alwaysUpdate) return;

        UpdateBar();
    }

    [ContextMenu("Update Bar")]
    private void UpdateBar()
    {
        ApplyScale();
        ApplyColor();
    }

    private void ApplyScale()
    {
        float newX = _scaleTransform.localScale.x;
        float newY = _scaleTransform.localScale.y;

        if (_scaleX)
        {
            newX = Mathf.LerpUnclamped(0f, _fullBarScale, _fillLevel);
        }
        if (_scaleY)
        {
            newY = Mathf.LerpUnclamped(0f, _fullBarScale, _fillLevel);
        }

        _scaleTransform.localScale = new Vector3(newX, newY, _scaleTransform.localScale.z);
    }

    private void ApplyColor()
    {
        if (!_useGradient) return;

        _spriteRenderer.color = _colorGradient.Evaluate(_fillLevel);
    }
}
