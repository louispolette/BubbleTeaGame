using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Space]

    [SerializeField] private SpeechBubble _speechBubbleInstance;
    [SerializeField] private Customer _customerPrefab;

    [Space]

    [SerializeField] private Vector2 _customerSpawnpoint;
    [SerializeField] private float _customerInterval = 3f;

    public static OrderManager Instance;

    private int _orderNumber = 0;
    private bool _gameStarted = false;
    private bool _sendCustomers = false;
    private float _customerLeftTime;

    private Customer _currentCustomer;
    private CustomerState _currentCustomerState = CustomerState.None;
    private Coroutine _orderCoroutine;

    public CustomerState CurrentCustomerState => _currentCustomerState;

    public enum CustomerState
    {
        None,
        Incoming,
        Ordering,
        Waiting,
        Leaving,
    }

    private void OnEnable()
    {
        Customer.OnCustomerArrived += StartOrder;
    }

    private void OnDisable()
    {
        Customer.OnCustomerArrived -= StartOrder;
    }

    private void Awake()
    {
        #region singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        #endregion
    }

    private void Update()
    {
        if (_currentCustomer != null)
        {
            if (_currentCustomerState == CustomerState.Waiting)
            {
                DecreaseCustomerPatience();
            }
        }
        else if (_sendCustomers)
        {
            WaitForNextCustomer();
        }
    }

    public void StartGame()
    {
        _gameStarted = true;
        _sendCustomers = true;
        CustomerNew();
    }

    private void StartOrder()
    {
        if (_currentCustomerState != CustomerState.Incoming) return;

        _orderNumber++;
        _currentCustomerState = CustomerState.Ordering;

        if (_orderCoroutine != null)
        {
            StopCoroutine(_orderCoroutine);
        }

        _orderCoroutine = StartCoroutine(OrderCoroutine());
    }

    private void StartWaiting()
    {
        _currentCustomerState = CustomerState.Waiting;
    }

    private void WaitForNextCustomer()
    {
        if (Time.time - _customerLeftTime >= _customerInterval)
        {
            CustomerNew();
        }
    }

    private void DecreaseCustomerPatience()
    {
        _currentCustomer.Patience -= Time.deltaTime;

        if (_currentCustomer.Patience <= 0)
        {
            OrderFail();
        }
    }

    private CustomerData ChooseCustomerData()
    {
        return null;
    }

    public void CheckOrder()
    {
        // Check if the given order is correct and call either OrderSuccess() or OrderFail()
    }

    private void OrderSuccess()
    {
        CustomerLeave();
    }

    private void OrderFail()
    {
        CustomerLeave();
    }

    private void CustomerNew()
    {
        _currentCustomer = Instantiate(_customerPrefab, _customerSpawnpoint, Quaternion.identity);
        var customerData = ChooseCustomerData();
        _currentCustomer.ApplyData(customerData);
        _currentCustomer.Enter();

        _currentCustomerState = CustomerState.Incoming;
    }

    private void CustomerLeave()
    {
        _currentCustomer.Leave();
        _currentCustomer = null;
        _currentCustomerState = CustomerState.Leaving;
        _customerLeftTime = Time.time;
    }

    public void NoCustomers()
    {
        _currentCustomer = null;
        _currentCustomerState = CustomerState.None;
    }

    private IEnumerator OrderCoroutine()
    {
        //Show the order

        yield return null;

        StartWaiting();
    }
}
