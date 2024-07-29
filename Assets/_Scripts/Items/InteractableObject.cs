using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    [Header("Bounds")]

    [Tooltip("La méthode utilisée pour définir la zone cliquable de l'objet")]
    [SerializeField] protected BoundsMode boundsMode = BoundsMode.useRenderer;
    [SerializeField] private bool _showClickableArea = false;
    public enum BoundsMode { useRenderer, useCollider };

    public Renderer _renderer { get; private set; }
    public Collider2D _collider { get; private set; }

    protected virtual void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _collider = GetComponentInChildren<Collider2D>();
    }

    /// <summary>
    /// Retourne les limites du renderer ou du collider en fonction de ClickAreaMode
    /// </summary>
    /// <returns></returns>
    public Bounds GetBounds()
    {
        switch (boundsMode)
        {
            case BoundsMode.useRenderer:
                return _renderer.bounds;
            case BoundsMode.useCollider:
                return _collider.bounds;
            default:
                return new Bounds();
        }
    }

    public bool CheckIfPointInArea(Vector3 point)
    {
        return GetBounds().Contains(point);
    }

    /// <summary>
    /// Vérifie si la souris est dans la zone cliquable de l'objet
    /// </summary>
    protected bool CheckIfMouseInArea()
    {
        Bounds bounds = GetBounds();

        return ClickManager.IsMouseInBounds(bounds);
    }

    #region gizmos

    private void OnDrawGizmosSelected()
    {
        if (_showClickableArea)
        {
            Gizmos.color = Color.white;

            Bounds bounds = new Bounds();

            switch (boundsMode)
            {
                case BoundsMode.useRenderer:
                    Renderer renderer = GetComponentInChildren<Renderer>();
                    if (!renderer) break;
                    bounds = renderer.bounds;
                    break;
                case BoundsMode.useCollider:
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
