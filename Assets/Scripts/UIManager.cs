using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject moveCounter;
    public GameObject deckCounter;
    public GameObject deckZone;
    public GameObject inPlayZone;
    public GameObject handZone;
    public GameObject discardCounter;
    public GameObject discardZone;
    [SerializeField]
    private GameObject unitUI;
    [SerializeField]
    private GameObject unitUIRemainingHealthObj;
    [SerializeField]
    private GameObject unitUISpriteObj;
    private Image unitUISprite;
    [SerializeField]
    private GameObject unitUIHealthContainer;
    [SerializeField]
    private GameObject unitUIHealthObj;
    private Image unitUIHealth;
    [SerializeField]
    private GameObject unitUINameObj;
    private Text unitUIName;
    [SerializeField]
    private GameObject unitUIHealthCounterObj;
    private Text unitUIHealthCounter;
    private Vector2 unitUIHealthBarBaseSize;

	// Use this for initialization
	void Awake() {
        InitUnitUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    void InitUnitUI()
    {
        unitUISprite = unitUISpriteObj.GetComponent<Image>();
        unitUIHealth = unitUIHealthObj.GetComponent<Image>();
        unitUIHealthCounter = unitUIHealthCounterObj.GetComponent<Text>();
        unitUIName = unitUINameObj.GetComponent<Text>();
        unitUI.SetActive(false);
        unitUIHealthBarBaseSize = unitUIHealth.GetComponent<RectTransform>().sizeDelta;
    }

    public void UpdateMoveCounter(int movesAvailable)
    {
        moveCounter.GetComponent<Text>().text = "Moves Available: " + movesAvailable;
    }

    public void UpdateDeckCounter(int cardsInDeck)
    {
        deckCounter.GetComponent<Text>().text = cardsInDeck.ToString();
    }

    public void UpdateDiscardCounter(int cardsInDiscard)
    {
        discardCounter.GetComponent<Text>().text = cardsInDiscard.ToString();
    }

    public void SortHand(List<Card> cardsInHand)
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].Reposition(GetCardHandPosition(i), true);
        }
    }

    public Vector3 GetCardHandPosition(int handCountNum)
    {
        return Services.CardConfig.HandCardSpacing * handCountNum;
    }

    public Vector3 GetInPlayCardPosition(int inPlayZoneCountNum)
    {
        return Services.CardConfig.InPlaySpacing * inPlayZoneCountNum;
    }

    public void SortInPlayZone(List<Card> cardsInPlay)
    {
        for (int i = 0; i < cardsInPlay.Count; i++)
        {
            cardsInPlay[i].Reposition(GetInPlayCardPosition(i), true);
        }
    }

    public void ShowUnitUI(string name, int curHP, int maxHP, Sprite sprite)
    {
        unitUI.SetActive(true);
        unitUIName.text = name;
        unitUIHealthCounter.text = curHP + "/" + maxHP;
        unitUISprite.sprite = sprite;
        unitUIHealth.GetComponent<RectTransform>().sizeDelta = new Vector2(
            unitUIHealthBarBaseSize.x * (float)curHP / maxHP,
            unitUIHealthBarBaseSize.y);
        if (curHP == 0) unitUIRemainingHealthObj.SetActive(false);
    }

    public void HideUnitUI()
    {
        unitUI.SetActive(false);
    }
}
