using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    public void DestroyObject(ClickableObject obj)
    {
        if (obj.gameObject == gameObject) return;
        Destroy(obj.gameObject);
    }
}
