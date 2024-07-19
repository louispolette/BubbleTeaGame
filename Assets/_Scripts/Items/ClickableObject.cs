using UnityEngine;

public class ClickableObject : MonoBehaviour
{
    [Space]

    [SerializeField] private bool _isDraggable = false;

    [Space]

    [Tooltip("La méthode utilisée pour définir la zone cliquable de l'objet")]
    [SerializeField] ClickAreaMode clickAreaMode = ClickAreaMode.useRenderer;

    [Header("Debug")]

    [SerializeField] private bool _showClickableArea = false;

    public enum ClickAreaMode
    {
        useRenderer,
        useCollider
    }

    private Renderer _renderer;
    private Collider2D _collider;
    public Rigidbody2D Rigidbody { get; private set; }

    private void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _collider = GetComponentInChildren<Collider2D>();
        Rigidbody = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        ClickManager.OnMouseClickDown += CheckForClick;
    }

    private void OnDisable()
    {
        ClickManager.OnMouseClickDown -= CheckForClick;
    }

    /// <summary>
    /// Appelé lorsque le ClickManager détecte un clic de la souris
    /// </summary>
    private void CheckForClick()
    {
        Bounds bounds = GetBounds();

        bool isInBounds = ClickManager.IsMouseInBounds(bounds);

        if (isInBounds)
        {
            if (_isDraggable)
            {
                ClickManager.draggedObject = this;
            }

            // Invoke click action
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
}
