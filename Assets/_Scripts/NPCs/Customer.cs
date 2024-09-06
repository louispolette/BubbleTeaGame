using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public float Patience { get; set; } = 10f;

    public delegate void CustomerArrived();
    public static event CustomerArrived OnCustomerArrived;

    public void ApplyData(CustomerData data)
    {

    }

    public void Enter()
    {

    }

    private void Arrive()
    {
        OnCustomerArrived?.Invoke();
    }

    public void Leave()
    {

    }

    private void OnDestroy()
    {
        if (OrderManager.Instance.CurrentCustomerState == OrderManager.CustomerState.Leaving)
        {
            OrderManager.Instance.NoCustomers();
        }
    }
}
