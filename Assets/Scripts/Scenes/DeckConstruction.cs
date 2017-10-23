using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class DeckConstruction : Scene<MainTransitionData> {

    private MainTransitionData transitionData;
    private Dictionary<Card.CardType, CardCount> clumpedCollection;
    [SerializeField]
    private Vector3 deckDisplaySpacing;
    [SerializeField]
    private Vector3 deckDisplayOffset;
    [SerializeField]
    private int deckDisplayCols;
    [SerializeField]
    private int deckDisplayRows;
    [SerializeField]
    private Vector3 collectionDisplaySpacing;
    [SerializeField]
    private Vector3 collectionDisplayOffset;
    [SerializeField]
    private int collectionDisplayCols;
    [SerializeField]
    private int collectionDisplayRows;
    [SerializeField]
    private Transform deckZone;
    [SerializeField]
    private Transform collectionZone;
    [SerializeField]
    private GameObject collectionPage;
    private List<GameObject> collectionPages;
    private int currentPageNum;
    [SerializeField]
    private GameObject nextPageButton;
    [SerializeField]
    private GameObject prevPageButton;
    [SerializeField]
    private Text pageCounter;
    [SerializeField]
    private Text deckCounter;
    [SerializeField]
    private Button submitDeckButton;

    internal override void Init()
    {
        Services.DeckConstruction = this;
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
    }

    internal override void OnEnter(MainTransitionData data)
    {
        transitionData = data;
        PopulateCollection();
        PopulateDeckDisplay();
    }

    internal override void OnExit()
    {
        foreach(Card card in transitionData.deck)
        {
            card.collectionMode = false;
        }
    }

    void PopulateDeckDisplay()
    {
        for (int i = 0; i < transitionData.deck.Count; i++)
        {
            Card card = transitionData.deck[i];
            card.CreatePhysicalCard(deckZone);
            card.controller.EnterDeckBuildingMode();
            card.Reposition(GetDeckPosition(i), true);
        }
        AssessDeckReadiness();
    }

    void PopulateCollection()
    {
        collectionPages = new List<GameObject>();
        clumpedCollection = new Dictionary<Card.CardType, CardCount>();
        Transform currentPage = Instantiate(collectionPage, collectionZone).transform;
        collectionPages.Add(currentPage.gameObject);
        AddCollectionEntry(Card.CardType.Step, -1, 0, currentPage);
        AddCollectionEntry(Card.CardType.Punch, -1, 1, currentPage);
        int collectionIndex = 1;
        for (int i = 0; i < transitionData.collection.Count; i++)
        {
            Card card = transitionData.collection[i];
            bool alreadyPresent = false;
            foreach (Card.CardType cardType in clumpedCollection.Keys)
            {
                if (cardType == card.cardType)
                {
                    clumpedCollection[cardType].Add();
                    alreadyPresent = true;
                    break;
                }
            }
            if (!alreadyPresent)
            {
                collectionIndex += 1;
                if (collectionIndex % (collectionDisplayRows * collectionDisplayCols) == 0)
                {
                    currentPage = Instantiate(collectionPage, collectionZone).transform;
                    collectionPages.Add(currentPage.gameObject);
                }
                AddCollectionEntry(card.cardType, 1, collectionIndex, currentPage);
            }
        }
        SetCollectionPage(0);
        prevPageButton.SetActive(false);
        if(collectionPages.Count < 2)
        {
            nextPageButton.SetActive(false);
        }
    }

    void SetCollectionPage(int pageNum)
    {
        currentPageNum = pageNum;
        for (int i = 0; i < collectionPages.Count; i++)
        {
            if (i != currentPageNum) collectionPages[i].SetActive(false);
            else collectionPages[i].SetActive(true);
        }
        if (currentPageNum >= collectionPages.Count - 1) nextPageButton.SetActive(false);
        else nextPageButton.SetActive(true);
        if (currentPageNum == 0) prevPageButton.SetActive(false);
        else prevPageButton.SetActive(true);

        pageCounter.text = "Page " + (currentPageNum + 1) + "/" + collectionPages.Count;
    }

    void AddCollectionEntry(Card.CardType type, int count, int index, Transform currentPage)
    {
        Card card = Services.CardConfig.CreateCardOfType(type);
        clumpedCollection[type] = new CardCount(count, card);
        card.CreatePhysicalCard(currentPage);
        card.controller.EnterDeckBuildingMode();
        card.Reposition(GetCollectionPosition(index), true);
        clumpedCollection[type].CreateCounter(card.controller.transform);
    }

    Vector3 GetCollectionPosition(int index)
    {
        return collectionDisplayOffset + new Vector3(
                index % collectionDisplayCols * collectionDisplaySpacing.x,
                (index / collectionDisplayCols) % collectionDisplayRows * collectionDisplaySpacing.y,
                0);
    }

    Vector3 GetDeckPosition(int index)
    {
        return new Vector3(index % deckDisplayCols * deckDisplaySpacing.x,
            (index / deckDisplayCols) * deckDisplaySpacing.y,
            index * deckDisplaySpacing.z) + deckDisplayOffset;
    }
    
    void SortDeck()
    {
        for (int i = 0; i < transitionData.deck.Count; i++)
        {
            transitionData.deck[i].Reposition(GetDeckPosition(i), true);
        }
    }

    public void OnCardClicked(Card card)
    {
        if (transitionData.deck.Contains(card))
        {
            transitionData.deck.Remove(card);
            card.DestroyPhysicalCard();
            SortDeck();
            if(!clumpedCollection.ContainsKey(card.cardType))
            {
                int collectionIndex = clumpedCollection.Count;
                Transform currentPage;
                if (collectionIndex % (collectionDisplayCols * collectionDisplayRows) == 0)
                {
                    currentPage =
                        GameObject.Instantiate(collectionPage, collectionZone).transform;
                    collectionPages.Add(currentPage.gameObject);
                }
                else currentPage = collectionPages[collectionPages.Count - 1].transform;
                AddCollectionEntry(card.cardType, 1, collectionIndex, currentPage);
                SetCollectionPage(currentPageNum);
            }
            else
            {
                clumpedCollection[card.cardType].Add();
            }
        }
        else
        {
            Card newCardInDeck = Services.CardConfig.CreateCardOfType(card.cardType);
            transitionData.deck.Add(newCardInDeck);
            newCardInDeck.CreatePhysicalCard(deckZone);
            newCardInDeck.controller.EnterDeckBuildingMode();
            SortDeck();
            bool lastCopy = !clumpedCollection[card.cardType].Remove();
            if (lastCopy)
            {
                card.DestroyPhysicalCard();
                clumpedCollection.Remove(card.cardType);
                List<Card.CardType> keys = new List<Card.CardType>(clumpedCollection.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    Card collectionCard = clumpedCollection[keys[i]].card;
                    collectionCard.controller.transform.SetParent(
                        collectionPages[i / (collectionDisplayCols * collectionDisplayRows)]
                        .transform);
                    collectionCard.Reposition(GetCollectionPosition(i), true);
                }
                if(keys.Count ==
                    ((collectionPages.Count-1) * collectionDisplayCols * collectionDisplayRows))
                {
                    Destroy(collectionPages[collectionPages.Count - 1].gameObject);
                    collectionPages.RemoveAt(collectionPages.Count - 1);
                    if (currentPageNum == collectionPages.Count) PrevPage();
                    else SetCollectionPage(currentPageNum);
                }
            }
        }
        AssessDeckReadiness();
    }

    void AssessDeckReadiness()
    {
        deckCounter.text = transitionData.deck.Count + "/" + transitionData.minDeckSize;
        if (transitionData.deck.Count < transitionData.minDeckSize)
        {
            deckCounter.color = Color.yellow;
            submitDeckButton.interactable = false;
        }
        else if (transitionData.deck.Count > transitionData.maxDeckSize)
        {
            deckCounter.color = Color.red;
            submitDeckButton.interactable = false;
        }
        else
        {
            deckCounter.color = Color.green;
            submitDeckButton.interactable = true;
        }
    }

    public void NextPage()
    {
        SetCollectionPage(currentPageNum + 1);
    }

    public void PrevPage()
    {
        SetCollectionPage(currentPageNum - 1);
    }

    public void SubmitDeck()
    {
        TranslateClumpedCollection();
        Services.SceneStackManager.Swap<LevelTransition>(transitionData);
    }

    void TranslateClumpedCollection()
    {
        List<Card> collection = new List<Card>();
        foreach (KeyValuePair<Card.CardType, CardCount> entry in clumpedCollection)
        {
            if (entry.Key != Card.CardType.Punch && entry.Key != Card.CardType.Step)
            {
                for (int i = 0; i < entry.Value.count; i++)
                {
                    collection.Add(Services.CardConfig.CreateCardOfType(entry.Key));
                }
            }
        }
        transitionData.collection = collection;
    }
}
