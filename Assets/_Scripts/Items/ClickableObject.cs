using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    #region serialized members

    [Space]

    [SerializeField] private bool _isDraggable = false;

    [Space]

    [Tooltip("La méthode utilisée pour définir la zone cliquable de l'objet")]
    [SerializeField] ClickAreaMode clickAreaMode = ClickAreaMode.useRenderer;

    #region events

    [Header("Click Events")]

    [Space]

    [Tooltip("Fonctions appelées à chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickedDown;

    [Space]

    [Tooltip("Définit si onClickedUp() peut être déclenché même si la souris n'est pas sur la zone cliquable si l'objet a été cliqué au préalable")]
    [SerializeField] private ClickUpMode clickUpMode = ClickUpMode.Permissive;

    [Space]

    [Tooltip("Fonctions appelées à chaque relâchement de clic sur la zone cliquable de l'objet ou n'importe où si l'objet a été cliqué au préalable (en fonction de ClickUpMode)")]
    public UnityEvent onClickedUp;

    [Space]

    [Tooltip("Définit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir déclencher onClickHeld()")]
    [SerializeField] private ClickHoldMode clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    [Tooltip("Définit si la souris doit rester sur la zone cliquable pour déclencher onClickHeld")]
    [SerializeField] private bool mustStayInAreaToHold = true;

    [Tooltip("Permet de déclencher onClickedDown et/ou onClickedUp lorsque la souris sort et rentre de la zone cliquable tout en gardant le clic appuyé")]
    [SerializeField] private HoldLeaveMode extraEventCalls = HoldLeaveMode.ClickDownAndClickUp;

    [Space]

    [Tooltip("Fonctions appelées chaque frame où le clic est maintenu sur l'objet")]
    public UnityEvent onClickHeld;

    [Header("Drag Events")]

    [Space]

    [ConditionalHide("_isDraggable")]
    [Tooltip("Fonctions appelées quand l'objet commence à être tiré")]
    public UnityEvent onDraggedStart;

    [Space]

    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré")]
    public UnityEvent onDragged;

    [Space]

    [Tooltip("Fonctions appelées lorsque l'objet est relâché")]
    public UnityEvent onDropped;

    [Header("Drag Move Events")]

    [Space]

    [Tooltip("Fonctions appelées lorsque l'objet est tiré et commence à se déplacer")]
    public UnityEvent onMovedStart;

    [Space]

    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré et se déplace")]
    public UnityEvent onMoved;

    [Space]

    [Tooltip("Fonctions appelées lorsque l'objet est tiré et arrête de se déplacer")]
    public UnityEvent onMovedStop;

    #endregion

    [Header("Debug")]

    [SerializeField] private bool _showClickableArea = false;

    #endregion

    #region unserialized members

    /// <summary>
    /// True si l'objet a été cliqué dans sa zone cliquable lors du dernier clic, revient à false au prochain relâchement de clic
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
    /// Appelé lorsque le ClickManager détecte un clic de la souris
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

    /// <summary>
    /// Vérifie si la souris est dans la zone cliquable de l'objet
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
