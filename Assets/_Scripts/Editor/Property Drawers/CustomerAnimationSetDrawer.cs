using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.AssetLoading;

[CustomPropertyDrawer(typeof(CustomerAnimationSet))]
public class CustomerAnimationSetDrawer : PropertyDrawer
{
    private const string VISUAL_TREE_PATH = "Property Drawers/CustomerAnimationSetVisualTree";

    private VisualTreeAsset _visualTree;

    private VisualTreeAsset VisualTree
    {
        get
        {
            if (_visualTree == null)
            {
                _visualTree = AssetLoadingUtils.LoadVisualTreeAsset(VISUAL_TREE_PATH);
            }

            return _visualTree;
        }
    }

    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new VisualElement();

        if (VisualTree != null)
        {
            VisualTree.CloneTree(root);
        }

        Image image = new Image();
        image.sprite = (Sprite)property.FindPropertyRelative("idleSprite").objectReferenceValue;
        Debug.Log(image);
        root.Add(image);

        return root;
    }
}
