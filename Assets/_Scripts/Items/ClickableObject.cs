using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : InteractableObject
{
    #region serialized members

    [SerializeField] private bool _isDraggable = false;

    [Tooltip("L'objet retournera à sa position initiale lorsqu'il est relâché")]
    [SerializeField] private bool _returnToRestingPosition = false;

    [SerializeField] private float _returnDuration = 0.75f;

    [SerializeField] private AnimationCurve _returnCurve;

    [Tooltip("Définit si l'objet sera centré sur la position de la souris lorsqu'il est tiré ou non")]
    [SerializeField] private bool _centerOnDrag = false;

    [Tooltip("Définit si onClickedUp() peut être déclenché même si la souris n'est pas sur la zone cliquable si l'objet a été cliqué au préalable")]
    [SerializeField] private ClickUpMode _clickUpMode = ClickUpMode.Permissive;

    [Tooltip("Définit si la souris doit rester sur la zone cliquable pour déclencher onClickHeld")]
    [SerializeField] private bool _mustStayInAreaToHold = true;

    [Tooltip("Définit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir déclencher onClickHeld()")]
    [SerializeField] private ClickHoldMode _clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    [Tooltip("Permet de déclencher onClickedDown et/ou onClickedUp lorsque la souris sort et rentre de la zone cliquable tout en gardant le clic appuyé")]
    [SerializeField] private HoldLeaveMode _extraEventCalls = HoldLeaveMode.ClickDownAndClickUp;

    #region events

    [Tooltip("Fonctions appelées à chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickedDown;

    [Tooltip("Fonctions appelées à chaque relâchement de clic sur la zone cliquable de l'objet ou n'importe où si l'objet a été cliqué au préalable (en fonction de ClickUpMode)")]
    public UnityEvent onClickedUp;

    [Tooltip("Fonctions appelées chaque frame où le clic est maintenu sur l'objet")]
    public UnityEvent onClickHeld;

    [Tooltip("Fonctions appelées quand l'objet commence à être tiré")]
    public UnityEvent onDraggedStart;

    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré")]
    public UnityEvent onDragged;

    [Tooltip("Fonctions appelées lorsque l'objet est relâché")]
    public UnityEvent onDropped;

    [Tooltip("Fonctions appelées lorsque l'objet est tiré et commence à se déplacer")]
    public UnityEvent onMovedStart;

    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré et se déplace")]
    public UnityEvent onMoved;

    [Tooltip("Fonctions appelées lorsque l'objet est tiré et arrête de se déplacer")]
    public UnityEvent onMovedStop;

    #endregion

    [Tooltip("Clip audio joué lorsque l'objet est cliqué")]
    [SerializeField] private AudioClip _clickSFX;

    [Tooltip("Clip audio joué lorsque l'objet est attrapé")]
    [SerializeField] private AudioClip _dragStartSFX;

    [Tooltip("Clip audio joué lorsque l'objet est lâché")]
    [SerializeField] private AudioClip _dropSFX;

    #endregion

    #region unserialized members

    public bool IsLocked { get; private set; } = false;

    /// <summary>
    /// True si l'objet a été cliqué dans sa zone cliquable lors du dernier clic, revient à false au prochain relâchement de clic
    /// </summary>
    public bool HasBeenClickedInArea { get; private set; } = false;
    public bool IsDraggable { get => _isDraggable; }
    public bool CenterOnDrag { get => _centerOnDrag; }
    public AudioClip ClickSFX { get => _clickSFX; }
    public AudioClip DragStartSFX { get => _dragStartSFX; }
    public AudioClip DropSFX { get => _dropSFX; }
    public bool IsBeingDragged { get; set; } = false;
    public Vector3 lastPosition { get; set; }
    public bool DisableReturning {  get; set; }

    public Rigidbody2D Rigidbody { get; private set; }
    public ObjectHolder Holder { get; set; }

    private bool _isMoving = false;
    /// <summary>
    /// True si la souris est sortie de la zone cliquable pendant qu'elle maintenait le clic sur l'objet
    /// </summary>
    private bool _mouseHasExitedArea = true;

    private Vector3 _restingPosition;
    private Coroutine _returnCoroutine;

    #endregion

    #region enums

    public enum ClickUpMode
    {
        Permissive,
        MustBeOnArea
    }

    public enum ClickHoldMode
    {
        MustClickOnAreaFirst,
        Unrestricted
    }
    //TRY TO USE FLAGS
    public enum HoldLeaveMode
    {
        ClickDownAndClickUp,
        ClickUpOnLeave,
        ClickDownOnEnter,
        NoAdditionnalEvents
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();
        Rigidbody = GetComponent<Rigidbody2D>();
        _restingPosition = transform.position;
    }

    private void OnEnable()
    {
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    #region events

    private void SubscribeEvents()
    {
        ClickManager.OnMouseClickDown += ClickDown;
        ClickManager.OnMouseClickHold += ClickHold;
        ClickManager.OnMouseClickUp += ClickUp;
    }

    private void UnsubscribeEvents()
    {
        ClickManager.OnMouseClickDown -= ClickDown;
        ClickManager.OnMouseClickHold -= ClickHold;
        ClickManager.OnMouseClickUp -= ClickUp;
    }

    #endregion

    /// <summary>
    /// Appelé lorsque le ClickManager détecte un clic de la souris
    /// </summary>
    private void ClickDown()
    {
        if (!CheckIfMouseInArea()) return;

        HasBeenClickedInArea = true;
        _mouseHasExitedArea = false;

        ClickManager.Instance.ClickedObjectsThisFrame.Add(this);
    }

    /// <summary>
    /// Appelé chaque frame où le ClickManager détecte que le clic de la souris est maintenu
    /// </summary>
    private void ClickHold()
    {
        if (_clickHoldMode == ClickHoldMode.MustClickOnAreaFirst && !HasBeenClickedInArea) return;

        if (CheckIfMouseInArea())
        {
            if (_clickHoldMode == ClickHoldMode.Unrestricted)
            {
                HasBeenClickedInArea = true;
            }

            onClickHeld.Invoke();

            if (_mouseHasExitedArea)
            {
                _mouseHasExitedArea = false;
                if (CanClickDownOnEnter) onClickedDown.Invoke();
            }
        }
        else if (HasBeenClickedInArea)
        {
            if (!_mustStayInAreaToHold)
            {
                onClickHeld.Invoke();
            }

            if (!_mouseHasExitedArea)
            {
                _mouseHasExitedArea = true;
                if (CanClickUpOnLeave) onClickedUp.Invoke();
            }
        }
    }

    /// <summary>
    /// Appelé lorsque le ClickManager détecte un relâchement du clic de la souris
    /// </summary>
    private void ClickUp()
    {
        if (HasBeenClickedInArea && (CheckIfMouseInArea() || _clickUpMode == ClickUpMode.Permissive))
        {
            onClickedUp.Invoke();

            if (_returnToRestingPosition)
            {
                ReturnToRestingPosition();
            }
        }

        HasBeenClickedInArea = false;
        _mouseHasExitedArea = true;
    }

    public bool IsMoving
    {
        get
        {
            return _isMoving;
        }
        set
        {
            if (value == true && _isMoving == false)
            {
                _isMoving = true;
                onMovedStart.Invoke();
            }
            else if (value == false && _isMoving == true)
            {
                _isMoving = false;
                onMovedStop.Invoke();
            }
        }
    }
    
    /// <summary>
    /// True si l'on peut déclencher onClickUp en sortant de la zone cliquable en gardant le le clic appuyé
    /// </summary>
    private bool CanClickUpOnLeave => _extraEventCalls == HoldLeaveMode.ClickUpOnLeave
                                   || _extraEventCalls == HoldLeaveMode.ClickDownAndClickUp && !_isDraggable;

    /// <summary>
    /// True si l'on peut déclencher onClickUp en sortant de la zone cliquable en gardant le le clic appuyé
    /// </summary>
    private bool CanClickDownOnEnter => _extraEventCalls == HoldLeaveMode.ClickDownOnEnter 
                                     || _extraEventCalls == HoldLeaveMode.ClickDownAndClickUp && !_isDraggable;

    public void ReturnToRestingPosition(float durationMultiplier = 1f)
    {
        if (_returnCoroutine != null)
        {
            StopCoroutine(_returnCoroutine);
        }

        StartCoroutine(ReturnCoroutine(_returnDuration * durationMultiplier));
    }

    /// <summary>
    /// Déplace l'objet vers le point donné
    /// </summary>
    /// <param name="targetPosition"></param>
    public void Drag(Vector3 targetPosition)
    {
        if (Rigidbody) // Utilise Rigidbody si possible
        {
            Rigidbody.MovePosition(targetPosition);
        }
        else
        {
            transform.position = targetPosition;
        }
    }

    public void PlayClickSFX()
    {
        AudioManager.Instance.PlaySound(_clickSFX, 1f, 1f, 0.05f);
    }

    public void PlayDragStartSFX()
    {
        AudioManager.Instance.PlaySound(_dragStartSFX, 1f, 1f, 0.05f);
    }

    public void PlayDropSFX()
    {
        AudioManager.Instance.PlaySound(_dragStartSFX, 1f, 1f, 0.05f);
    }

    public void Lock()
    {
        if (IsLocked) return;

        IsLocked = true;
        UnsubscribeEvents();
    }

    public void Unlock()
    {
        if (!IsLocked) return;

        IsLocked = false;
        SubscribeEvents();
    }

    public void LeaveHolder()
    {
        Holder?.ReleaseContent();
    }

    private IEnumerator ReturnCoroutine(float duration)
    {
        Lock();
        Vector3 initialPos = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float lerp = _returnCurve.Evaluate(elapsedTime / duration);
            transform.position = Vector3.Lerp(initialPos, _restingPosition, lerp);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        transform.position = _restingPosition;
        Unlock();
    }
}
