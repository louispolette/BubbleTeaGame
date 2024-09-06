using UnityEngine;

public abstract class Ingredient : MonoBehaviour
{
    [Space]

    [SerializeField] private IngredientData _data;

    public abstract IngredientTargetMode IngredientTarget { get;}

    public enum IngredientTargetMode { bubbleTea, pastry };

    protected SpriteRenderer _spriteRenderer;

    protected CookableObject _cookableObject;

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ApplyData(_data);
    }

    public void ApplyData(IngredientData data)
    {
        _spriteRenderer.sprite = data.sprite;
        gameObject.name = data.name;
        Debug.Log("IngredientData Applied");
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
