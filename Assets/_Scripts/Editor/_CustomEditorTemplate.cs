using UnityEditor;
using UnityEngine.UIElements;

#if false

[CustomEditor(typeof(/*type*/), editorForChildClasses: /*bool*/)]
public class _CustomEditorTemplate : CustomEditorBase
{

    private void OnEnable()
    {

    }

    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = base.CreateInspectorGUI();

        return root;
    }
}

#endif