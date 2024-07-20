using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public static ClickManager Instance;

    public static Vector3 mousePosition;
    public static ClickableObject draggedObject;

    public delegate void MouseClickDown();
    /// <summary>
    /// Invoqu� lorsque le ClickManager d�tecte un clic de la souris
    /// </summary>
    public static event MouseClickDown OnMouseClickDown;

    public delegate void MouseClickHold();
    /// <summary>
    /// Invoqu� chaque frame o� le ClickManager d�tecte que le clic de la souris est maintenu
    /// </summary>
    public static event MouseClickHold OnMouseClickHold;

    public delegate void MouseClickUp();
    /// <summary>
    /// Invoqu� lorsque le ClickManager d�tecte rel�chement d'un clic de la souris
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
    /// Bouge la position de l'objet d�plac� � celle de la souris
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
    /// Permet de set l'objet tenu � l'objet donn� en param�tre
    /// </summary>
    /// <param name="newObject">L'objet � tenir</param>
    public void StartDraggingObject(ClickableObject newObject)
    {
        if (draggedObject != null) StopDraggingObject();

        newObject.IsBeingDragged = true;
        newObject.onDragStart.Invoke();
        draggedObject = newObject;
    }

    /// <summary>
    /// Arr�te de d�placer l'objet actuellement tenu
    /// </summary>
    public void StopDraggingObject()
    {
        if (draggedObject == null) return;

        draggedObject.IsBeingDragged = false;
        draggedObject.onDrop.Invoke();
        draggedObject = null;
    }

    /// <summary>
    /// V�rifie si la position de la souris est dans les limites d�finies
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
