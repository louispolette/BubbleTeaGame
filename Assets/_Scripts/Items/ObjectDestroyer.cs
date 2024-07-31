using UnityEngine;
using UnityEngine.Events;

public class ObjectDestroyer : MonoBehaviour
{
    [SerializeField] public UnityEvent OnDestroyObject;

    public void DestroyObject(ClickableObject obj)
    {
        if (obj.gameObject == gameObject) return;
        OnDestroyObject.Invoke();
        Destroy(obj.gameObject);
    }
}
