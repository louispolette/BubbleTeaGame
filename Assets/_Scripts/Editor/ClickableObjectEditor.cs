using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : Editor
{
    public VisualTreeAsset _visualTree;

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new VisualElement();

        _visualTree.CloneTree(root);

        return root;
    }
}
