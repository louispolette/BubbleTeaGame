using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Space]

    [SerializeField] private SpeechBubble _speechBubble;
    [SerializeField] private Customer _customer;

    [Space]

    [SerializeField] private float _customerInterval = 3f;

    [Header("PLACEHOLDERS")]

    [SerializeField] private float _orderDuration = 2.5f;

    [Space]

    [SerializeField] private OrderResult _orderResult = OrderResult.Success;

    private enum OrderResult { Success, Failure }

    public static OrderManager Instance;

    private bool _shopOpen = false;
    private float _customerLeftTime;

    private Coroutine _orderCoroutine;

    private Queue<CustomerData> _customerQueue = new Queue<CustomerData>();

    #region event subscriptions
    private void OnEnable()
    {
        Customer.OnCustomerArrived += StartOrder;
        Customer.OnCustomerGone += CustomerGone;
    }

    private void OnDisable()
    {
        Customer.OnCustomerArrived -= StartOrder;
        Customer.OnCustomerGone -= CustomerGone;
    }
    #endregion

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

        BuildCustomerQueue();
    }

    private void Start()
    {
        StartGame();
    }

    private void Update()
    {
        if (_customer.State != CustomerState.Absent)
        {
            if (_customer.State == CustomerState.Waiting)
            {
                DecreaseCustomerPatience();
            }
        }
        else if (_shopOpen)
        {
            if (_customerQueue.Count <= 0)
            {
                EndGame();
            }

            WaitForNextCustomer();
        }
    }

    private void BuildCustomerQueue()
    {
        _customerQueue.Clear();

        List<CustomerData> customerDatabase = Resources.LoadAll<CustomerData>("Customers").ToList();

        while (customerDatabase.Count > 0)
        {
            int randomIndex = Random.Range(0, customerDatabase.Count);

            _customerQueue.Enqueue(customerDatabase[randomIndex]);

            customerDatabase.RemoveAt(randomIndex);
        }
    }

    public void StartGame()
    {
        _shopOpen = true;
        CustomerNew();
    }

    private void EndGame()
    {
        _shopOpen = false;
    }

    private void StartOrder()
    {
        if (_customer.State != CustomerState.Incoming) return;
        
        _customer.State = CustomerState.Ordering;

        if (_orderCoroutine != null)
        {
            StopCoroutine(_orderCoroutine);
        }

        _orderCoroutine = StartCoroutine(OrderCoroutine());
    }

    private void StartWaiting()
    {
        _customer.State = CustomerState.Waiting;
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
        _customer.DoPatienceTick();

        if (_customer.Patience <= 0)
        {
            OrderFail();
        }
    }

    public void CheckOrder()
    {
        if (_orderResult == OrderResult.Success)
        {
            OrderSuccess();
        }
        else
        {
            OrderFail();
        }
    }

    [ContextMenu("End Order/Success")]
    private void OrderSuccess()
    {
        CustomerLeave();
    }

    [ContextMenu("End Order/Fail")]
    private void OrderFail()
    {
        CustomerLeave();
    }

    private void CustomerNew()
    {
        _customer.TeleportToSpawnpoint();

        var customerData = _customerQueue.Dequeue();
        _customer.Init(customerData);

        _customer.State = CustomerState.Incoming;
        _customer.EnterShop();
    }

    private void CustomerLeave()
    {
        _customerLeftTime = Time.time;

        _customer.State = CustomerState.Leaving;
        _customer.LeaveShop();
    }

    private void CustomerGone()
    {
        _customer.State = CustomerState.Absent;
    }

    private IEnumerator OrderCoroutine()
    {
        float timeRemaining = _orderDuration;

        while (timeRemaining > 0)
        {
            // Do stuff

            timeRemaining -= Time.deltaTime;
        }

        yield return null;

        StartWaiting();
    }
}