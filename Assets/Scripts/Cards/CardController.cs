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
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Reposition(Vector3 pos)
    {
        transform.position = pos;
        basePos = pos;
    }

    private void OnMouseEnter()
    {
        if (!Services.GameManager.mouseDown)
        {
            transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
            transform.position = basePos + Services.CardConfig.OnHoverOffset;
            sr.sortingOrder += 1;
        }
    }

    private void OnMouseExit()
    {
        transform.localScale = baseScale;
        transform.position = basePos;
        sr.sortingOrder = baseSortingOrder;
    }

    private void OnMouseDown()
    {
        Services.GameManager.mouseDown = true;
        Vector3 mousePos = Services.Main.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mouseRelativePos = new Vector3(
            mousePos.x - transform.position.x, 
            mousePos.y - transform.position.y, 
            0);
    }

    private void OnMouseDrag()
    {
        Vector3 mousePos = Services.Main.mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 newPos = new Vector3(
            mousePos.x - mouseRelativePos.x, 
            mousePos.y - mouseRelativePos.y, 
            basePos.z + Services.CardConfig.OnHoverOffset.z);
        transform.position = newPos;
        if (transform.position.y >= Services.CardConfig.CardPlayThresholdYPos)
        {
            sr.color = Services.CardConfig.PlayableColor;
        }
        else
        {
            sr.color = Color.white;
        }
    }

    private void OnMouseUp()
    {
        Services.GameManager.mouseDown = false;
        if (transform.position.y >= Services.CardConfig.CardPlayThresholdYPos)
        {
            Services.GameManager.player.PlayCard(card);
        }
        transform.position = basePos;
    }
}
