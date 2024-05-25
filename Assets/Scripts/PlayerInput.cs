using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerInput : MonoBehaviour
{
    private static PlayerInput _instance;
    public static PlayerInput Instance =>_instance;
    private void Awake()
    {
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    [FormerlySerializedAs("currentHolder")] public CardContainer currentContainer;

    public Card hoveredCard;
    public Card currentCard;
    
    
    [HideInInspector] public UnityEvent<Card> PointerEnterEvent = new();
    [HideInInspector] public UnityEvent<Card> PointerExitEvent = new();
    [HideInInspector] public UnityEvent<Card, bool> PointerUpEvent = new();
    [HideInInspector] public UnityEvent<Card> PointerDownEvent = new();
    [HideInInspector] public UnityEvent<Card> BeginDragEvent = new();
    [HideInInspector] public UnityEvent<Card> EndDragEvent = new();
    [HideInInspector] public UnityEvent<Card, bool> SelectEvent = new();
}
