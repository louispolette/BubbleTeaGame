using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : InteractableObject
{
    #region serialized members

    [Header("Dragging")]

    [SerializeField] private bool _isDraggable = false;

    [Tooltip("D�finit si l'objet sera centr� sur la position de la souris lorsqu'il est tir� ou non")]
    [SerializeField] private bool _centerOnDrag = false;

    [Tooltip("Clip audio jou� lorsque l'objet est cliqu�")]
    [SerializeField, ConditionalHide("_isDraggable", true, inverse : true)] private AudioClip _clickSFX;

    [Tooltip("Clip audio jou� lorsque l'objet est attrap�")]
    [SerializeField, ConditionalHide("_isDraggable", true)] private AudioClip _dragStartSFX;

    [Tooltip("Clip audio jou� lorsque l'objet est l�ch�")]
    [SerializeField, ConditionalHide("_isDraggable", true)] private AudioClip _dropSFX;

    [Header("Event Settings")]

    [Tooltip("D�finit si onClickedUp() peut �tre d�clench� m�me si la souris n'est pas sur la zone cliquable si l'objet a �t� cliqu� au pr�alable")]
    [SerializeField] private ClickUpMode _clickUpMode = ClickUpMode.Permissive;

    [Space]

    [Tooltip("D�finit si la souris doit rester sur la zone cliquable pour d�clencher onClickHeld")]
    [SerializeField] private bool _mustStayInAreaToHold = true;

    [Tooltip("D�finit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir d�clencher onClickHeld()")]
    [SerializeField] private ClickHoldMode _clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    [Tooltip("Permet de d�clencher onClickedDown et/ou onClickedUp lorsque la souris sort et rentre de la zone cliquable tout en gardant le clic appuy�")]
    [SerializeField] private HoldLeaveMode _extraEventCalls = HoldLeaveMode.ClickDownAndClickUp;

    #region events

    [Header("Events")]

    [Tooltip("Fonctions appel�es � chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickedDown;

    [Tooltip("Fonctions appel�es � chaque rel�chement de clic sur la zone cliquable de l'objet ou n'importe o� si l'objet a �t� cliqu� au pr�alable (en fonction de ClickUpMode)")]
    public UnityEvent onClickedUp;

    [Tooltip("Fonctions appel�es chaque frame o� le clic est maintenu sur l'objet")]
    public UnityEvent onClickHeld;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appel�es quand l'objet commence � �tre tir�")]
    public UnityEvent onDraggedStart;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appel�es chaque frame o� l'objet est tir�")]
    public UnityEvent onDragged;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appel�es lorsque l'objet est rel�ch�")]
    public UnityEvent onDropped;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appel�es lorsque l'objet est tir� et commence � se d�placer")]
    public UnityEvent onMovedStart;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appel�es chaque frame o� l'objet est tir� et se d�place")]
    public UnityEvent onMoved;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appel�es lorsque l'objet est tir� et arr�te de se d�placer")]
    public UnityEvent onMovedStop;

    #endregion

    #endregion

    #region unserialized members

    public bool IsLocked { get; private set; } = false;

    /// <summary>
    /// True si l'objet a �t� cliqu� dans sa zone cliquable lors du dernier clic, revient � false au prochain rel�chement de clic
    /// </summary>
    public bool HasBeenClickedInArea { get; private set; } = false;
    public bool IsDraggable { get => _isDraggable; }
    public bool CenterOnDrag { get => _centerOnDrag; }
    public AudioClip ClickSFX { get => _clickSFX; }
    public AudioClip DragStartSFX { get => _dragStartSFX; }
    public AudioClip DropSFX { get => _dropSFX; }
    public bool IsBeingDragged { get; set; } = false;
    public Vector3 lastPosition { get; set; }

    public Rigidbody2D Rigidbody { get; private set; }
    public ObjectHolder Holder { get; set; }

    private bool isMoving = false;
    /// <summary>
    /// True si la souris est sortie de la zone cliquable pendant qu'elle maintenait le clic sur l'objet
    /// </summary>
    private bool mouseHasExitedArea = true;

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
    /// Appel� lorsque le ClickManager d�tecte un clic de la souris
    /// </summary>
    private void ClickDown()
    {
        if (!CheckIfMouseInArea()) return;

        HasBeenClickedInArea = true;
        mouseHasExitedArea = false;

        ClickManager.Instance.ClickedObjectsThisFrame.Add(this);
    }

    /// <summary>
    /// Appel� chaque frame o� le ClickManager d�tecte que le clic de la souris est maintenu
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

            if (mouseHasExitedArea)
            {
                mouseHasExitedArea = false;
                if (CanClickDownOnEnter) onClickedDown.Invoke();
            }
        }
        else if (HasBeenClickedInArea)
        {
            if (!_mustStayInAreaToHold)
            {
                onClickHeld.Invoke();
            }

            if (!mouseHasExitedArea)
            {
                mouseHasExitedArea = true;
                if (CanClickUpOnLeave) onClickedUp.Invoke();
            }
        }
    }

    /// <summary>
    /// Appel� lorsque le ClickManager d�tecte un rel�chement du clic de la souris
    /// </summary>
    private void ClickUp()
    {
        if (HasBeenClickedInArea && (CheckIfMouseInArea() || _clickUpMode == ClickUpMode.Permissive))
        {
            onClickedUp.Invoke();
        }

        HasBeenClickedInArea = false;
        mouseHasExitedArea = true;
    }

    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
        set
        {
            if (value == true && isMoving == false)
            {
                isMoving = true;
                onMovedStart.Invoke();
            }
            else if (value == false && isMoving == true)
            {
                isMoving = false;
                onMovedStop.Invoke();
            }
        }
    }
    
    /// <summary>
    /// True si l'on peut d�clencher onClickUp en sortant de la zone cliquable en gardant le le clic appuy�
    /// </summary>
    private bool CanClickUpOnLeave
    {
        get
        {
            return _extraEventCalls == HoldLeaveMode.ClickUpOnLeave || _extraEventCalls == HoldLeaveMode.ClickDownAndClickUp
                   && !_isDraggable;
        }
    }

    /// <summary>
    /// True si l'on peut d�clencher onClickUp en sortant de la zone cliquable en gardant le le clic appuy�
    /// </summary>
    private bool CanClickDownOnEnter
    {
        get
        {
            return _extraEventCalls == HoldLeaveMode.ClickDownOnEnter || _extraEventCalls == HoldLeaveMode.ClickDownAndClickUp
                   && !_isDraggable;
        }
    }

    /// <summary>
    /// D�place l'objet vers le point donn�
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
}
