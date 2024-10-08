using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if false

[CanEditMultipleObjects]
[CustomEditor(typeof())]
public class _CustomEditorTemplate : CustomEditorBase
{
    private string _visualTreePath = "Custom Editors/ClickableObjectVisualTree";

    protected override string VisualTreePath => _visualTreePath;

    public static void SetCallbacks(CustomEditorBase target)
    {
        
    }

    private void OnEnable()
    {
        SetCallbacks(this);
    }


    protected override void BuildTree()
    {
        _root = new VisualElement();

        if (VisualTree == null) return;

        VisualTree.CloneTree(_root);
    }

    public override VisualElement CreateInspectorGUI()
    {
        BuildTree();
        VisualElement root = base.CreateInspectorGUI();

        return root;
    }
}

#endif