using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#if true

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : Editor
{
    public VisualTreeAsset _visualTree;

    private PropertyField _isDraggableVisual;
    private SerializedProperty _isDraggableProperty;

    public (string, bool)[] _isDraggableHideableVisualInfo =
    {
        ("CenterOnDrag", false),
        ("IsDraggableAdvancedSettings", true),
        ("Click", true),
        ("DragStart", false),
        ("Drop", false),
        ("ClickEvents", true),
        ("DragAndMoveEvents", false)
    };

    private List<VisualElement> _isDraggableHideableVisuals = new List<VisualElement>();

    private void OnEnable()
    {
        _isDraggableProperty = serializedObject.FindProperty("_isDraggable");
    }

    public override VisualElement CreateInspectorGUI()
    {
        #region get UI builder stuff

        VisualElement root = new VisualElement();

        _visualTree.CloneTree(root);

        #endregion

        _isDraggableVisual = root.Q<PropertyField>("IsDraggable");
        _isDraggableVisual.RegisterCallback<ChangeEvent<bool>>(OnDraggableChanged);

        _isDraggableHideableVisuals.Clear();

        foreach ((string, bool) hideableVisualInfo in _isDraggableHideableVisualInfo)
        {
            VisualElement hideableVisualElement = root.Q<VisualElement>(hideableVisualInfo.Item1);
            _isDraggableHideableVisuals.Add(hideableVisualElement);
        }

        CheckDraggableBool();

        return root;
    }

    private void OnDraggableChanged(ChangeEvent<bool> evt)
    {
        CheckDraggableBool();
    }

    private void CheckDraggableBool()
    {
        if (_isDraggableProperty.boolValue)
        {
            ToggleDisplay(_isDraggableHideableVisuals.ToArray(), true);
        }
        else
        {
            ToggleDisplay(_isDraggableHideableVisuals.ToArray(), false);
        }
    }

    private void ToggleDisplay(VisualElement[] elements, bool displayBool)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            bool inverse = _isDraggableHideableVisualInfo[i].Item2;

            elements[i].style.display = (displayBool ^ inverse) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

#endif