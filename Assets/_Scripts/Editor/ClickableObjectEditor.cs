using UnityEditor;
using UnityEngine.UIElements;

#if true

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : CustomEditorBase
{

    public (string, int)[] _isDraggableMask =
    {
        ("CenterOnDrag", 1),
        ("IsDraggableAdvancedSettings", 0),
        ("Click", 0),
        ("DragStart", 1),
        ("Drop", 1),
        ("ClickEvents", 0),
        ("DragAndMoveEvents", 1)
    };

    private void OnEnable()
    {
        SerializedProperty isDraggableProperty = serializedObject.FindProperty("_isDraggable");

        AddConditionalDisplay("IsDraggable", isDraggableProperty, _isDraggableMask);
    }

    // This doesn't get called when nested

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = base.CreateInspectorGUI();

        return root;
    }
}

#endif