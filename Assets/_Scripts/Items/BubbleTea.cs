using System.Collections;
using UnityEngine;

public class BubbleTea : CookableObject
{
    [SerializeField] float _fillSpeed = 1f;

    private CupFiller _cupFiller;

    private Coroutine _fillCoroutine;

    private void Awake()
    {
        _cupFiller = GetComponentInChildren<CupFiller>();
    }

    [ContextMenu("Start Filling")]
    public void StartFilling()
    {
        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
        }

        _fillCoroutine = StartCoroutine(FillCoroutine());
    }

    [ContextMenu("Stop Filling")]
    public void StopFilling()
    {
        StopCoroutine(_fillCoroutine);
    }

    private IEnumerator FillCoroutine()
    {
        while (_cupFiller.FillRatio < 1f)
        {
            _cupFiller.SetFillRatio(_cupFiller.FillRatio + _fillSpeed * Time.deltaTime * 0.1f);
            yield return null;
        }

        _cupFiller.SetFillRatio(1f);
    }
}
