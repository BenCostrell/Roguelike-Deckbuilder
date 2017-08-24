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
    private GameObject endTurnButtonObj;
    private Button endTurnButton;
    [SerializeField]
    private GameObject unitUI;
    [SerializeField]
    private GameObject unitUIRemainingHealthObj;
    [SerializeField]
    private GameObject unitUIRemainingHealthBody;
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
    [SerializeField]
    private GameObject playerUI;
    [SerializeField]
    private GameObject playerUIRemainingHealthObj;
    [SerializeField]
    private GameObject playerUIRemainingHealthBody;
    [SerializeField]
    private GameObject playerUISpriteObj;
    private Image playerUISprite;
    [SerializeField]
    private GameObject playerUIHealthContainer;
    [SerializeField]
    private GameObject playerUIHealthObj;
    private Image playerUIHealth;
    [SerializeField]
    private GameObject playerUIHealthCounterObj;
    private Text playerUIHealthCounter;
    private Vector2 playerUIHealthBarBaseSize;
    private int nextLockID_;
    public int nextLockID
    {
        get
        {
            nextLockID_ += 1;
            return nextLockID_;
        }
    }
    private int endTurnLockID;
    private bool endTurnLocked;

    // Use this for initialization
    void Awake() {
        InitUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    void InitUI()
    {
        unitUISprite = unitUISpriteObj.GetComponent<Image>();
        unitUIHealth = unitUIHealthObj.GetComponent<Image>();
        unitUIHealthCounter = unitUIHealthCounterObj.GetComponent<Text>();
        unitUIName = unitUINameObj.GetComponent<Text>();
        unitUI.SetActive(false);
        unitUIHealthBarBaseSize = 
            unitUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta;

        playerUISprite = playerUISpriteObj.GetComponent<Image>();
        playerUIHealth = playerUIHealthObj.GetComponent<Image>();
        playerUIHealthCounter = playerUIHealthCounterObj.GetComponent<Text>();
        playerUIHealthBarBaseSize = 
            playerUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta;

        endTurnButton = endTurnButtonObj.GetComponent<Button>();
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
        unitUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta = new Vector2(
            unitUIHealthBarBaseSize.x * (float)curHP / maxHP,
            unitUIHealthBarBaseSize.y);
        if (curHP == 0) unitUIRemainingHealthObj.SetActive(false);
    }

    public void UpdatePlayerUI(int curHP, int maxHP)
    {
        playerUIHealthCounter.text = curHP + "/" + maxHP;
        playerUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta = new Vector2(
            playerUIHealthBarBaseSize.x * (float)curHP / maxHP,
            playerUIHealthBarBaseSize.y);
        if (curHP == 0) playerUIRemainingHealthObj.SetActive(false);
        else playerUIRemainingHealthObj.SetActive(true);
        playerUISprite.sprite = Services.GameManager.player.controller.GetComponent<SpriteRenderer>().sprite;
    }

    public void HideUnitUI()
    {
        unitUI.SetActive(false);
    }

    public void DisableEndTurn(int lockID)
    {
        if (!endTurnLocked)
        {
            endTurnLocked = true;
            endTurnLockID = lockID;
            endTurnButton.enabled = false;
        }
    }

    public void EnableEndTurn(int lockID)
    {
        if (lockID == endTurnLockID && endTurnLocked)
        {
            endTurnLocked = false;
            endTurnButton.enabled = true;
        }
    }
}
