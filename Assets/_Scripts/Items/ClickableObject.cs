using UnityEngine;
using UnityEngine.Events;

public class ClickableObject : MonoBehaviour
{
    [Space]

    [SerializeField] private bool _isDraggable = false;

    [Space]

    [Tooltip("La m�thode utilis�e pour d�finir la zone cliquable de l'objet")]
    [SerializeField] ClickAreaMode clickAreaMode = ClickAreaMode.useRenderer;

    public enum ClickAreaMode
    {
        useRenderer,
        useCollider
    }

    [Header("Click Events")]

    [Space]

    [Tooltip("Fonctions appel�es � chaque clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickDown;

    [Space]

    [Tooltip("D�finit si onClickUp() peut �tre d�clench� m�me si la souris n'est pas sur la zone cliquable si l'objet a �t� cliqu� au pr�alable")]
    [SerializeField] private ClickUpMode clickUpMode = ClickUpMode.Permissive;

    public enum ClickUpMode
    {
        Permissive,
        MustBeOnArea
    }

    [Space]

    [Tooltip("Fonctions appel�es � chaque rel�chement de clic sur la zone cliquable de l'objet")]
    public UnityEvent onClickUp;

    [Space]

    [Tooltip("D�finit si le joueur doit d'abord cliquer sur la zone cliquable ou non pour pouvoir d�clencher onClickHold()")]
    [SerializeField] private ClickHoldMode clickHoldMode = ClickHoldMode.MustClickOnAreaFirst;

    public enum ClickHoldMode
    {
        MustClickOnAreaFirst,
        Unrestricted
    }

    [Space]

    [Tooltip("Fonctions appel�es chaque frame o� le clic est maintenu sur l'objet")]
    public UnityEvent onClickHold;

    [Header("Drag Events")]

    [Space]

    [Tooltip("Fonctions appel�es quand l'objet commence � �tre tir�")]
    public UnityEvent onDragStart;

    [Space]

    [Tooltip("Fonctions appel�es chaque frame o� l'objet est tir�")]
    public UnityEvent onDrag;

    [Space]

    [Tooltip("Fonctions appel�es lorsque l'objet est rel�ch�")]
    public UnityEvent onDrop;

    [Header("Debug")]

    [SerializeField] private bool _showClickableArea = false;

    public bool IsBeingDragged { get; set; } = false;

    private Renderer _renderer;
    private Collider2D _collider;

    /// <summary>
    /// True si l'objet a �t� cliqu� dans sa zone cliquable lors du dernier clic, revient � false au prochain rel�chement de clic
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
    /// Appel� lorsque le ClickManager d�tecte un clic de la souris
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
    /// Appel� chaque frame o� le ClickManager d�tecte que le clic de la souris est maintenu
    /// </summary>
    private void ClickHold()
    {
        if (clickHoldMode == ClickHoldMode.MustClickOnAreaFirst && !HasBeenClickedInArea) return;

        if (!CheckIfMouseInArea()) return;

        onClickHold.Invoke();
    }

    /// <summary>
    /// Appel� lorsque le ClickManager d�tecte un rel�chement du clic de la souris
    /// </summary>
    private void ClickUp()
    {
        if (!(CheckIfMouseInArea() || (clickUpMode == ClickUpMode.Permissive && HasBeenClickedInArea))) return;

        HasBeenClickedInArea = false;

        onClickUp.Invoke();
    }

    /// <summary>
    /// V�rifie si la souris est dans la zone cliquable de l'objet
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
