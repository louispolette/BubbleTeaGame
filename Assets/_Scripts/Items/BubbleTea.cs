using System.Collections;
using UnityEngine;

public class BubbleTea : CookableObject
{
    [Space]

    [SerializeField] float _fillSpeed = 1f;

    private CupFiller _cupFiller;

    private Coroutine _fillCoroutine;

    private void Awake()
    {
        _cupFiller = GetComponentInChildren<CupFiller>();
    }

    public override void HandleObject(ClickableObject obj)
    {
        if (obj.TryGetComponent(out Ingredient ingredient))
        {
            if (ingredient.IngredientTarget == Ingredient.IngredientTargetMode.bubbleTea)
            {
                AddIngredient(ingredient);
            }
        }
    }

    #region filling

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

    #endregion
}
