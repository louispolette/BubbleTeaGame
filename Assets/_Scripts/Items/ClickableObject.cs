using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    [Space]

    [SerializeField] private bool _isDraggable = false;

    [Space]

    [Tooltip("La méthode utilisée pour définir la zone cliquable de l'objet")]
    [SerializeField] ClickAreaMode clickAreaMode = ClickAreaMode.useRenderer;

    public enum ClickAreaMode
    {
        useRenderer,
        useCollider
    }

    [Header("Click Events")]

    [Space]

    [Tooltip("Fonctions appelées à chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickDown;

    [Space]

    [Tooltip("Définit si onClickUp() peut être déclenché même si la souris n'est pas sur la zone cliquable si l'objet a été cliqué au préalable")]
    [SerializeField] private ClickUpMode clickUpMode = ClickUpMode.Permissive;

    public enum ClickUpMode
    {
        Permissive,
        MustBeOnArea
    }

    [Space]

    [Tooltip("Fonctions appelées à chaque relâchement de clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickUp;

    [Space]

    [Tooltip("Définit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir déclencher onClickHold()")]
    [SerializeField] private ClickHoldMode clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    public enum ClickHoldMode
    {
        MustClickOnAreaFirst,
        Unrestricted
    }

    [Space]

    [Tooltip("Fonctions appelées chaque frame où le clic est maintenu sur l'objet")]
    public UnityEvent onClickHold;

    [Header("Drag Events")]

    [Space]

    [Tooltip("Fonctions appelées quand l'objet commence à être tiré")]
    public UnityEvent onDragStart;

    [Space]

    [Tooltip("Fonctions appelées chaque frame où l'objet est tiré")]
    public UnityEvent onDrag;

    [Space]

    [Tooltip("Fonctions appelées lorsque l'objet est relâché")]
    public UnityEvent onDrop;

    [Header("Debug")]

    [SerializeField] private bool _showClickableArea = false;

    public bool IsBeingDragged { get; set; } = false;

    private Renderer _renderer;
    private Collider2D _collider;

    /// <summary>
    /// True si l'objet a été cliqué dans sa zone cliquable lors du dernier clic, revient à false au prochain relâchement de clic
    /// </summary>
    public bool HasBeenClickedInArea { get; private set; } = false;

    public Rigidbody2D Rigidbody { get; private set; }

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

        if (_isDraggable)
        {
            ClickManager.Instance.StartDraggingObject(this);
        }

        onClickDown.Invoke();
    }

    /// <summary>
    /// Appelé chaque frame où le ClickManager détecte que le clic de la souris est maintenu
    /// </summary>
    private void ClickHold()
    {
        if (clickHoldMode == ClickHoldMode.MustClickOnAreaFirst && !HasBeenClickedInArea) return;

        if (!CheckIfMouseInArea()) return;

        onClickHold.Invoke();
    }

    /// <summary>
    /// Appelé lorsque le ClickManager détecte un relâchement du clic de la souris
    /// </summary>
    private void ClickUp()
    {
        if (!(CheckIfMouseInArea() || (clickUpMode == ClickUpMode.Permissive && HasBeenClickedInArea))) return;

        HasBeenClickedInArea = false;

        onClickUp.Invoke();
    }

    /// <summary>
    /// Vérifie si la souris est dans la zone cliquable de l'objet
    /// </summary>
    private bool CheckIfMouseInArea()
    {
        Bounds bounds = GetBounds();

        return ClickManager.IsMouseInBounds(bounds);
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
