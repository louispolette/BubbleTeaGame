using System.Collections.Generic;
using UnityEngine;

public abstract class CookableObject : MonoBehaviour
{
    [Space]

    [SerializeField] private List<IngredientData> _contents;

    public List<IngredientData> Contents => _contents;

    public abstract void HandleObject(ClickableObject obj);

    protected virtual void AddIngredient(Ingredient ingredient)
    {
        ingredient.AddIngredient(this);
    }

    public void AddToContent(IngredientData ingredientData)
    {
        _contents.Add(ingredientData);
    }
}
