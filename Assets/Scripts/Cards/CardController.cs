using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerClickHandler,IPointerEnterHandler,IPointerExitHandler
{
    public Card card { get; private set; }
    private Vector3 basePos;
    [HideInInspector]
    public Vector3 baseScale;
    private Transform baseParent;
    private RectTransform rect;
    public Image frame { get; private set; }
    public Color color
    {
        get
        {
            return frame.color;
        }
        set
        {
            prevColor = frame.color;
            frame.color = value;
        }
    }
    public Color prevColor { get; private set; }
    private Image art;
    private Vector3 artBaseLocalPos;
    private Text nameText;
    private Text effectText;
    private Vector3 mouseRelativePos;
    public bool selected;
    public static List<Card> currentlySelectedCards = new List<Card>();
    private bool inDiscardZone;
    private bool selectedLastFrame;
    private Player player { get { return Services.GameManager.player; } }
    public int baseSiblingIndex;

    // Use this for initialization
    public void Init(Card card_)
    {
        card = card_;
        baseParent = transform.parent;
        rect = GetComponent<RectTransform>();
        Text[] textElements = GetComponentsInChildren<Text>();
        Image[] imageElements = GetComponentsInChildren<Image>();
        nameText = textElements[0];
        effectText = textElements[1];
        frame = imageElements[1];
        art = imageElements[2];
        nameText.text = card.info.Name;
        effectText.text = card.info.CardText;
        art.sprite = card.sprite;
        artBaseLocalPos = art.transform.localPosition;
        baseScale = transform.localScale;
        baseSiblingIndex = rect.GetSiblingIndex();
    }

    // Update is called once per frame
    void Update()
    {
        if (selected && !(card is MovementCard))
        {
            Drag();
            if (Input.GetMouseButtonDown(0) && selectedLastFrame) PlaceCard();
        }
        selectedLastFrame = selected;
    }

    public void Reposition(Vector3 pos, bool changeBasePos)
    {
        Reposition(pos, changeBasePos, false);
    }

    public void Reposition(Vector3 pos, bool changeBasePos, bool front)
    {
        transform.localPosition= pos;
        if (changeBasePos) {
            basePos = pos;
        }
        if (front) rect.SetAsLastSibling();
        else rect.SetSiblingIndex(baseSiblingIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (((currentlySelectedCards.Count == 0) ||
            (player.movementCardsSelected.Count != 0)) && 
            !Input.GetMouseButton(0))
        {
            if (card.deckViewMode || card.collectionMode)
            {

                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Vector3 offset = Services.CardConfig.OnHoverOffsetDeckViewMode;
                if(transform.position.y < 200)
                {
                    offset = new Vector3(offset.x, -offset.y, offset.z);
                }
                Reposition(basePos + offset, false, true);

            }
            else if (card.playable && !selected)
            {
                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Reposition(basePos + Services.CardConfig.OnHoverOffset, false, true);
                if(!(card is MovementCard)) card.OnSelect();
            }
            else if (card.chest != null)
            {
                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Reposition(basePos + Services.CardConfig.OnHoverOffsetChestMode, false, true);

            }
            if (player.selectingCards)
            {
                OnHoverEnterForCardSelection();
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!selected)
        {
            if (player.selectingCards)
            {
                OnHoverExitForCardSelection();
            }
            else if (card.deckViewMode || card.collectionMode)
            {
                DisplayInDeckViewMode();
            }
            else if (card.playable && !Input.GetMouseButton(0))
            {
                DisplayAtBasePos();
                if(!(card is MovementCard)) card.OnUnselect();
            }
            else if (card.chest != null)
            {
                DisplayAtBasePos();
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (card.deckViewMode) { } //nothing right now
        else if (card.collectionMode)
        {
            Services.DeckConstruction.OnCardClicked(card);
        }
        else if (player.selectingCards) {
            Services.EventManager.Fire(new CardSelected(card));
        }
        else if (!selected)
        {
            SelectCard();
        }
        else if (card is MovementCard)
        {
            UnselectMovementCard();
        }
    }

    public void UnselectMovementCard()
    {
        DisplayAtBasePos();
        card.OnUnselect();
        currentlySelectedCards.Remove(card);
        selected = false;
    }

    public void SelectCard()
    {
        selected = true;
        currentlySelectedCards.Add(card);
        Vector3 mousePos = Input.mousePosition;
        mouseRelativePos = mousePos - transform.position;
        if (card is MovementCard && card.playable)
        {
            color = (Color.blue + Color.white) / 2;
            transform.localScale = baseScale * 1.1f;
            Reposition(basePos + Services.CardConfig.OnHoverOffset, false, true);
        }
        else
        {
            transform.SetParent(Services.UIManager.bottomCorner);
        }
        if (card.playable)
        {
            card.OnSelect();
        }
        if (card.chest != null)
        {
            card.chest.OnCardPicked(card);
            selected = false;
            currentlySelectedCards.Remove(card);
        }

    }

    void Drag()
    {
        transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
        if (card.playable || card.deckViewMode)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 newPos = mousePos - mouseRelativePos;
            Reposition(newPos, false, true);
            if (card.playable)
            {
                if (IsInDiscardZone())
                {
                    color = Color.red;
                    if (card is TileTargetedCard)
                    {
                        transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
                        SetCardFrameStatus(true);
                        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
                    }
                }
                else if (rect.anchoredPosition.y >= Services.CardConfig.CardPlayThresholdYPos
                    && card.CanPlay())
                {
                    color = Services.CardConfig.PlayableColor;
                    if (card is TileTargetedCard)
                    {
                        transform.localScale = baseScale;
                        SetCardFrameStatus(false);
                        Services.EventManager.Register<TileSelected>(OnTileSelected);
                    }
                }
                else
                {
                    color = Color.white;
                    if (card is TileTargetedCard)
                    {
                        transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
                        SetCardFrameStatus(true);
                        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
                    }
                }
            }
        }
    }

    void SetCardFrameStatus(bool status)
    {
        nameText.enabled = status;
        effectText.enabled = status;
        frame.enabled = status;
        if (status)
        {
            art.GetComponent<RectTransform>().anchoredPosition = artBaseLocalPos;
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 mouseLocalPos = rect.InverseTransformPoint(mousePosition);
            art.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                mouseLocalPos.x,
                mouseLocalPos.y);
        }
    }

    void PlaceCard()
    {
        selected = false;
        currentlySelectedCards.Remove(card);
        TryToPlayCard(false);
        
    }

    public bool TryToPlayCard(bool forcePlay)
    {
        card.OnUnselect();
        SetCardFrameStatus(true);
        if (IsInDiscardZone())
        {
            player.DiscardCardFromHand(card);
            return false;
        }
        else if (card.playable && card.CanPlay() && (forcePlay ||
            (rect.anchoredPosition.y >= Services.CardConfig.CardPlayThresholdYPos &&
            !(card is TileTargetedCard))))
        {
            Services.Main.taskManager.AddTask(player.PlayCard(card));
            return true;
        }
        else
        {
            DisplayAtBasePos();
            return false;
        }
    }

    public void DisplayInPlay()
    {
        color = Color.white;
        transform.SetParent(Services.UIManager.inPlayZone.transform);
        transform.localScale = baseScale;
    }

    public void DisplayAtBasePos()
    {
        color = Color.white;
        transform.localScale = baseScale;
        transform.SetParent(baseParent);
        Reposition(basePos, false);
    }

    public void DisplayInDeckViewMode()
    {
        transform.localScale = baseScale;
        Reposition(basePos, false);
    }

    public void UnselectedForCard()
    {
        color = prevColor;
    }

    public void SelectedForCard()
    {
        color = Color.red;
    }

    void OnHoverEnterForCardSelection()
    {
        color = (Color.red + Color.white) / 2;
    }

    void OnHoverExitForCardSelection()
    {
        color = prevColor;
    }

    public void OnTileSelected(TileSelected e)
    {
        Tile tileSelected = e.tile;
        TileTargetedCard targetedCard = card as TileTargetedCard;
        if (targetedCard.IsTargetValid(tileSelected))
        {
            bool successfullyPlayedCard = TryToPlayCard(true);
            if (successfullyPlayedCard)
            {
                targetedCard.OnTargetSelected(tileSelected);
            }
        }
        else
        {
            card.OnUnselect();
            SetCardFrameStatus(true);
            DisplayAtBasePos();
        }
        selected = false;
        currentlySelectedCards.Remove(card);
        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
    }

    public void OnCardDisable()
    {
        color = Color.gray;
        color = Color.gray; //looks silly but it's so gray becomes the "base color"
    }

    bool IsInDiscardZone()
    {
        RectTransform discardRT =
                    Services.UIManager.discardZone.GetComponent<RectTransform>();
        return discardRT.rect.Contains(discardRT.InverseTransformPoint(transform.position));
    }
}
