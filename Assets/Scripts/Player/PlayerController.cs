using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private Player player;
    private LineRenderer lr;
    private GameObject arrowhead;
    [SerializeField]
    private GameObject healthUIobj;
    private TextMesh healthUI;
    private SpriteRenderer sr;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(Player player_)
    {
        player = player_;
        lr = GetComponentInChildren<LineRenderer>();
        sr = GetComponent<SpriteRenderer>();
        arrowhead = lr.gameObject;
        arrowhead.SetActive(false);
        healthUI = healthUIobj.GetComponent<TextMesh>();
        MeshRenderer mr = healthUIobj.GetComponent<MeshRenderer>();
        mr.sortingLayerID = sr.sortingLayerID;
        mr.sortingOrder = sr.sortingOrder;
    }

    public void UpdateHealthUI()
    {
        healthUI.text = player.currentHealth + "/" + player.maxHealth;
    }


    public void PlaceOnTile(Tile tile)
    {
        transform.position = new Vector3(tile.coord.x, tile.coord.y, 0);
    }

    public void ShowPathArrow(List<Tile> path)
    {
        if (path.Count > 0)
        {
            path.Add(player.currentTile);
            arrowhead.SetActive(true);
            Vector3[] positions = new Vector3[path.Count];
            for (int i = 0; i < path.Count; i++)
            {
                Vector3 basePos = path[i].coord.ScreenPos();
                positions[i] = new Vector3(basePos.x, basePos.y, -1);
            }
            lr.positionCount = positions.Length;
            lr.SetPositions(positions);
            Vector3 arrowheadBasePos = path[0].coord.ScreenPos();
            arrowhead.transform.position = 
                new Vector3(arrowheadBasePos.x, arrowheadBasePos.y, -2);
            Coord direction = Coord.Subtract(path[0].coord, path[1].coord);
            float angle = Coord.DirectionToAngle(direction) - 210;
            arrowhead.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            arrowhead.SetActive(false);
        }
    }

    public void HideArrow()
    {
        arrowhead.SetActive(false);
    }
}
