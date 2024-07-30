using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    #region serialized members

    [Header("Effects")]

    [Tooltip("Effet cr�� � la position de la souris lorsque le joueur clique")]
    [SerializeField] private ParticleSystem _clickEffect;

    [SerializeField] private AudioClip _clickSound;

    [Header("Movement Detection")]

    [Tooltip("Fr�quence en secondes de la d�tection de mouvement des objets tir�s\n\nPlus cette valeur est petite, plus la d�tection de mouvement sera sensible")]
    [SerializeField] private float _moveDetectionTimestep = 0.05f;

    #endregion

    #region unserialized members

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

    public delegate void ReleaseObject();
    /// <summary>
    /// Invoqu� lorsque un objet est rel�ch�
    /// </summary>
    public static event ReleaseObject OnReleaseObject;

    private Vector3 _offset;
    private bool _isDraggingObject;
    private float _moveDetectionTimer;

    private Camera _mainCamera;
    private ParticleSystem _clickFXInstance;
    private AudioSource _audioSource;

    public List<ClickableObject> ClickedObjectsThisFrame { get; set; } = new List<ClickableObject>();

    #endregion

    private void Awake()
    {
        #region singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        #endregion

        _mainCamera = Camera.main;
        _audioSource = GetComponentInChildren<AudioSource>();
        _audioSource.clip = _clickSound;

        if (_clickEffect)
        {
            _clickFXInstance = Instantiate(_clickEffect, transform);
        }
    }

    private void Update()
    {
        // Get mouse position

        Vector3 screenToWorldPos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition = new Vector3(screenToWorldPos.x, screenToWorldPos.y, 0f);

        // Get input & send events

        HandleInput();

        // Move dragged object

        if (!_isDraggingObject) return;

        draggedObject.onDragged.Invoke();

        if (draggedObject.IsMoving)
        {
            draggedObject.onMoved.Invoke();
        }

        DragObject();

        // Check if dragged object has moved

        _moveDetectionTimer += Time.deltaTime;

        if (_moveDetectionTimer >= _moveDetectionTimestep)
        {
            DoMoveDetectionUpdate();
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (OnMouseClickDown != null)
            {
                TriggerClickDownEvent();
            }

            DoClickSound();

            if (!draggedObject)
            {
                DoClickEffect();
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (!draggedObject)
            {
                TriggerClickHoldEvent();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (draggedObject)
            {
                StopDraggingObject();
            }
            else
            {
                TriggerClickUpEvent();
            }
        }
    }

    private void TriggerClickDownEvent()
    {
        ClickedObjectsThisFrame.Clear();

        OnMouseClickDown();

        ClickableObject clickedObject = GetObjectInFront(ClickedObjectsThisFrame);

        if (clickedObject != null)
        {
            clickedObject.onClickedDown.Invoke();

            if (clickedObject.IsDraggable) StartDraggingObject(clickedObject);
        }

        if (draggedObject)
        {
            _offset = (draggedObject.CenterOnDrag) ? Vector3.zero : draggedObject.transform.position - mousePosition;
        }
    }

    private void TriggerClickHoldEvent()
    {
        OnMouseClickHold?.Invoke();
    }

    private void TriggerClickUpEvent()
    {
        OnMouseClickUp?.Invoke();
    }

    /// <summary>
    /// Renvoie l'objet cliquable parmi tous ceux dans la liste donn�e qui est rendu au-dessus des autres
    /// </summary>
    /// <returns></returns>
    private ClickableObject GetObjectInFront(List<ClickableObject> clickedObjects)
    {
        if (clickedObjects.Count == 0) return null;

        ClickableObject objectInFront = clickedObjects[0];

        for (int i = 1; i < clickedObjects.Count; i++)
        {
            // We first compare sorting layers

            Comparison layerComparison = CompareSortingLayers();

            if (layerComparison == Comparison.CurrentIsHigher)
            {
                objectInFront = clickedObjects[i];
            }
            else if (layerComparison == Comparison.Equal)
            {
                // If they're the same, we compare sorting orders

                Comparison sortOrderComparison = CompareSortingOrders();

                if (sortOrderComparison == Comparison.CurrentIsHigher)
                {
                    objectInFront = clickedObjects[i];
                }
                else if (sortOrderComparison == Comparison.Equal)
                {
                    // If they're also the same, we compare their y positions

                    float currentRendererPosition = clickedObjects[i]._renderer.transform.position.y;
                    float objectInFrontRendererPosition = objectInFront._renderer.transform.position.y;

                    if (currentRendererPosition < objectInFrontRendererPosition)
                    {
                        objectInFront = clickedObjects[i];
                    }
                }
            }

            #region comparision functions

            Comparison CompareSortingLayers()
            {
                int currentObjectLayerVal = SortingLayer.GetLayerValueFromID(clickedObjects[i]._renderer.sortingLayerID);
                int objectInFrontLayerVal = SortingLayer.GetLayerValueFromID(objectInFront._renderer.sortingLayerID);

                if (currentObjectLayerVal > objectInFrontLayerVal)
                {
                    return Comparison.CurrentIsHigher;
                }
                else if (currentObjectLayerVal < objectInFrontLayerVal)
                {
                    return Comparison.CurrentIsLower;
                }
                else
                {
                    return Comparison.Equal;
                }
            }

            Comparison CompareSortingOrders()
            {
                int currentObjectSortOrder = clickedObjects[i]._renderer.sortingOrder;
                int objectInFrontSortingOrder = objectInFront._renderer.sortingOrder;

                if (currentObjectSortOrder > objectInFrontSortingOrder)
                {
                    return Comparison.CurrentIsHigher;
                }
                else if (currentObjectSortOrder < objectInFrontSortingOrder)
                {
                    return Comparison.CurrentIsLower;
                }
                else
                {
                    return Comparison.Equal;
                }
            }

            #endregion
        }

        return objectInFront;
    }

    /// <summary>
    /// Utilis� dans GetObjectInFront()
    /// </summary>
    private enum Comparison { CurrentIsHigher, CurrentIsLower, Equal }


    /// <summary>
    /// V�rifie si l'objet tir� a chang� de position depuis le dernier appel de cette m�thode
    /// </summary>
    private void DoMoveDetectionUpdate()
    {
        _moveDetectionTimer = 0f;
        
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
        newObject.LeaveHolder();
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

        OnReleaseObject?.Invoke();

        draggedObject = null;
    }
    
    private void DoClickEffect()
    {
        if (_clickEffect == null) return;

        _clickFXInstance.transform.position = mousePosition;
        _clickFXInstance.Emit(1);
    }

    private void DoClickSound()
    {
        if (_clickSound == null) return;

        _audioSource.pitch = Random.Range(0.95f, 1.05f);
        _audioSource.PlayOneShot(_clickSound);
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
