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

    public delegate void MouseClickHold();
    /// <summary>
    /// Invoqué chaque frame où le ClickManager détecte que le clic de la souris est maintenu
    /// </summary>
    public static event MouseClickHold OnMouseClickHold;

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
        // Get info

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Get input & send events

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

        if (Input.GetMouseButton(0))
        {
            OnMouseClickHold?.Invoke();
        }

        if (Input.GetMouseButtonUp(0))
        {
            OnMouseClickUp?.Invoke();

            if (draggedObject)
            {
                StopDraggingObject();
            }
        }

        // Update dragged object

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
        draggedObject.onDrag.Invoke();

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

    /// <summary>
    /// Permet de set l'objet tenu à l'objet donné en paramètre
    /// </summary>
    /// <param name="newObject">L'objet à tenir</param>
    public void StartDraggingObject(ClickableObject newObject)
    {
        if (draggedObject != null) StopDraggingObject();

        newObject.IsBeingDragged = true;
        newObject.onDragStart.Invoke();
        draggedObject = newObject;
    }

    /// <summary>
    /// Arrête de déplacer l'objet actuellement tenu
    /// </summary>
    public void StopDraggingObject()
    {
        if (draggedObject == null) return;

        draggedObject.IsBeingDragged = false;
        draggedObject.onDrop.Invoke();
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
