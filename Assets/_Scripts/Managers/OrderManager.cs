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

    [Header("Audio")]

    [SerializeField] private AudioClip _correctOrderSFX;
    [SerializeField] private AudioClip _wrongOrderSFX;

    [Header("PLACEHOLDERS")]

    [SerializeField] private float _orderDuration = 2.5f;
    [SerializeField] private float _reactionDuration = 2.5f;

    [Space]

    [SerializeField] private OrderResult _orderResult = OrderResult.Success;

    private enum OrderResult { Success, Failure }

    public static OrderManager Instance;

    private bool _shopOpen = false;
    private float _customerLeftTime;

    private Coroutine _orderCoroutine;
    private Coroutine _reactionCoroutine;

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

        _speechBubble.Show();

        if (_orderCoroutine != null)
        {
            StopCoroutine(_orderCoroutine);
        }

        _orderCoroutine = StartCoroutine(OrderCoroutine(_orderDuration));
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
            EndOrder(false, true);
        }
    }

    public void CheckOrder() // Actually implement this
    {
        if (_orderResult == OrderResult.Success)
        {
            EndOrder(true);
        }
        else
        {
            EndOrder(false);
        }
    }

    private void EndOrder(bool isSuccess, bool skipReaction = false)
    {
        if (isSuccess)
        {
            AudioManager.Instance.PlaySound(_correctOrderSFX, 1f);
        }
        else
        {
            AudioManager.Instance.PlaySound(_wrongOrderSFX, 1f);
        }

        if (skipReaction)
        {
            CustomerLeave();
            return;
        }

        CustomerReaction();
    }

    private void CustomerReaction()
    {
        _customer.State = CustomerState.Reacting;

        _customer.ReactToOrder();

        if (_reactionCoroutine != null)
        {
            StopCoroutine(_reactionCoroutine);
        }

        _reactionCoroutine = StartCoroutine(ReactionCoroutine(_reactionDuration));
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

        _speechBubble.Hide();
        _customer.State = CustomerState.Leaving;
        _customer.LeaveShop();
    }

    private void CustomerGone()
    {
        _customer.State = CustomerState.Absent;
    }

    private IEnumerator OrderCoroutine(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0)
        {
            // Do stuff overtime

            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        StartWaiting();
    }

    private IEnumerator ReactionCoroutine(float duration)
    {
        float timeRemaining = duration;

        while (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        CustomerLeave();
    }

#if UNITY_EDITOR
    [ContextMenu("End Order/Success")]
    private void ForceOrderSuccess()
    {
        EndOrder(true);
    }

    [ContextMenu("End Order/Fail")]
    private void ForceOrderFail()
    {
        EndOrder(false);
    }

    [ContextMenu("End Order (No Reaction)/Success")]
    private void ForceOrderSuccessNoReaction()
    {
        EndOrder(true, true);
    }

    [ContextMenu("End Order (No Reaction)/Fail")]
    private void ForceOrderFailNoReaction()
    {
        EndOrder(false, true);
    }
#endif
}