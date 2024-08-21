using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if true

[CanEditMultipleObjects]
[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : CustomEditorBase
{
    private string _visualTreePath = "Custom Editors/ClickableObjectVisualTree";

    protected override string VisualTreePath => _visualTreePath;

    public static (string, int)[] _isDraggableMask =
    {
        ("CenterOnDrag", 1),
        ("IsDraggableAdvancedSettings", 0),
        ("Click", 0),
        ("DragStart", 1),
        ("Drop", 1),
        ("ClickEvents", 0),
        ("DragAndMoveEvents", 1)
    };

    public static void SetCallbacks(CustomEditorBase target)
    {
        SerializedProperty isDraggableProperty = target.serializedObject.FindProperty("_isDraggable");

        target.AddConditionalDisplay("IsDraggable", isDraggableProperty, _isDraggableMask);
    }

    private void OnEnable()
    {
        SetCallbacks(this);
        InteractableObjectEditor.SetCallbacks(this);
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