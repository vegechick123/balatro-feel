using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HorizontalCardHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,IPointer
{

    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 7;
    public List<Card> cards;
    
    [SerializeField] private bool tweenCardReturn = true;

    private HorizontalLayoutGroup _horizontalLayoutGroup;

    private TransformLayout _layout;
    private void Awake()
    {
        _horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        _layout = GetComponent<TransformLayout>();
    }

    void Start()
    {
        for (int i = 0; i < cardsToSpawn; i++)
        {
            Instantiate(slotPrefab, transform);
        }

        rect = GetComponent<RectTransform>();
        foreach (var card in GetComponentsInChildren<Card>())
        {
            card.SetParent(this);
            cards.Add(card);
        }
        
        int cardCount = 0;

        /*PlayerInput.Instance.PointerEnterEvent.AddListener(CardPointerEnter);
        PlayerInput.Instance.PointerExitEvent.AddListener(CardPointerExit);
        PlayerInput.Instance.BeginDragEvent.AddListener(BeginDrag);
        PlayerInput.Instance.EndDragEvent.AddListener(EndDrag);*/
        foreach (Card card in cards)
        {
            card.name = cardCount.ToString();
            cardCount++;
        }
        _layout.Refresh();
    }

    private void BeginDrag(Card card)
    {
        selectedCard = card;
    }


    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        Card currentCard = PlayerInput.Instance.currentCard;
        
        if (PlayerInput.Instance.currentHolder != this|| currentCard == null)
            return;


       //  Debug.Log($"{this},Run");
        
        
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i].cardVisual != null)
                cards[i].cardVisual.UpdateIndex(transform.childCount);
        }

        if (cards.Contains(currentCard))
        {
            int originIndex = cards.IndexOf(currentCard);
            for (int i = 0; i < cards.Count; i++)
            {
                /*if (cards[i] == currentCard)
                {
                    if (i + 1 >= cards.Count || currentCard.transform.position.x < cards[i + 1].transform.position.x)
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }*/

                //Debug.Log($"{currentCard.transform.position.x}," );
                int compareIndex = i<originIndex? i :i + 1;
                if (i + 1 >= cards.Count || currentCard.transform.position.x < cards[compareIndex].transform.position.x)
                {
                    if (i != originIndex)
                    {
                        MoveCardTo(currentCard,i);
                    }
                    break;
                }
            }
        }
        else
        {
            for (int i = 0; i < cards.Count+1; i++)
            {
                if (i == cards.Count||currentCard.transform.position.x < cards[i].transform.position.x)
                {
                    InsertCardAt(currentCard,i);
                    break;
                    /*if (currentCard.GetComponentInParent<HorizontalCardHolder>()!=this)
                    {
                        InsertCardAt(currentCard,i);
                        break;
                    }*/
                }

                /*if (currentCard.transform.position.x < cards[i].transform.position.x)
                {
                    if (currentCard.GetComponentInParent<HorizontalCardHolder>()!=this)
                    {
                        InsertCardAt(currentCard,i);
                        break;
                    }
                }*/
            }
        }

    }

    void MoveCardTo(Card insetCard, int index)
    {
        Debug.Log($"Move :{insetCard},{index}");
        Debug.Assert(cards.Contains(insetCard));
        cards.Remove(insetCard);
        cards.Insert(index,insetCard);
        Transform cardParent = insetCard.transform.parent;
        cardParent.transform.SetSiblingIndex(index);
        insetCard.DetachSlot();
        _layout.Refresh();
        insetCard.AttachSlot();
    }
    void InsertCardAt(Card insetCard,int index)
    {
        Debug.Log($"{insetCard},{index}");
        Debug.Assert(!cards.Contains(insetCard));
        Transform cardParent = insetCard.transform.parent;
        insetCard.SetParent(this);
        cards.Insert(index,insetCard);
        cardParent.transform.SetSiblingIndex(index);
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
            
        }
        insetCard.DetachSlot();
        _layout.Refresh();
        insetCard.AttachSlot();
    }
    void Swap(int index)
    {

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            card.cardVisual.UpdateIndex(transform.childCount);
        }
    }

    public void RemoveCard(Card card)
    {
        Debug.Assert(card.parent != this, "Should call after set card parent!");
        cards.Remove(card);
        _layout.Refresh();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayerInput.Instance.currentHolder = this;
        return;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (PlayerInput.Instance.currentHolder == this)
            PlayerInput.Instance.currentHolder = null;
        return;
    }

    public void OnPointerEnter()
    {
        OnPointerEnter(null);
    }

    public void OnPointerExit()
    {
        OnPointerExit(null);
    }
}
