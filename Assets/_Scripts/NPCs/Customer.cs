using System.Collections;
using UnityEngine;

public enum CustomerState
{
    Absent,
    Incoming,
    Ordering,
    Waiting,
    Leaving,
}

public class Customer : MonoBehaviour
{
    [Space]

    [SerializeField] private Transform _spawnpoint;
    [SerializeField] private Transform _waitPosition;

    [Space]

    [SerializeField] private SpriteRenderer _customerRenderer;

    private float _basePatience = 60f;
    private float _patience;
    private float _movementSpeed = 1f;

    public delegate void CustomerArrived();
    public static event CustomerArrived OnCustomerArrived;

    public delegate void CustomerGone();
    public static event CustomerGone OnCustomerGone;

    private ProgressionBar _patienceBar;

    private Coroutine _moveCoroutine;

    public CustomerState State { get; set; }

    private void Awake()
    {
        _patienceBar = GetComponentInChildren<ProgressionBar>();
    }

    public float Patience
    {
        get
        {
            return _patience;
        }

        private set
        {
            _patience = value;
            _patienceBar.FillLevel = _patience / _basePatience;
        }
    }

    public void Init(CustomerData data = null)
    {
        if (data != null)
        {
            ApplyData(data);
        }

        Patience = _basePatience;
    }

    public void ApplyData(CustomerData data)
    {
        _basePatience = data.patience;
        _movementSpeed = data.movementSpeed;
        _customerRenderer.color = data.color;
    }

    [ContextMenu("Move Customer/Spawnpoint")]
    public void TeleportToSpawnpoint()
    {
        transform.position = _spawnpoint.position;
    }

    [ContextMenu("Move Customer/Wait Position")]
    public void TeleportToWaitPosition()
    {
        transform.position = _waitPosition.position;
    }

    public void EnterShop()
    {
        Move(_waitPosition.position, CustomerMoveContext.Entering);
    }

    public void LeaveShop()
    {
        Move(_spawnpoint.position, CustomerMoveContext.Leaving);
    }

    private void Arrive()
    {
        OnCustomerArrived?.Invoke();
    }

    private void Move(Vector3 destination, CustomerMoveContext context)
    {
        if (_moveCoroutine != null)
        {
            StopCoroutine(_moveCoroutine);
        }

        _moveCoroutine = StartCoroutine(MoveCoroutine(destination, context));
    }

    private IEnumerator MoveCoroutine(Vector3 destination, CustomerMoveContext context)
    {
        while (transform.position != destination)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, _movementSpeed * Time.deltaTime);
            yield return null;
        }

        switch (context)
        {
            case CustomerMoveContext.Entering:
                Arrive();
                break;
            case CustomerMoveContext.Leaving:
                Leave();
                break;
        }
    }

    private enum CustomerMoveContext { Entering, Leaving }

    private void Leave()
    {
        OnCustomerGone?.Invoke();
    }

    public void DoPatienceTick()
    {
        Patience -= Time.deltaTime;
    }
}
