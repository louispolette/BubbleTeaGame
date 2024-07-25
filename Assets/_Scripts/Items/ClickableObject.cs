using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    #region serialized members

    [Space]

    [SerializeField] private bool _isDraggable = false;

    [Space]

    [Tooltip("La m�thode utilis�e pour d�finir la zone cliquable de l'objet")]
    [SerializeField] ClickAreaMode clickAreaMode = ClickAreaMode.useRenderer;

    #region events

    [Header("Click Events")]

    [Space]

    [Tooltip("Fonctions appel�es � chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickedDown;

    [Space]

    [Tooltip("D�finit si onClickedUp() peut �tre d�clench� m�me si la souris n'est pas sur la zone cliquable si l'objet a �t� cliqu� au pr�alable")]
    [SerializeField] private ClickUpMode clickUpMode = ClickUpMode.Permissive;

    [Space]

    [Tooltip("Fonctions appel�es � chaque rel�chement de clic sur la zone cliquable de l'objet ou n'importe o� si l'objet a �t� cliqu� au pr�alable (en fonction de ClickUpMode)")]
    public UnityEvent onClickedUp;

    [Space]

    [Tooltip("D�finit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir d�clencher onClickHeld()")]
    [SerializeField] private ClickHoldMode clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    [Tooltip("D�finit si la souris doit rester sur la zone cliquable pour d�clencher onClickHeld")]
    [SerializeField] private bool mustStayInAreaToHold = true;

    [Tooltip("Permet de d�clencher onClickedDown et/ou onClickedUp lorsque la souris sort et rentre de la zone cliquable tout en gardant le clic appuy�")]
    [SerializeField] private HoldLeaveMode extraEventCalls = HoldLeaveMode.ClickDownAndClickUp;

    [Space]

    [Tooltip("Fonctions appel�es chaque frame o� le clic est maintenu sur l'objet")]
    public UnityEvent onClickHeld;

    [Header("Drag Events")]

    [Space]

    [ConditionalHide("_isDraggable")]
    [Tooltip("Fonctions appel�es quand l'objet commence � �tre tir�")]
    public UnityEvent onDraggedStart;

    [Space]

    [Tooltip("Fonctions appel�es chaque frame o� l'objet est tir�")]
    public UnityEvent onDragged;

    [Space]

    [Tooltip("Fonctions appel�es lorsque l'objet est rel�ch�")]
    public UnityEvent onDropped;

    [Header("Drag Move Events")]

    [Space]

    [Tooltip("Fonctions appel�es lorsque l'objet est tir� et commence � se d�placer")]
    public UnityEvent onMovedStart;

    [Space]

    [Tooltip("Fonctions appel�es chaque frame o� l'objet est tir� et se d�place")]
    public UnityEvent onMoved;

    [Space]

    [Tooltip("Fonctions appel�es lorsque l'objet est tir� et arr�te de se d�placer")]
    public UnityEvent onMovedStop;

    #endregion

    [Header("Debug")]

    [SerializeField] private bool _showClickableArea = false;

    #endregion

    #region unserialized members

    /// <summary>
    /// True si l'objet a �t� cliqu� dans sa zone cliquable lors du dernier clic, revient � false au prochain rel�chement de clic
    /// </summary>
    public bool HasBeenClickedInArea { get; private set; } = false;
    public bool IsDraggable { get => _isDraggable; }
    public bool IsBeingDragged { get; set; } = false;
    public Vector3 lastPosition { get; set; }

    public Renderer _renderer { get; private set; }
    public Collider2D _collider { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }

    private bool isMoving = false;
    /// <summary>
    /// True si la souris est sortie de la zone cliquable pendant qu'elle maintenait le clic sur l'objet
    /// </summary>
    private bool mouseHasExitedArea = true;

    #endregion

    #region enums
    public enum ClickAreaMode
    {
        useRenderer,
        useCollider
    }

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
    public enum HoldLeaveMode
    {
        ClickDownAndClickUp,
        ClickUpOnLeave,
        ClickDownOnEnter,
        NoAdditionnalEvents
    }

    #endregion

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _collider = GetComponentInChildren<Collider2D>();
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
    /// Appel� lorsque le ClickManager d�tecte un clic de la souris
    /// </summary>
    private void ClickDown()
    {
        if (!CheckIfMouseInArea()) return;

        HasBeenClickedInArea = true;
        mouseHasExitedArea = false;

        ClickManager.Instance.ClickedObjectsThisFrame.Add(this);

/*        if (_isDraggable)
        {
            ClickManager.Instance.StartDraggingObject(this);
        }

        onClickedDown.Invoke();*/
    }

    /// <summary>
    /// Appel� chaque frame o� le ClickManager d�tecte que le clic de la souris est maintenu
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
    /// Appel� lorsque le ClickManager d�tecte un rel�chement du clic de la souris
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

    /// <summary>
    /// V�rifie si la souris est dans la zone cliquable de l'objet
    /// </summary>
    private bool CheckIfMouseInArea()
    {
        Bounds bounds = GetBounds();

        return ClickManager.IsMouseInBounds(bounds);
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
            return extraEventCalls == HoldLeaveMode.ClickUpOnLeave || extraEventCalls == HoldLeaveMode.ClickDownAndClickUp
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
            return extraEventCalls == HoldLeaveMode.ClickDownOnEnter || extraEventCalls == HoldLeaveMode.ClickDownAndClickUp
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

    /// <summary>
    /// Retourne les limites du renderer ou du collider en fonction de ClickAreaMode
    /// </summary>
    /// <returns></returns>
    private Bounds GetBounds()
    {
        switch (clickAreaMode)
        {
            case ClickAreaMode.useRenderer:
                return _renderer.bounds;
            case ClickAreaMode.useCollider:
                return _collider.bounds;
            default:
                return new Bounds();
        }
    }

    #region gizmos

    private void OnDrawGizmosSelected()
    {
        if (_showClickableArea)
        {
            Gizmos.color = Color.white;

            Bounds bounds = new Bounds();

            switch (clickAreaMode)
            {
                case ClickAreaMode.useRenderer:
                    Renderer renderer = GetComponentInChildren<Renderer>();
                    if (!renderer) break;
                    bounds = renderer.bounds;
                    break;
                case ClickAreaMode.useCollider:
                    Collider2D collider = GetComponentInChildren<Collider2D>();
                    if (!collider) break;
                    bounds = collider.bounds;
                    break;
            }

            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }
    }

    #endregion
}
