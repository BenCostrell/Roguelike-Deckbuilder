using UnityEngine;
using System.Collections;

public class CardController : MonoBehaviour
{
    private Card card;
    private Vector3 basePos;
    private Vector3 baseScale;
    private int baseSortingOrder;
    private SpriteRenderer sr;
    private TextMesh[] textMeshes;
    private Vector3 mouseRelativePos;
    // Use this for initialization
    public void Init(Card card_)
    {
        card = card_;
        textMeshes = GetComponentsInChildren<TextMesh>();
        textMeshes[0].text = card.info.Name;
        textMeshes[1].text = card.info.CardText;
        baseScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
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
        transform.position = pos;
        if (changeBasePos) basePos = pos;
        sr.sortingOrder = baseSortingOrder - Mathf.CeilToInt(pos.z/2);
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
        if (card.playable)
        {
            DisplayInHand();
        }
    }

    private void OnMouseDown()
    {
        if (card.playable)
        {
            Services.GameManager.mouseDown = true;
            Vector3 mousePos = Services.Main.mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseRelativePos = new Vector3(
                mousePos.x - transform.position.x,
                mousePos.y - transform.position.y,
                0);
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
        else DisplayInHand();
    }

    public void DisplayInPlay()
    {
        sr.color = Color.white;
        transform.localScale = baseScale;
    }

    public void DisplayInHand()
    {
        sr.color = Color.white;
        transform.localScale = baseScale;
        Reposition(basePos, false);
    }
}
