using System.Collections.Generic;
using UnityEngine;

public class ObjectHolder : MonoBehaviour
{
    [SerializeField] private Transform anchor;

    public void SnapObject(ClickableObject obj)
    {
        obj.transform.position = anchor.position;
    }
}
