
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler,IPressAble,IHoverable
{
    public Transform visualContainer;
    private Canvas canvas;
    [SerializeField] private bool instantiateVisual = true;
    private VisualCardsHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject cardVisualPrefab;
    [HideInInspector] public CardVisual cardVisual;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    public CardContainer parent;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent;
    [HideInInspector] public UnityEvent<Card> PointerExitEvent;
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<Card> PointerDownEvent;
    [HideInInspector] public UnityEvent<Card> BeginDragEvent;
    [HideInInspector] public UnityEvent<Card> EndDragEvent;
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent;


    public Transform slotTransform { get; private set; }

    public void DetachSlot()
    {
        transform.SetParent(null,true);
    }
    public void AttachSlot()
    {
        transform.SetParent(slotTransform,true);
    }
    
    public void SetParent(CardContainer inParent,int index = -1)
    {
        CardContainer originParent = parent;
        
        parent = null;
        if (originParent)
        {
            originParent.RemoveCard(this);
        }
        inParent.AddCard(this,index);
        parent = inParent;
        slotTransform.parent = parent.transform;
    }
    
    private void Awake()
    {
        slotTransform = transform.parent;
    }

    void Start()
    {

        if (!instantiateVisual)
            return;

        visualHandler = FindObjectOfType<VisualCardsHandler>();
        cardVisual = Instantiate(cardVisualPrefab, visualHandler ? visualHandler.transform : visualContainer).GetComponent<CardVisual>();
        cardVisual.Initialize(this);
    }

    public void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        PlayerInput.Instance.currentCard = this;
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        
        isDragging = true;
        // canvas.GetComponent<GraphicRaycaster>().enabled = false;
        wasDragged = true;
        DetachSlot();
    }

    public void OnDrag(PointerEventData eventData)
    {
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        transform.DOLocalMove( Vector3.zero, true ? .15f : 0).SetEase(Ease.OutBack);
        PlayerInput.Instance.currentCard = null;
        isDragging = false;
        AttachSlot();
        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
        PlayerInput.Instance.hoveredCard = this;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
        PlayerInput.Instance.hoveredCard = this;
    }


    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0) != true)
            return;
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        if (selected)
            transform.localPosition += (cardVisual.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (cardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {
        return slotTransform.CompareTag("Slot") ? slotTransform.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return slotTransform.CompareTag("Slot") ? slotTransform.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return slotTransform.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(slotTransform.parent.childCount - 1), 0, 1) : 0;
    }

    private void OnDestroy()
    {
        if(cardVisual != null)
            Destroy(cardVisual.gameObject);
    }

    public void OnPressBegin()
    {
        OnBeginDrag(null);
    }

    public void OnPressEnd()
    {
        OnEndDrag(null);
    }

    public void OnPressing()
    {
        
    }

    public void OnHover()
    {
        
    }
}
