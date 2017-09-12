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
    public TrashController overTrash;
    private bool selected;
    public static Card currentlySelectedCard;

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
        if (selected) Drag();
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
        if (currentlySelectedCard == null)
        {
            if (card.playable)
            {
                if (!Input.GetMouseButton(0))
                {
                    transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                    Reposition(basePos + Services.CardConfig.OnHoverOffset, false);
                    card.OnSelect();
                }
            }
            if (card.deckViewMode && overTrash == null)
            {
                if (!Input.GetMouseButton(0))
                {
                    transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                    Reposition(basePos + Services.CardConfig.OnHoverOffsetDeckViewMode, false);
                }
            }
            if (card.chest != null)
            {
                if (!Input.GetMouseButton(0))
                {
                    transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                    Reposition(basePos + Services.CardConfig.OnHoverOffsetChestMode, false);
                }
            }

            if (Services.GameManager.player.selectingCards)
            {
                OnHoverEnterForCardSelection();
            }
        }
    }

    private void OnMouseExit()
    {
        if (currentlySelectedCard == null)
        {
            if (!selected)
            {
                if (card.playable && !Input.GetMouseButton(0) || card.chest != null)
                {
                    DisplayAtBasePos();
                    card.OnUnselect();
                }
                if (card.deckViewMode && overTrash == null)
                {
                    DisplayInDeckViewMode();
                }
                if (Services.GameManager.player.selectingCards)
                {
                    OnHoverExitForCardSelection();
                }
            }
        }
    }

    private void OnMouseDown()
    {
        if (!selected)
        {
            selected = true;
            currentlySelectedCard = card;
            Vector3 mousePos =
                Services.GameManager.currentCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseRelativePos = new Vector3(
                mousePos.x - transform.localPosition.x,
                mousePos.y - transform.localPosition.y,
                0);
            if (card.playable)
            {
                card.OnSelect();
            }
            if (card.chest != null)
            {
                card.chest.OnCardPicked(card);
                selected = false;
                currentlySelectedCard = null;
            }
            if (Services.GameManager.player.selectingCards)
            {
                Services.EventManager.Fire(new CardSelected(card));
            }
        }
        else
        {
            PlaceCard();
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
                if (transform.localPosition.y >= Services.CardConfig.CardPlayThresholdYPos
                    && card.CanPlay())
                {
                    color = Services.CardConfig.PlayableColor;
                    if(card is TileTargetedCard)
                    {
                        //TileTargetedCard targetedCard = card as TileTargetedCard;
                        //targetedCard.OnSelect();
                        SetCardFrameStatus(false);
                        Services.EventManager.Register<TileSelected>(OnTileSelected);
                    }
                }
                else
                {
                    color = Color.white;
                    if(card is TileTargetedCard)
                    {
                        //TileTargetedCard targetedCard = card as TileTargetedCard;
                        //targetedCard.OnUnselect();
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
        currentlySelectedCard = null;
        if (card.deckViewMode)
        {
            if (overTrash != null) overTrash.PlaceCardInTrash(card);
            else DisplayAtBasePos();
        }
        else TryToPlayCard();
        
    }

    bool TryToPlayCard()
    {
        card.OnUnselect();
        SetCardFrameStatus(true);
        if (card.playable && card.CanPlay() &&
        transform.localPosition.y >= Services.CardConfig.CardPlayThresholdYPos)
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

    public void DisplayCardOnBoard()
    {
        card.Disable();
        sr.enabled = false;
        foreach(TextMesh mesh in textMeshes)
        {
            mesh.gameObject.SetActive(false);
        }
        transform.localScale = baseScale;
        Reposition(basePos, false);
        colliders[0].enabled = false;
    }

    public void ShowBoardCardOnHover()
    {
        sr.enabled = true;
        foreach (TextMesh mesh in textMeshes)
        {
            mesh.gameObject.SetActive(true);
        }
        transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
        Vector3 offset = Services.CardConfig.CardOnBoardHoverOffset;
        Vector3 uiPos = Services.GameManager.currentCamera.ScreenToWorldPoint(
            Services.UIManager.moveCounter.GetComponent<RectTransform>().position);
        if (sr.bounds.min.y - 1 < uiPos.y)
        {
            offset = new Vector3(offset.x, -offset.y + 0.5f, offset.z);
        }
        Reposition(basePos + offset, false);
        colliders[0].enabled = false;
    }

    public void UnselectedForCard()
    {
        color = prevColor;
    }

    public void SelectedForCard()
    {
        color = Color.blue;
    }

    void OnHoverEnterForCardSelection()
    {
        color = (Color.blue + Color.white) / 2;
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
            bool successfullyPlayedCard = TryToPlayCard();
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
        currentlySelectedCard = null;
        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
    }
}
