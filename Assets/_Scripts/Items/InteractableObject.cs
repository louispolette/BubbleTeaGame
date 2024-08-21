using UnityEngine;
using UnityEngine.Rendering;

public abstract class InteractableObject : MonoBehaviour
{
    //[Header("Bounds")]

    [Tooltip("La méthode utilisée pour définir la zone cliquable de l'objet")]
    [SerializeField] protected BoundsMode _boundsMode = BoundsMode.useRenderer;

    [SerializeField] private Renderer _renderer;
    [SerializeField] private SortingGroup _sortingGroup;
    [SerializeField] private Collider2D _collider;

    [SerializeField] private bool _showArea = false;

    public enum BoundsMode { useRenderer, useCollider };

    public Renderer Renderer => _renderer;
    public SortingGroup SortingGroup => _sortingGroup;
    public Collider2D Collider => _collider;

    protected virtual void Awake()
    {
        _renderer = GetComponentInChildren<Renderer>();
        _sortingGroup = GetComponentInChildren<SortingGroup>();
        _collider = GetComponentInChildren<Collider2D>();
    }

    /// <summary>
    /// Retourne les limites du renderer ou du collider en fonction de ClickAreaMode
    /// </summary>
    /// <returns></returns>
    public Bounds GetBounds()
    {
        switch (_boundsMode)
        {
            case BoundsMode.useRenderer:
                return Renderer.bounds;
            case BoundsMode.useCollider:
                return Collider.bounds;
            default:
                return new Bounds();
        }
    }

    public int SortingLayerValue
    {
        get
        {
            if (SortingGroup != null)
            {
                return SortingLayer.GetLayerValueFromID(_sortingGroup.sortingLayerID);
            }
            else
            {
                return SortingLayer.GetLayerValueFromID(_renderer.sortingLayerID);
            }
        }
    }

    public int SortingOrder
    {
        get
        {
            if (_sortingGroup != null)
            {
                return _sortingGroup.sortingOrder;
            }
            else
            {
                return _renderer.sortingOrder;
            }
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
        if (_showArea)
        {
            Gizmos.color = Color.white;

            Bounds bounds = new Bounds();

            switch (_boundsMode)
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
