using UnityEngine;

public abstract class Ingredient : MonoBehaviour
{
    [Space]

    [SerializeField] protected IngredientData _data;

    public abstract IngredientTargetMode IngredientTarget { get;}

    public enum IngredientTargetMode { bubbleTea, pastry };

    protected SpriteRenderer _spriteRenderer;

    protected CookableObject _cookableObject;

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ApplyData();
    }

    private void ApplyData()
    {
        _spriteRenderer.sprite = _data.sprite;
        gameObject.name = _data.name;
        Debug.Log("Data Applied");
    }

    public void AddIngredient(CookableObject target)
    {
        _cookableObject = target;
        PlayAnimation();
    }

    public virtual void PlayAnimation()
    {
        if (TryGetComponent(out ClickableObject obj))
        {
            obj.Lock();
        }
    }
}
