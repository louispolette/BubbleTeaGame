using UnityEditor;
using UnityEngine.UIElements;

#if true

[CustomEditor(typeof(InteractableObject))]
public class InteractableObjectEditor : CustomEditorBase
{

    private (string, int)[] _boundsModeMask =
    {
        ("SortingGroup", (int)InteractableObject.BoundsMode.useRenderer),
        ("SpriteRenderer", (int)InteractableObject.BoundsMode.useRenderer),
        ("Collider", (int)InteractableObject.BoundsMode.useCollider)
    };

    private void OnEnable()
    {
        SerializedProperty boundsModeProperty = serializedObject.FindProperty("_boundsMode");

        AddConditionalDisplay("BoundsMode", boundsModeProperty, _boundsModeMask);
    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = base.CreateInspectorGUI();

        return root;
    }
}

#endif