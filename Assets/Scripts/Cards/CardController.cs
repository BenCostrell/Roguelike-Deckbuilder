using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CardController : MonoBehaviour
{
    private Card card;
    private Vector3 basePos;
    public Vector3 baseScale { get; private set; }
    private int baseSortingOrder;
    public SpriteRenderer sr { get; private set; }
    private SpriteRenderer imageSr;
    private TextMesh[] textMeshes;
    private Vector3 mouseRelativePos;
    private BoxCollider2D[] colliders;
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
        if (card.playable)
        {
            if (!Services.GameManager.mouseDown)
            {
                transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
                Reposition(basePos + Services.CardConfig.OnHoverOffset, false);
            }
        }
    }

    private void OnMouseExit()
    {
        if (card.playable && !Services.GameManager.mouseDown)
        {
            DisplayInHand();
        }
        if (card.currentTile != null)
        {
            DisplayCardOnBoard();
        }
    }

    private void OnMouseDown()
    {
        if (card.playable)
        {
            Services.GameManager.mouseDown = true;
            Vector3 mousePos = Services.Main.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseRelativePos = new Vector3(
                mousePos.x - transform.localPosition.x,
                mousePos.y - transform.localPosition.y,
                0);
        }
        if (card.currentTile != null)
        {
            ShowBoardCardOnHover();
        }
    }

    private void OnMouseDrag()
    {
        if (card.playable)
        {
            Vector3 mousePos = Services.Main.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 newPos = new Vector3(
                mousePos.x - mouseRelativePos.x,
                mousePos.y - mouseRelativePos.y,
                basePos.z + Services.CardConfig.OnHoverOffset.z);
            Reposition(newPos, false);
            if (transform.position.y >= Services.CardConfig.CardPlayThresholdYPos
                && card.CanPlay())
            {
                sr.color = Services.CardConfig.PlayableColor;
            }
            else
            {
                sr.color = Color.white;
            }
        }
    }

    private void OnMouseUp()
    {
        Services.GameManager.mouseDown = false;
        if (card.playable && card.CanPlay() &&
            transform.position.y >= Services.CardConfig.CardPlayThresholdYPos)
        {
            Services.GameManager.player.PlayCard(card);
        }
        else if (card.currentTile == null) DisplayInHand();
    }

    public void DisplayInPlay()
    {
        sr.color = Color.white;
        transform.parent = Services.UIManager.inPlayZone.transform;
        transform.localScale = baseScale;
    }

    public void DisplayInHand()
    {
        sr.color = Color.white;
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
        Reposition(basePos + Services.CardConfig.CardOnBoardHoverOffset, false);
        colliders[0].enabled = true;
    }
}
