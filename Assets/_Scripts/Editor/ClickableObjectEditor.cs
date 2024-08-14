using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if true

public abstract class CustomEditorBase : Editor
{
    public VisualTreeAsset _visualTree;

    private VisualElement _root;
    
    protected List<ConditionalEditorDisplay> _conditionalEditorDisplays = new List<ConditionalEditorDisplay>();

    private VisualElement Root
    {
        get
        {
            if (_root == null)
            {
                BuildTree();
            }

            return _root;
        }
    }

    public override VisualElement CreateInspectorGUI()
    {
        BuildTree();

        foreach (ConditionalEditorDisplay conditionalDisplay in _conditionalEditorDisplays)
        {
            conditionalDisplay.Update();
        }

        return Root;
    }

    private void BuildTree()
    {
        _root = new VisualElement();

        _visualTree.CloneTree(_root);
    }

    protected void AddConditionalDisplay(string sourceElementName, SerializedProperty sourceProperty, (string, int)[] elementsToHideInfo)
    {
        ConditionalEditorDisplay c;

        VisualElement sourceElement = Root.Query<VisualElement>(sourceElementName);

        c = new ConditionalEditorDisplay(Root, sourceElement, sourceProperty, elementsToHideInfo);

        _conditionalEditorDisplays.Add(c);
    }

    protected class ConditionalEditorDisplay
    {
        private VisualElement _root;
        private (string, int)[] _elementsToHideInfo;
        private List<VisualElement> _elementsToHide = new List<VisualElement>();
        private VisualElement _sourceElement;
        private SerializedProperty _sourceProperty;
        private SerializedPropertyType _type;

        public ConditionalEditorDisplay(VisualElement root, VisualElement sourceElement, SerializedProperty sourceProperty, (string, int)[] elementsToHideInfo)
        {
            _root = root;
            _sourceElement = sourceElement;
            _sourceProperty = sourceProperty;
            _elementsToHideInfo = elementsToHideInfo;
            _type = _sourceProperty.propertyType;
        }

        public void Update()
        {
            RegisterElementCallback(_sourceElement);

            _elementsToHide.Clear();

            foreach ((string, int) hideableVisualInfo in _elementsToHideInfo)
            {
                VisualElement hideableVisualElement = _root.Q<VisualElement>(hideableVisualInfo.Item1);
                _elementsToHide.Add(hideableVisualElement);
            }

            UpdateDisplay();
        }

        private void RegisterElementCallback(VisualElement visualElement)
        {
            switch (_type)
            {
                case SerializedPropertyType.Boolean:
                    Debug.Log(visualElement.name);
                    visualElement.RegisterCallback<SerializedPropertyChangeEvent>(OnSourceChanged);
                    break;
                case SerializedPropertyType.Enum:
                    visualElement.RegisterCallback<ChangeEvent<System.Enum>>(OnSourceChanged);
                    break;
            }
        }

        // Why don't these get called

        private void OnSourceChanged(SerializedPropertyChangeEvent evt)
        {
            Debug.Log("ChangeEvent");
            UpdateDisplay();
        }

        private void OnSourceChanged(ChangeEvent<bool> evt)
        {
            Debug.Log("ChangeEvent");
            UpdateDisplay();
        }

        private void OnSourceChanged(ChangeEvent<System.Enum> evt)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            Debug.Log("UpdateDisplay");
            switch (_type)
            {
                case SerializedPropertyType.Boolean:
                    {
                        bool display = _sourceProperty.boolValue;

                        for (int i = 0; i < _elementsToHide.Count; i++)
                        {
                            bool inverse = _elementsToHideInfo[i].Item2 == 0;

                            _elementsToHide[i].style.display = (display ^ inverse) ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                    }
                    break;
                case SerializedPropertyType.Enum:
                    {
                        for (int i = 0; i < _elementsToHide.Count; i++)
                        {
                            bool display = _sourceProperty.enumValueIndex == _elementsToHideInfo[i].Item2;

                            _elementsToHide[i].style.display = (display) ? DisplayStyle.Flex : DisplayStyle.None;
                        }
                    }
                    break;
            }
        }
    }
}

#if true

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : CustomEditorBase
{

    public (string, int)[] _isDraggableHideableVisualInfo =
    {
        ("CenterOnDrag", 0),
        ("IsDraggableAdvancedSettings", 1),
        ("Click", 1),
        ("DragStart", 0),
        ("Drop", 0),
        ("ClickEvents", 1),
        ("DragAndMoveEvents", 0)
    };

    private void OnEnable()
    {
        SerializedProperty _isDraggableProperty = serializedObject.FindProperty("_isDraggable");

        AddConditionalDisplay("IsDraggable", _isDraggableProperty, _isDraggableHideableVisualInfo);
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = base.CreateInspectorGUI();

        return root;
    }
}

#endif

#if false

[CustomEditor(typeof(ClickableObject))]
public class ClickableObjectEditor : Editor
{
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

        CreateConditionalDisplay("IsDraggable", _isDraggableProperty, _isDraggableHideableVisualInfo);
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = base.CreateInspectorGUI();

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

#endif