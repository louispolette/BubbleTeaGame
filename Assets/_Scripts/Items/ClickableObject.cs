using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : InteractableObject
{
    #region serialized members

    [Header("Dragging")]

    [SerializeField] private bool _isDraggable = false;

    [Header("Event Settings")]

    [Tooltip("Définit si onClickedUp() peut être déclenché même si la souris n'est pas sur la zone cliquable si l'objet a été cliqué au préalable")]
    [SerializeField] private ClickUpMode clickUpMode = ClickUpMode.Permissive;

    [Space]

    [Tooltip("Définit si la souris doit rester sur la zone cliquable pour déclencher onClickHeld")]
    [SerializeField] private bool mustStayInAreaToHold = true;

    [Tooltip("Définit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir déclencher onClickHeld()")]
    [SerializeField] private ClickHoldMode clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    [Tooltip("Permet de déclencher onClickedDown et/ou onClickedUp lorsque la souris sort et rentre de la zone cliquable tout en gardant le clic appuyé")]
    [SerializeField] private HoldLeaveMode extraEventCalls = HoldLeaveMode.ClickDownAndClickUp;

    #region events

    [Header("Events")]

    [Tooltip("Fonctions appelées à chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickedDown;

    [Tooltip("Fonctions appelées à chaque relâchement de clic sur la zone cliquable de l'objet ou n'importe où si l'objet a été cliqué au préalable (en fonction de ClickUpMode)")]
    public UnityEvent onClickedUp;

    [Tooltip("Fonctions appelées chaque frame où le clic est maintenu sur l'objet")]
    public UnityEvent onClickHeld;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appelées quand l'objet commence à être tiré")]
    public UnityEvent onDraggedStart;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré")]
    public UnityEvent onDragged;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appelées lorsque l'objet est relâché")]
    public UnityEvent onDropped;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appelées lorsque l'objet est tiré et commence à se déplacer")]
    public UnityEvent onMovedStart;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré et se déplace")]
    public UnityEvent onMoved;

    [ConditionalHide("_isDraggable", true)]
    [Tooltip("Fonctions appelées lorsque l'objet est tiré et arrête de se déplacer")]
    public UnityEvent onMovedStop;

    #endregion

    #endregion

    #region unserialized members

    /// <summary>
    /// True si l'objet a été cliqué dans sa zone cliquable lors du dernier clic, revient à false au prochain relâchement de clic
    /// </summary>
    public bool HasBeenClickedInArea { get; private set; } = false;
    public bool IsDraggable { get => _isDraggable; }
    public bool IsBeingDragged { get; set; } = false;
    public Vector3 lastPosition { get; set; }

    public Rigidbody2D Rigidbody { get; private set; }

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
        ClickManager.OnMouseClickDown += ClickDown;
        ClickManager.OnMouseClickHold += ClickHold;
        ClickManager.OnMouseClickUp += ClickUp;
    }

    private void OnDisable()
    {
        ClickManager.OnMouseClickDown -= ClickDown;
        ClickManager.OnMouseClickHold -= ClickHold;
        ClickManager.OnMouseClickUp -= ClickUp;
    }

    /// <summary>
    /// Appelé lorsque le ClickManager détecte un clic de la souris
    /// </summary>
    private void ClickDown()
    {
        if (!CheckIfMouseInArea()) return;

        HasBeenClickedInArea = true;
        mouseHasExitedArea = false;

        ClickManager.Instance.ClickedObjectsThisFrame.Add(this);
    }

    /// <summary>
    /// Appelé chaque frame où le ClickManager détecte que le clic de la souris est maintenu
    /// </summary>
    private void ClickHold()
    {
        if (clickHoldMode == ClickHoldMode.MustClickOnAreaFirst && !HasBeenClickedInArea) return;

        if (CheckIfMouseInArea())
        {
            if (clickHoldMode == ClickHoldMode.Unrestricted)
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
            if (!mustStayInAreaToHold)
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
    /// Appelé lorsque le ClickManager détecte un relâchement du clic de la souris
    /// </summary>
    private void ClickUp()
    {
        if (HasBeenClickedInArea && (CheckIfMouseInArea() || clickUpMode == ClickUpMode.Permissive))
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
    /// True si l'on peut déclencher onClickUp en sortant de la zone cliquable en gardant le le clic appuyé
    /// </summary>
    private bool CanClickUpOnLeave
    {
        get
        {
            return extraEventCalls == HoldLeaveMode.ClickUpOnLeave || extraEventCalls == HoldLeaveMode.ClickDownAndClickUp
                   && !_isDraggable;
        }
    }

    /// <summary>
    /// True si l'on peut déclencher onClickUp en sortant de la zone cliquable en gardant le le clic appuyé
    /// </summary>
    private bool CanClickDownOnEnter
    {
        get
        {
            return extraEventCalls == HoldLeaveMode.ClickDownOnEnter || extraEventCalls == HoldLeaveMode.ClickDownAndClickUp
                   && !_isDraggable;
        }
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
}
