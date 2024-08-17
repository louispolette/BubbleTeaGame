
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if true

[CustomEditor(typeof(InteractableObject), editorForChildClasses: true)]
public class InteractableObjectEditor : CustomEditorBase
{
    private string _visualTreePath = "Custom Editors/InteractableObjectVisualTree";

    protected override string VisualTreePath => _visualTreePath;

    private static (string, int)[] _boundsModeMask =
    {
        ("SortingGroup", (int)InteractableObject.BoundsMode.useRenderer),
        ("SpriteRenderer", (int)InteractableObject.BoundsMode.useRenderer),
        ("Collider", (int)InteractableObject.BoundsMode.useCollider)
    };

    public static void SetCallbacks(CustomEditorBase target)
    {
        SerializedProperty boundsModeProperty = target.serializedObject.FindProperty("_boundsMode");

        target.AddConditionalDisplay("BoundsMode", boundsModeProperty, _boundsModeMask);
    }

    protected void OnEnable()
    {
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