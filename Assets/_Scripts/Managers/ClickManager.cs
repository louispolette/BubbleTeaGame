using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Tooltip("Fr�quence en secondes de la d�tection de mouvement des objets tir�s")]
    [SerializeField] private float _moveDetectionTimestep = 0.05f;

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
    private bool _isDraggingObject;

    private float _moveDetectionTimer;

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

        if (_isDraggingObject)
        {
            draggedObject.onDragged.Invoke();
            if (draggedObject.IsMoving) draggedObject.onMoved.Invoke();

            DragObject();

            _moveDetectionTimer += Time.deltaTime;

            if(_moveDetectionTimer >= _moveDetectionTimestep)
            {
                MoveDetectionUpdate();
            }
        }
    }

    private void MoveDetectionUpdate()
    {
        _moveDetectionTimer = 0f;
        
        Debug.Log(Vector2.Distance(draggedObject.transform.position, draggedObject.lastPosition));
        //if (Vector2.Distance(draggedObject.transform.position, draggedObject.lastPosition) >= _immobileObjectLeniency)
        if (draggedObject.transform.position != draggedObject.lastPosition)
        {
            draggedObject.IsMoving = true;
        }
        else
        {
            draggedObject.IsMoving = false;
        }

        draggedObject.lastPosition = draggedObject.transform.position;
    }

    /// <summary>
    /// Bouge la position de l'objet d�plac� � celle de la souris
    /// </summary>
    private void DragObject()
    {
        Vector3 targetPosition = mousePosition + _offset;

        draggedObject.Drag(targetPosition);
    }

    /// <summary>
    /// Permet de set l'objet tenu � l'objet donn� en param�tre
    /// </summary>
    /// <param name="newObject">L'objet � tenir</param>
    public void StartDraggingObject(ClickableObject newObject)
    {
        _isDraggingObject = true;

        if (draggedObject != null) StopDraggingObject();

        newObject.IsBeingDragged = true;
        newObject.IsMoving = false;
        newObject.onDraggedStart.Invoke();
        draggedObject = newObject;
    }

    /// <summary>
    /// Arr�te de d�placer l'objet actuellement tenu
    /// </summary>
    public void StopDraggingObject()
    {
        _isDraggingObject = false;

        if (draggedObject == null) return;

        draggedObject.IsBeingDragged = false;
        draggedObject.IsMoving = false;
        draggedObject.onDropped.Invoke();
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
