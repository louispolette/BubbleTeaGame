using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class CustomEditorBase : Editor
{
    public VisualTreeAsset _visualTree;

    private VisualElement _root;

    protected List<ConditionalEditorDisplay> _conditionalEditorDisplays = new List<ConditionalEditorDisplay>();

    public VisualElement Root
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
            conditionalDisplay.Root = this.Root;
            conditionalDisplay.Update();
        }

        return Root;
    }

    private void BuildTree()
    {
        _root = new VisualElement();

        _visualTree.CloneTree(_root);
    }

    protected void AddConditionalDisplay(string sourceElementName, SerializedProperty sourceProperty, (string, int)[] elementMask)
    {
        ConditionalEditorDisplay c;

        c = new ConditionalEditorDisplay(Root, sourceElementName, sourceProperty, elementMask);

        _conditionalEditorDisplays.Add(c);
    }

    protected class ConditionalEditorDisplay
    {
        private VisualElement _root;
        private (string, int)[] _elementsToHideInfo;
        private List<VisualElement> _elementsToHide = new List<VisualElement>();
        private string _sourceElementName;
        private SerializedProperty _sourceProperty;
        private SerializedPropertyType _type;

        public VisualElement Root { get => _root; set => _root = value; }

        public VisualElement SourceElement
        {
            get
            {
                return _root.Q<VisualElement>(_sourceElementName);
            }
        }

        public ConditionalEditorDisplay(VisualElement root, string sourceElementName, SerializedProperty sourceProperty, (string, int)[] elementsToHideInfo)
        {
            _root = root;
            _sourceElementName = sourceElementName;
            _sourceProperty = sourceProperty;
            _elementsToHideInfo = elementsToHideInfo;
            _type = _sourceProperty.propertyType;
        }

        public void Update()
        {
            RegisterElementCallback(SourceElement);

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
            visualElement.RegisterCallback<SerializedPropertyChangeEvent>(OnSourceChanged);
        }

        private void OnSourceChanged(SerializedPropertyChangeEvent evt)
        {
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            switch (_type)
            {
                case SerializedPropertyType.Boolean:
                    UpdateDisplayBool();
                    break;
                case SerializedPropertyType.Enum:
                    UpdateDisplayEnum();
                    break;
            }

            void UpdateDisplayBool()
            {
                bool display = _sourceProperty.boolValue;

                for (int i = 0; i < _elementsToHide.Count; i++)
                {
                    bool inverse = _elementsToHideInfo[i].Item2 == 0;

                    _elementsToHide[i].style.display = (display ^ inverse) ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }

            void UpdateDisplayEnum()
            {
                for (int i = 0; i < _elementsToHide.Count; i++)
                {
                    bool display = _sourceProperty.enumValueIndex == _elementsToHideInfo[i].Item2;

                    _elementsToHide[i].style.display = (display) ? DisplayStyle.Flex : DisplayStyle.None;
                }
            }
        }
    }
}