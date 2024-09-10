using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ImageSelector : VisualElement
{
    public new class UxmlFactory : UxmlFactory<ImageSelector> { }
    public ImageSelector() { }

    private const string VISUAL_TREE_PATH = "Assets/UXML/Resources/Custom Controls/ImageSelectorVisualTree.uxml";

    Image Image => this.Q<Image>("Image");

    public ImageSelector(SerializedProperty property)
    {
        Init(property);
    }

    public void Init(SerializedProperty property)
    {
        VisualTreeAsset asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(VISUAL_TREE_PATH);

        asset.CloneTree(this);


    }
}