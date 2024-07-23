using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    [Tooltip("Fréquence en secondes de la détection de mouvement des objets tirés\n\nPlus cette valeur est petite, plus la détection de mouvement sera sensible")]
    [SerializeField] private float _moveDetectionTimestep = 0.05f;

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
    private bool _isDraggingObject;
    private float _moveDetectionTimer;

    private Camera _mainCamera;

    public List<ClickableObject> ClickedObjectsThisFrame { get; set; } = new List<ClickableObject>();

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

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        // Get mouse position

        mousePosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);

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
        if (Input.GetMouseButtonDown(0) && OnMouseClickDown != null)
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
                _offset = draggedObject.transform.position - mousePosition;
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (!draggedObject)
            {
                OnMouseClickHold?.Invoke();
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
                OnMouseClickUp?.Invoke();
            }
        }
    }

    /// <summary>
    /// Renvoie l'objet cliquable parmi tous ceux dans la liste donnée qui est rendu au-dessus des autres
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
    /// Utilisé dans GetObjectInFront()
    /// </summary>
    private enum Comparison { CurrentIsHigher, CurrentIsLower, Equal }


    /// <summary>
    /// Vérifie si l'objet tiré a changé de position depuis le dernier appel de cette méthode
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
    /// Bouge la position de l'objet déplacé à celle de la souris
    /// </summary>
    private void DragObject()
    {
        Vector3 targetPosition = mousePosition + _offset;

        draggedObject.Drag(targetPosition);
    }

    /// <summary>
    /// Permet de set l'objet tenu à l'objet donné en paramètre
    /// </summary>
    /// <param name="newObject">L'objet à tenir</param>
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
    /// Arrête de déplacer l'objet actuellement tenu
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
