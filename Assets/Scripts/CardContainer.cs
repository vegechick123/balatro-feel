using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardContainer : MonoBehaviour,IPointer
{

    [SerializeField] private GameObject slotPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int cardsToSpawn = 7;
    public List<Card> cards;
    private SpriteRenderer _spriteRenderer;
    
    private TransformLayout _layout;
    private void Awake()
    {
        _layout = GetComponent<TransformLayout>();
        _spriteRenderer= GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        for (int i = 0; i < cardsToSpawn; i++)
        {
            Instantiate(slotPrefab, transform);
        }
        
        foreach (var card in GetComponentsInChildren<Card>())
        {
            card.SetParent(this);
        }
        
        int cardCount = 0;
        
        foreach (Card card in cards)
        {
            card.name = cardCount.ToString();
            cardCount++;
        }
        _layout.Refresh();
    }



    void Update()
    {
        /*if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }*/

        Card currentCard = PlayerInput.Instance.currentCard;
        
        if (PlayerInput.Instance.currentContainer != this|| currentCard == null)
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
                }
                
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
        /*Transform focusedParent = selectedCard.transform.parent;
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
        }*/
    }

    public void AddCard(Card card, bool refresh=true)
    {
        Debug.Assert(card.parent == null, "Already has parent!");
        cards.Add(card);
        _layout.Refresh();
    }
    public void RemoveCard(Card card, bool refresh=true)
    {
        Debug.Assert(card.parent != this, "Should call after set card parent!");
        cards.Remove(card);
        _layout.Refresh();
    }
    public void OnPointerEnter()
    {
        PlayerInput.Instance.currentContainer = this;
        return;
    }

    public void OnPointerExit()
    {
        if (PlayerInput.Instance.currentContainer == this)
            PlayerInput.Instance.currentContainer = null;
        return;
    }
}
