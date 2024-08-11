using System.Collections;
using UnityEngine;

public class BubbleTeaIngredient : Ingredient
{
    public override IngredientTargetMode IngredientTarget { get => IngredientTargetMode.bubbleTea; }

    private Animator _animator;

    protected override void Awake()
    {
        base.Awake();

        _animator = GetComponentInChildren<Animator>();
    }

    public override void PlayAnimation()
    {
        base.PlayAnimation();

        StartCoroutine(AnimationCoroutine());
    }

    private IEnumerator AnimationCoroutine()
    {
        yield return new WaitForSeconds(1f);

        _cookableObject.AddToContent(_data);

        Destroy(gameObject);
    }
}
