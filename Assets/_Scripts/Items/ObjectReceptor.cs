using UnityEngine;
using UnityEngine.Events;

public class ObjectReceptor : InteractableObject
{
    [Tooltip("Action � effectuer lorsqu'un objet est re�u")]
    public UnityEvent<ClickableObject> onObjectReceived;

    [Tooltip("OnObjectReceived ne sera pas d�clench� si l'objet re�u ne poss�de pas un des tags de la liste\n\nAccepte tout si la liste est vide")]
    [SerializeField] private string[] _requiredTag;

    private void OnEnable()
    {
        ClickManager.OnReleaseObject += CheckMousePosition;
    }

    private void OnDisable()
    {
        ClickManager.OnReleaseObject -= CheckMousePosition;
    }

    private void CheckMousePosition()
    {
        if (!CheckIfMouseInArea()) return;

        var obj = ClickManager.draggedObject;

        if (obj == null || !ObjectHasValidTag(obj)) return;
        
        onObjectReceived.Invoke(ClickManager.draggedObject);
    }

    private bool ObjectHasValidTag(ClickableObject obj)
    {
        if (_requiredTag.Length == 0) return true;

        bool hasTag = false;

        foreach (var tag in _requiredTag)
        {
            if (obj.CompareTag(tag))
            {
                hasTag = true;
                break;
            }
        }

        return hasTag;
    }
}
