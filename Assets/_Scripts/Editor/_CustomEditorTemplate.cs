using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

#if false

[CustomEditor(typeof(Component))]
public class ComponentEditor : Editor
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