using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CardController : MonoBehaviour
{
    public Card card { get; private set; }
    private Vector3 basePos;
    public Vector3 baseScale;
    private int baseSortingOrder;
    public SpriteRenderer sr { get; private set; }
    public Color color
    {
        get
        {
            return sr.color;
        }
        set
        {
            prevColor = sr.color;
            sr.color = value;
        }
    }
    public Color prevColor { get; private set; }
    private SpriteRenderer imageSr;
    private Vector3 imageBaseLocalPos;
    private TextMesh[] textMeshes;
    private Vector3 mouseRelativePos;
    private BoxCollider2D[] colliders;
    public bool selected;
    public static List<Card> currentlySelectedCards = new List<Card>();
    private bool inDiscardZone;
    private bool selectedLastFrame;

    // Use this for initialization
    public void Init(Card card_)
    {
        card = card_;
        colliders = GetComponents<BoxCollider2D>();
        textMeshes = GetComponentsInChildren<TextMesh>();
        textMeshes[0].text = card.info.Name;
        textMeshes[1].text = card.info.CardText;
        baseScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
        imageSr = GetComponentsInChildren<SpriteRenderer>()[1];
        imageSr.sprite = card.sprite;
        imageBaseLocalPos = imageSr.transform.localPosition;
        baseSortingOrder = sr.sortingOrder;
        foreach(TextMesh mesh in textMeshes)
        {
            mesh.gameObject.GetComponent<MeshRenderer>().sortingLayerID = sr.sortingLayerID;
            mesh.gameObject.GetComponent<MeshRenderer>().sortingOrder = sr.sortingOrder + 1;
        }
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
        transform.localPosition = pos;
        if (changeBasePos) basePos = pos;
        sr.sortingOrder = baseSortingOrder + Mathf.CeilToInt(-transform.position.z/2);
        imageSr.sortingOrder = sr.sortingOrder + 1;
        foreach (TextMesh mesh in textMeshes)
        {
            mesh.gameObject.GetComponent<MeshRenderer>().sortingOrder = sr.sortingOrder + 1;
        }
    }

    private void OnMouseEnter()
    {
        if (((currentlySelectedCards.Count == 0) ||
            (Services.GameManager.player.movementCardsSelected.Count != 0)) && 
            !Input.GetMouseButton(0))
        {
            if (card.deckViewMode || card.collectionMode)
            {

                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Reposition(basePos + Services.CardConfig.OnHoverOffsetDeckViewMode, false);

            }
            else if (card.playable && !selected)
            {
                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Reposition(basePos + Services.CardConfig.OnHoverOffset, false);
                if(!(card is MovementCard)) card.OnSelect();
            }
            else if (card.chest != null)
            {
                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Reposition(basePos + Services.CardConfig.OnHoverOffsetChestMode, false);

            }
            if (Services.GameManager.player.selectingCards)
            {
                OnHoverEnterForCardSelection();
            }
        }
    }

    private void OnMouseExit()
    {
        if (!selected)
        {
            if (card.deckViewMode || card.collectionMode)
            {
                DisplayInDeckViewMode();
            }
            else if (card.playable && !Input.GetMouseButton(0) || card.chest != null)
            {
                DisplayAtBasePos();
                if(!(card is MovementCard)) card.OnUnselect();
            }
            if (Services.GameManager.player.selectingCards)
            {
                OnHoverExitForCardSelection();
            }

        }
    }

    private void OnMouseDown()
    {
        if (card.deckViewMode) { } //nothing right now
        else if (card.collectionMode)
        {
            Services.DeckConstruction.OnCardClicked(card);
        }
        else if (Services.GameManager.player.selectingCards) {
            Services.EventManager.Fire(new CardSelected(card));
        }
        else if (!selected)
        {
            SelectCard();
        }
        else if (card is MovementCard)
        {
            DisplayAtBasePos();
            card.OnUnselect();
            currentlySelectedCards.Remove(card);
            selected = false;
        }
    }

    public void SelectCard()
    {
        selected = true;
        currentlySelectedCards.Add(card);
        Vector3 mousePos =
            Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseRelativePos = new Vector3(
            mousePos.x - transform.localPosition.x,
            mousePos.y - transform.localPosition.y,
            0);
        if (card is MovementCard && card.playable)
        {
            color = (Color.blue + Color.white) / 2;
            transform.localScale = baseScale * 1.1f;
            Reposition(basePos + Services.CardConfig.OnHoverOffset, false);
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
            Vector3 mousePos = 
                Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = new Vector3(
                mousePos.x - mouseRelativePos.x,
                mousePos.y - mouseRelativePos.y,
                basePos.z);
            if (card.deckViewMode)
            {
                newPos = new Vector3(
                    newPos.x, 
                    newPos.y, 
                    Services.CardConfig.OnHoverOffsetDeckViewMode.z);
            }
            else
            {
                newPos += new Vector3(0, 0, Services.CardConfig.OnHoverOffset.z);
            }
                Reposition(newPos, false);
            if (card.playable)
            {
                if (Services.UIManager.discardZone.GetComponent<Collider2D>().bounds.Contains(transform.position))
                {
                    color = Color.red;
                    if (card is TileTargetedCard)
                    {
                        SetCardFrameStatus(true);
                        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
                    }
                }
                else if (transform.localPosition.y >= Services.CardConfig.CardPlayThresholdYPos
                    && card.CanPlay())
                {
                    color = Services.CardConfig.PlayableColor;
                    if (card is TileTargetedCard)
                    {
                        SetCardFrameStatus(false);
                        Services.EventManager.Register<TileSelected>(OnTileSelected);
                    }
                }
                else
                {
                    color = Color.white;
                    if (card is TileTargetedCard)
                    {
                        SetCardFrameStatus(true);
                        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
                    }
                }
            }
        }
    }

    void SetCardFrameStatus(bool status)
    {
        foreach (TextMesh tm in textMeshes)
        {
            tm.gameObject.SetActive(status);
        }
        sr.enabled = status;
        colliders[0].enabled = status;
        if (status)
        {
            imageSr.transform.localPosition = imageBaseLocalPos;
        }
        else
        {
            Vector3 mousePosition = 
                Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 mouseLocalPos = transform.InverseTransformPoint(mousePosition);
            imageSr.transform.localPosition = new Vector3(
                mouseLocalPos.x,
                mouseLocalPos.y,
                0);
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
        if (Services.UIManager.discardZone.GetComponent<Collider2D>().bounds.Contains(transform.position))
        {
            Services.GameManager.player.DiscardCardFromHand(card);
            return false;
        }
        else if (card.playable && card.CanPlay() && (forcePlay ||
            (transform.localPosition.y >= Services.CardConfig.CardPlayThresholdYPos &&
            !(card is TileTargetedCard))))
        {
            Services.Main.taskManager.AddTask(Services.GameManager.player.PlayCard(card));
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
        transform.parent = Services.UIManager.inPlayZone.transform;
        transform.localScale = baseScale;
    }

    public void DisplayAtBasePos()
    {
        color = Color.white;
        transform.localScale = baseScale;
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
}
