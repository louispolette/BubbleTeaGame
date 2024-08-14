using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#if true

[CustomEditor(typeof(InteractableObject))]
public class InteractableObjectEditor : Editor
{
    public VisualTreeAsset _visualTree;

    private VisualElement root;

    private void Initialize()
    {
        root = new VisualElement();
        _visualTree.CloneTree(root);
    }

    public override VisualElement CreateInspectorGUI()
    {
        Initialize();



        return root;
    }
}

#endif