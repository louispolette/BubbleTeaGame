using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    #region serialized members

    [Header("Effects")]

    [Tooltip("Effet créé à la position de la souris lorsque le joueur clique")]
    [SerializeField] private ParticleSystem _clickEffect;

    [Header("Sound")]

    [SerializeField] private AudioClip _defaultClickSound;
    [SerializeField] private AudioClip _defaultDragStartSound;
    [SerializeField] private AudioClip _defaultDropSound;

    [Header("Movement Detection")]

    [Tooltip("Fréquence en secondes de la détection de mouvement des objets tirés\n\nPlus cette valeur est petite, plus la détection de mouvement sera sensible")]
    [SerializeField] private float _moveDetectionTimestep = 0.05f;

    #endregion

    #region unserialized members

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

    public delegate void ReleaseObject();
    /// <summary>
    /// Invoqué lorsque un objet est relâché
    /// </summary>
    public static event ReleaseObject OnReleaseObject;

    private Vector3 _offset;
    private bool _isDraggingObject;
    private float _moveDetectionTimer;
    private bool _clickSoundOverriden = false;

    private Camera _mainCamera;
    private ParticleSystem _clickFXInstance;

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
            _clickSoundOverriden = false;

            if (OnMouseClickDown != null) TriggerClickDownEvent();

            if (!_clickSoundOverriden) DoClickSound();

            if (!draggedObject) DoClickEffect();
        }

        if (Input.GetMouseButton(0))
        {
            if (!draggedObject) TriggerClickHoldEvent();
        }

        if (Input.GetMouseButtonUp(0))
        {
            TriggerClickUpEvent();

            if (draggedObject)
            {
                StopDraggingObject();
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

            if (clickedObject.IsDraggable)
            {
                StartDraggingObject(clickedObject);
            }
            else if (clickedObject.ClickSFX != null)
            {
                clickedObject.PlayClickSFX();
                _clickSoundOverriden = true;
            }
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

                    float currentRendererPosition = clickedObjects[i].Renderer.transform.position.y;
                    float objectInFrontRendererPosition = objectInFront.Renderer.transform.position.y;

                    if (currentRendererPosition < objectInFrontRendererPosition)
                    {
                        objectInFront = clickedObjects[i];
                    }
                }
            }

            #region comparision functions

            Comparison CompareSortingLayers()
            {

                int currentObjectLayerVal = clickedObjects[i].SortingLayerValue;
                int objectInFrontLayerVal = objectInFront.SortingLayerValue;

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
                int currentObjectSortOrder = clickedObjects[i].SortingOrder;
                int objectInFrontSortingOrder = objectInFront.SortingOrder;

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

        if (newObject.DragStartSFX != null)
        {
            newObject.PlayDragStartSFX();
            _clickSoundOverriden = true;
        }
        else
        {
            DoDragStartSound();
        }

        newObject.LeaveHolder();
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

        if (draggedObject.DragStartSFX != null)
        {
            draggedObject.PlayDropSFX();
        }
        else
        {
            DoDropSound();
        }

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
        if (_defaultClickSound == null) return;

        AudioManager.Instance.PlaySound(_defaultClickSound, 1f, 1f, 0.05f);
    }
    private void DoDragStartSound()
    {
        if (_defaultDragStartSound == null) return;

        AudioManager.Instance.PlaySound(_defaultDragStartSound, 1f, 1f, 0.05f);
    }

    private void DoDropSound()
    {
        if (_defaultDropSound == null) return;

        AudioManager.Instance.PlaySound(_defaultDropSound, 1f, 1f, 0.05f);
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
