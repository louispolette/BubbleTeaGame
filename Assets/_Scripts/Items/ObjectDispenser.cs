using UnityEngine;

public class ObjectDispenser : MonoBehaviour
{
    [Space]

    [SerializeField] private ClickableObject _dispensedObject;

    public void DispenseObject()
    {
        ClickableObject newObject = Instantiate(_dispensedObject, ClickManager.mousePosition, Quaternion.identity);
        ClickManager.Instance.StartDraggingObject(newObject);
    }
}
