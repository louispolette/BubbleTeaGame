using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    [Space]

    [SerializeField] private Transform _anchor;

    [Space]

    [SerializeField] private bool _lockGivenObjects = false;

    public ClickableObject ContainedObject {  get; private set; }

    public void HoldObject(ClickableObject obj)
    {
        if (ContainedObject != null) return;

        ContainedObject = obj;
        ContainedObject.Holder = this;

        ContainedObject.transform.position = _anchor.position;

        if (_lockGivenObjects)
        {
            ContainedObject.Lock();
        }
    }

    public void ReleaseContent()
    {
        ContainedObject.Holder = null;

        if (ContainedObject.IsLocked)
        {
            ContainedObject.Unlock();
        }

        ContainedObject = null;
    }
}
