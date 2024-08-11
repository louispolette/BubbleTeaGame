using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#if false

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : Editor
{
    public VisualTreeAsset _visualTree;

    private PropertyField _isDraggableToggle;
    private SerializedProperty _isDraggableProperty;
    
    public string[] _isdraggableHiddenPropertiesNames =
    {
        "CenterOnDrag"
    };

    private List<VisualElement> _isDraggableHiddenProperties = new List<VisualElement>();

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

        _isDraggableToggle = root.Q<PropertyField>("IsDraggable");
        _isDraggableToggle.RegisterCallback<ChangeEvent<bool>>(OnDraggableChanged);

        _isDraggableHiddenProperties.Clear();
        foreach (string propertyName in _isdraggableHiddenPropertiesNames)
        {
            _isDraggableHiddenProperties.Add(root.Q<PropertyField>(propertyName));
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
            ToggleDisplay(_isDraggableHiddenProperties.ToArray(), true);
        }
        else
        {
            ToggleDisplay(_isDraggableHiddenProperties.ToArray(), false);
        }
    }

    private void ToggleDisplay(VisualElement[] elements, bool displayBool)
    {
        foreach (VisualElement element in elements)
        {
            element.style.display = (displayBool) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}

#endif