using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if true

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : InteractableObjectEditor
{
    private string _visualTreePath = "";

    private VisualTreeAsset _visualTree;

    private VisualTreeAsset VisualTree
    {
        get
        {
            if (_visualTree == null)
            {
                Debug.Log("Fetched Visual Tree");
                return Resources.Load(_visualTreePath) as VisualTreeAsset;
            }

            return _visualTree;
        }
    }

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

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = base.CreateInspectorGUI();

        VisualElement childInspectorRoot = new VisualElement();
        VisualTree.CloneTree(childInspectorRoot);

        root.Add(childInspectorRoot);

        return root;
    }
}

#endif