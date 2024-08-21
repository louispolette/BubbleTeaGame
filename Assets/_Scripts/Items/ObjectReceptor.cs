using UnityEngine;
using UnityEngine.Events;

public class ObjectReceptor : InteractableObject
{
    [Tooltip("Action à effectuer lorsqu'un objet est reçu")]
    public UnityEvent<ClickableObject> onObjectReceived;

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
        if (!CheckIfMouseInArea() || !ClickManager.draggedObject) return;

        onObjectReceived.Invoke(ClickManager.draggedObject);
    }
}
