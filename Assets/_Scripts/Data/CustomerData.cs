using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "Data/Customers/Customer Data")]
[System.Serializable]
public class CustomerData : ScriptableObject
{
    public float patience = 60f;
    public float movementSpeed = 5f;
    public CustomerAnimationSet animationSet;
}
