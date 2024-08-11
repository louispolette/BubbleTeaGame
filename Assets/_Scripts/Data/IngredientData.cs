using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "IngredientData", menuName = "Data/Ingredient Database")]
[System.Serializable]
public class IngredientData : ScriptableObject
{
    new public string name;

    public Sprite sprite;
}

#if UNITY_EDITOR

[CustomEditor(typeof(IngredientData))]
public class IngredientDataEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        var ingredientData = (IngredientData)target;

        if (ingredientData == null || ingredientData.sprite == null)
        {
            return null;
        }

        var texture = new Texture2D(width, height);

        EditorUtility.CopySerialized(ingredientData.sprite.texture, texture);

        return texture;
    }
}

#endif
