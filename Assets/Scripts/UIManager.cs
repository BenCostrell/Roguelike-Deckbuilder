﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameObject deckCounter;
    public GameObject deckZone;
    public GameObject inPlayZone;
    public GameObject handZone;
    public GameObject discardCounter;
    public GameObject discardZone;
    public GameObject dungeonDeckCounter;
    public GameObject dungeonDeckZone;
    public GameObject dungeonDiscardCounter;
    public GameObject dungeonDiscardZone;
    public GameObject dungeonPlayZone;
    [SerializeField]
    private GameObject endTurnButtonObj;
    private Button endTurnButton;
    [SerializeField]
    private GameObject playAllButtonObj;
    private Button playAllButton;
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
    private GameObject unitUIStatsObj;
    private Text unitUIStats;
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
    [SerializeField]
    private GameObject playerUIKeyIcon;
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
    private List<int> endTurnLockIDs;
    private bool endTurnLocked;
    private List<int> playAllLockIDs;
    private bool playAllLocked;

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
        unitUIStats = unitUIStatsObj.GetComponent<Text>();

        playerUISprite = playerUISpriteObj.GetComponent<Image>();
        playerUIHealth = playerUIHealthObj.GetComponent<Image>();
        playerUIHealthCounter = playerUIHealthCounterObj.GetComponent<Text>();
        playerUIHealthBarBaseSize = 
            playerUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta;
        playerUIKeyIcon.GetComponent<Image>().color = new Color(1, 1, 1, 0.125f);

        endTurnButton = endTurnButtonObj.GetComponent<Button>();
        playAllButton = playAllButtonObj.GetComponent<Button>();

        endTurnLockIDs = new List<int>();
        playAllLockIDs = new List<int>();
    }

    public void UpdateDeckCounter(int cardsInDeck)
    {
        deckCounter.GetComponent<Text>().text = cardsInDeck.ToString();
    }

    public void UpdateDungeonDeckCounter(int cardsInDeck)
    {
        dungeonDeckCounter.GetComponent<Text>().text = cardsInDeck.ToString();
    }

    public void UpdateDiscardCounter(int cardsInDiscard)
    {
        discardCounter.GetComponent<Text>().text = cardsInDiscard.ToString();
    }

    public void UpdateDungeonDiscardCounter(int cardsInDiscard)
    {
        dungeonDiscardCounter.GetComponent<Text>().text = cardsInDiscard.ToString();
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

    public void ShowUnitUI(Monster monster)
    {
        unitUI.SetActive(true);
        if (monster.currentHealth == 0) unitUIRemainingHealthObj.SetActive(false);
        else unitUIRemainingHealthObj.SetActive(true);
        unitUIName.text = monster.info.Name;
        unitUIHealthCounter.text = monster.currentHealth + "/" + monster.maxHealth;
        unitUISprite.sprite = monster.info.Sprite;
        unitUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta = new Vector2(
            unitUIHealthBarBaseSize.x * (float)monster.currentHealth / monster.maxHealth,
            unitUIHealthBarBaseSize.y);
        unitUIStats.text =
            "ATTACK " + monster.attackDamage + "\n" +
            "MOVE " + monster.movementSpeed + "\n" +
            "RANGE " + monster.attackRange;
    }

    public void UpdatePlayerUI(int curHP, int maxHP)
    {
        playerUIHealthCounter.text = curHP + "/" + maxHP;
        playerUIRemainingHealthBody.GetComponent<RectTransform>().sizeDelta = new Vector2(
            playerUIHealthBarBaseSize.x * (float)curHP / maxHP,
            playerUIHealthBarBaseSize.y);
        if (curHP == 0) playerUIRemainingHealthObj.SetActive(false);
        else playerUIRemainingHealthObj.SetActive(true);
        playerUISprite.sprite = 
            Services.GameManager.player.controller.GetComponent<SpriteRenderer>().sprite;
        if (Services.GameManager.player.hasKey)
            playerUIKeyIcon.GetComponent<Image>().color = Color.white;
    }

    public void HideUnitUI()
    {
        unitUI.SetActive(false);
    }

    public void DisableEndTurn(int lockID)
    {
        endTurnLocked = true;
        endTurnLockIDs.Add(lockID);
        endTurnButton.enabled = false;
        
    }

    public void EnableEndTurn(int lockID)
    {
        if (endTurnLockIDs.Contains(lockID))
        {
            endTurnLockIDs.Remove(lockID);
            if (endTurnLockIDs.Count == 0)
            {
                endTurnLocked = false;
                endTurnButton.enabled = true;
            }
        }
    }

    public void ForceUnlockEndTurn()
    {
        endTurnLocked = false;
        endTurnButton.enabled = true;
        endTurnLockIDs = new List<int>();
    }

    public void DisablePlayAll(int lockID)
    {
        playAllLocked = true;
        playAllLockIDs.Add(lockID);
        playAllButton.enabled = false;
    }

    public bool EnablePlayAll(int lockID)
    {
        if (playAllLockIDs.Contains(lockID))
        {
            playAllLockIDs.Remove(lockID);
            if (playAllLockIDs.Count == 0)
            {
                playAllLocked = false;
                playAllButton.enabled = true;
            }
        }
        return playAllLocked;
    }

    public void ForceUnlockPlayAll()
    {
        playAllLocked = false;
        playAllButton.enabled = true;
        playAllLockIDs = new List<int>();
    }
}
