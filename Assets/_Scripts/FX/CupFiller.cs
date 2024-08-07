using System;
using UnityEngine;

public class CupFiller : MonoBehaviour
{
    [Space]

    [SerializeField, Range(0f, 1f)] private float _fillRatio = 1f;

    [Space]

    [SerializeField] private ScaledObject[] _objectsToScale;

    [Space]

    [SerializeField] private Renderer[] _renderersToHide;

    [Space]

    [SerializeField] private bool _updateEveryFrame = false;

    private bool _renderersEnabled = true;

    public float FillRatio => _fillRatio;

    private void Awake()
    {
        CheckIfEmpty();
    }

    private void Update()
    {
        if (!_updateEveryFrame) return;

        UpdateObjects();
        CheckIfEmpty();
    }

    private void UpdateObjects()
    {
        foreach (ScaledObject obj in _objectsToScale)
        {
            if (obj.affectScale)
            {
                float width = Mathf.Lerp(obj.emptyScale.x, obj.fullScale.x, _fillRatio);
                float height = Mathf.Lerp(obj.emptyScale.y, obj.fullScale.y, _fillRatio);

                obj.objectTransform.localScale = new Vector3(width, height, obj.objectTransform.localScale.z);
            }
            if (obj.affectPosition)
            {
                float xPos = Mathf.Lerp(obj.emptyPosition.x, obj.fullPosition.x, _fillRatio);
                float yPos = Mathf.Lerp(obj.emptyPosition.y, obj.fullPosition.y, _fillRatio);

                obj.objectTransform.localPosition = new Vector3(xPos, yPos, obj.objectTransform.localPosition.z);
            }
        }
    }

    [ContextMenu("Update")]
    private void UpdateAndCheck()
    {
        UpdateObjects();
        CheckIfEmpty();
    }

    private void CheckIfEmpty()
    {
        if (_fillRatio > 0f)
        {
            EnableRenderers();
        }
        else
        {
            DisableRenderers();
        }
    }

    private void DisableRenderers()
    {
        if (!_renderersEnabled) return;

        foreach(Renderer renderer in _renderersToHide)
        {
            renderer.enabled = false;
        }

        _renderersEnabled = false;
    }

    private void EnableRenderers()
    {
        if (_renderersEnabled) return;

        foreach (Renderer renderer in _renderersToHide)
        {
            renderer.enabled = true;
        }

        _renderersEnabled = true;
    }

    public void SetFillRatio(float ratio)
    {
        _fillRatio = Mathf.Clamp01(ratio);
        UpdateObjects();
        CheckIfEmpty();
    }

    [Serializable]
    private struct ScaledObject
    {
        public Transform objectTransform;

        [Space]

        public bool affectPosition;
        public Vector2 emptyPosition;
        public Vector2 fullPosition;

        [Space]

        public bool affectScale;
        public Vector2 emptyScale;
        public Vector2 fullScale;
    }
}
