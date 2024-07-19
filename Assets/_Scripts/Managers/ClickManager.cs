using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance;

    public static Vector3 mousePosition;
    public static ClickableObject draggedObject;

    public delegate void MouseClickDown();
    /// <summary>
    /// Invoqué lorsque le ClickManager détecte un clic de la souris
    /// </summary>
    public static event MouseClickDown OnMouseClickDown;

    public delegate void MouseClickUp();
    /// <summary>
    /// Invoqué lorsque le ClickManager détecte relâchement d'un clic de la souris
    /// </summary>
    public static event MouseClickUp OnMouseClickUp;

    private Vector3 _offset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButtonDown(0))
        {
            if (OnMouseClickDown != null)
            {
                OnMouseClickDown();

                if (draggedObject)
                {
                    _offset = draggedObject.transform.position - mousePosition;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (OnMouseClickUp != null)
            {
                OnMouseClickUp();
            }

            if (draggedObject)
            {
                ReleaseDraggedObject();
            }
        }

        if (draggedObject)
        {
            DragObject();
        }
    }

    /// <summary>
    /// Bouge la position de l'objet déplacé à celle de la souris
    /// </summary>
    private void DragObject()
    {
        Vector3 targetPosition = mousePosition + _offset;

        if (draggedObject.Rigidbody) // Utilise Rigidbody si possible
        {
            draggedObject.Rigidbody.MovePosition(targetPosition);
        }
        else
        {
            draggedObject.transform.position = targetPosition;
        }
    }

    private void ReleaseDraggedObject()
    {
        draggedObject = null;
    }

    /// <summary>
    /// Vérifie si la position de la souris est dans les limites définies
    /// </summary>
    /// <param name="bounds"></param>
    /// <returns></returns>
    public static bool IsMouseInBounds(Bounds bounds)
    {
        Vector3 positionToCheck = mousePosition;
        positionToCheck.z = bounds.center.z;

        bool isInBounds = false;

        if (bounds.Contains(positionToCheck))
        {
            isInBounds = true;
        }

        return isInBounds;
    }
}
