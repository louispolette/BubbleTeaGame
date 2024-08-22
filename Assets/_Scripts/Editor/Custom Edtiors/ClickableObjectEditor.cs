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
        ("DragAdvancedSettings", 1),
        ("Click", 0),
        ("DragStart", 1),
        ("Drop", 1),
        ("ClickEvents", 0),
        ("DragAndMoveEvents", 1)
    };

    public static (string, int)[] _returnMask =
    {
        ("ReturnAdvancedSettings", 1),
    };

    public static void SetCallbacks(CustomEditorBase target)
    {
        SerializedProperty isDraggableProperty = target.serializedObject.FindProperty("_isDraggable");
        SerializedProperty returnProperty = target.serializedObject.FindProperty("_returnToRestingPosition");

        target.AddConditionalDisplay("IsDraggable", isDraggableProperty, _isDraggableMask);
        target.AddConditionalDisplay("ReturnToRestingPosition", returnProperty, _returnMask);
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