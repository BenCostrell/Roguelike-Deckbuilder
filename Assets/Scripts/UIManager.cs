using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public Text deckCounter;
    public GameObject deckZone;
    public GameObject inPlayZone;
    public GameObject handZone;
    public Text discardCounter;
    public GameObject discardZone;
    public Text dungeonDeckCounter;
    public GameObject dungeonDeckZone;
    public Text dungeonDiscardCounter;
    public GameObject dungeonDiscardZone;
    public GameObject dungeonPlayZone;
    [SerializeField]
    private Button endTurnButton;
    [SerializeField]
    private Button playAllButton;
    [SerializeField]
    private GameObject unitUI;
    [SerializeField]
    private GameObject unitUIRemainingHealthObj;
    [SerializeField]
    private RectTransform unitUIRemainingHealthBody;
    [SerializeField]
    private Image unitUISprite;
    [SerializeField]
    private Text unitUIName;
    [SerializeField]
    private Text unitUIHealthCounter;
    private Vector2 unitUIHealthBarBaseSize;
    [SerializeField]
    private Text unitUIStats;
    [SerializeField]
    private GameObject playerUI;
    [SerializeField]
    private GameObject playerUIRemainingHealthObj;
    [SerializeField]
    private RectTransform playerUIRemainingHealthBody;
    [SerializeField]
    private Image playerUISprite;
    [SerializeField]
    private GameObject playerUIHealthContainer;
    [SerializeField]
    private Text playerUIHealthCounter;
    [SerializeField]
    private Image playerUIKeyIcon;
    [SerializeField]
    private GameObject shieldIcon;
    [SerializeField]
    private Text shieldCounter;
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
    private List<int> playAllLockIDs;

    // Use this for initialization
    void Awake() {
        InitUI();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    void InitUI()
    {
        unitUI.SetActive(false);
        unitUIHealthBarBaseSize = unitUIRemainingHealthBody.sizeDelta;
        playerUIHealthBarBaseSize = playerUIRemainingHealthBody.sizeDelta;
        playerUIKeyIcon.color = new Color(1, 1, 1, 0.125f);

        endTurnLockIDs = new List<int>();
        playAllLockIDs = new List<int>();

        playerUIKeyIcon.gameObject.SetActive(false);
    }

    public void UpdateDeckCounter(int cardsInDeck)
    {
        deckCounter.text = cardsInDeck.ToString();
    }

    public void UpdateDungeonDeckCounter(int cardsInDeck)
    {
        dungeonDeckCounter.text = cardsInDeck.ToString();
    }

    public void UpdateDiscardCounter(int cardsInDiscard)
    {
        discardCounter.text = cardsInDiscard.ToString();
    }

    public void UpdateDungeonDiscardCounter(int cardsInDiscard)
    {
        dungeonDiscardCounter.text = cardsInDiscard.ToString();
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
        unitUIRemainingHealthBody.sizeDelta = new Vector2(
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
        playerUIRemainingHealthBody.sizeDelta = new Vector2(
            playerUIHealthBarBaseSize.x * (float)curHP / maxHP,
            playerUIHealthBarBaseSize.y);
        if (curHP == 0) playerUIRemainingHealthObj.SetActive(false);
        else playerUIRemainingHealthObj.SetActive(true);
        playerUISprite.sprite = 
            Services.GameManager.player.controller.GetComponent<SpriteRenderer>().sprite;
        if (Services.GameManager.player.hasKey)
            playerUIKeyIcon.color = Color.white;
    }

    public void HideUnitUI()
    {
        unitUI.SetActive(false);
    }

    public void DisableEndTurn(int lockID)
    {
        endTurnLockIDs.Add(lockID);
        endTurnButton.interactable = false;
        
    }

    public void EnableEndTurn(int lockID)
    {
        if (endTurnLockIDs.Contains(lockID))
        {
            endTurnLockIDs.Remove(lockID);
            if (endTurnLockIDs.Count == 0)
            {
                endTurnButton.interactable = true;
            }
        }
    }

    public void ForceUnlockEndTurn()
    {
        endTurnButton.interactable = false;
        endTurnLockIDs = new List<int>();
    }

    public void DisablePlayAll(int lockID)
    {
        playAllLockIDs.Add(lockID);
        playAllButton.interactable = false;
    }

    public void EnablePlayAll(int lockID)
    {
        if (playAllLockIDs.Contains(lockID))
        {
            playAllLockIDs.Remove(lockID);
            if (playAllLockIDs.Count == 0)
            {
                playAllButton.interactable = true;
            }
        }
    }

    public void ForceUnlockPlayAll()
    {
        playAllButton.interactable = true;
        playAllLockIDs = new List<int>();
    }

    public void SetShieldUI(int shieldAmount)
    {
        if (shieldAmount == 0) shieldIcon.SetActive(false);
        else
        {
            shieldIcon.SetActive(true);
            shieldCounter.text = shieldAmount.ToString();
        }
    }
}
