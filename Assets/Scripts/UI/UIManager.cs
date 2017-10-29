using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private Sprite cardBackSprite;
    [SerializeField]
    private Vector2 pileCardSpacing;
    [SerializeField]
    private Text deckCounter;
    public GameObject deckZone;
    public GameObject inPlayZone;
    public GameObject handZone;
    public Transform chestCardArea;
    [SerializeField]
    private Image chestAreaImage;
    public Transform bottomLeft;
    [SerializeField]
    private Text discardCounter;
    public GameObject discardZone;
    [SerializeField]
    private Text dungeonDeckCounter;
    public GameObject dungeonDeckZone;
    [SerializeField]
    private Text dungeonDiscardCounter;
    public GameObject dungeonDiscardZone;
    public GameObject dungeonPlayZone;
    [SerializeField]
    private Image dungeonTimer;
    [SerializeField]
    private Button endTurnButton;
    [SerializeField]
    private Button queueButton;
    private Text queueButtonText;
    [SerializeField]
    private Color queueColor;
    [SerializeField]
    private Color unqueueColor;
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
    [SerializeField]
    private GameObject levelCompleteUI;
    [SerializeField]
    private GameObject levelCompleteText;
    [SerializeField]
    private Image messageBanner;
    [SerializeField]
    private Text messageBannerText;
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
    public Vector3 dungeonTimerPos { get { return dungeonTimer.transform.position; } }
    public Vector3 dungeonDeckPos { get { return dungeonDeckZone.transform.position; } }
    public float bannerScrollDuration;
    public string startBannerMessage;
    public string playerTurnMessage;
    public string dungeonTurnMessage;

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
        ToggleChestArea(false);
        unitUIHealthBarBaseSize = unitUIRemainingHealthBody.sizeDelta;
        playerUIHealthBarBaseSize = playerUIRemainingHealthBody.sizeDelta;
        playerUIKeyIcon.color = new Color(1, 1, 1, 0.125f);
        queueButtonText = queueButton.GetComponentInChildren<Text>();

        endTurnLockIDs = new List<int>();
        playAllLockIDs = new List<int>();

        playerUIKeyIcon.gameObject.SetActive(false);
        levelCompleteUI.gameObject.SetActive(false);
    }

    public void SetMessageBanner(string bannerText)
    {
        messageBannerText.text = bannerText;
        messageBanner.gameObject.SetActive(true);
    }

    public void TurnOffMessageBanner()
    {
        messageBanner.gameObject.SetActive(false);
    }

    public void MoveMessageBanner(Vector2 pos)
    {
        messageBanner.rectTransform.anchoredPosition = pos;
    }

    public void UpdateDeckCounter(int cardsInDeck)
    {
        deckCounter.text = cardsInDeck.ToString();
        CreateCardPileUI(cardsInDeck, deckZone.transform);
    }

    public void UpdateDungeonDeckCounter(int cardsInDeck)
    {
        dungeonDeckCounter.text = cardsInDeck.ToString();
        CreateCardPileUI(cardsInDeck, dungeonDeckZone.transform);
    }

    public void UpdateDiscardCounter(int cardsInDiscard)
    {
        discardCounter.text = cardsInDiscard.ToString();
        CreateCardPileUI(cardsInDiscard, discardZone.transform);
    }

    public void UpdateDungeonDiscardCounter(int cardsInDiscard)
    {
        dungeonDiscardCounter.text = cardsInDiscard.ToString();
        CreateCardPileUI(cardsInDiscard, dungeonDiscardZone.transform);
    }

    void CreateCardPileUI(int cardCount, Transform zone)
    {
        Transform pileObjectZone = zone.GetChild(0);
        Image baseImage = zone.GetComponent<Image>();
        Image[] currentPileUIUnits = pileObjectZone.GetComponentsInChildren<Image>();
        int numCurrentUnits = currentPileUIUnits.Length;
        int targetUnitCount = cardCount - 1;
        int unitsDestroyed = 0;
        if (numCurrentUnits > targetUnitCount)
        {
            for (int i = 0; i < currentPileUIUnits.Length; i++)
            {
                Destroy(currentPileUIUnits[i].gameObject);
                unitsDestroyed += 1;
                if (unitsDestroyed == (numCurrentUnits - targetUnitCount)) break;
            }
        }
        else if(targetUnitCount > 0)
        {
            Vector2 size = baseImage.GetComponent<RectTransform>().sizeDelta;
            for (int i = numCurrentUnits - 1; i < targetUnitCount; i++)
            {
                GameObject obj = new GameObject();
                RectTransform rect = obj.AddComponent<RectTransform>();
                Image img = obj.AddComponent<Image>();
                obj.transform.SetParent(pileObjectZone);
                rect.sizeDelta = size;
                img.sprite = cardBackSprite;
                rect.localScale = Vector3.one;
                rect.SetAsFirstSibling();
                if (zone == dungeonDeckZone.transform || zone == dungeonDiscardZone.transform)
                {
                    img.color = baseImage.color;
                }                
                rect.anchoredPosition = i * pileCardSpacing;
            }
        }
        
    }

    public void SortHand(List<Card> cardsInHand)
    {
        for (int i = 0; i < cardsInHand.Count; i++)
        {
            cardsInHand[i].Reposition(GetHandCardPosition(i, cardsInHand.Count), true);
            cardsInHand[i].controller.baseSiblingIndex = 
                cardsInHand[i].controller.transform.GetSiblingIndex();
            cardsInHand[i].controller.transform.localRotation = 
                GetHandCardRotation(i, cardsInHand.Count);
                
        }
    }

    public Vector3 GetHandCardPosition(int handCountIndex, int handCount)
    {
        return new Vector3(Services.CardConfig.HandCardSpacing.x * handCountIndex,
            Services.CardConfig.HandCardSpacing.y 
            * Mathf.Cos(Services.CardConfig.HandCardFanFactor *
            GetHandCardRotation(handCountIndex, handCount).eulerAngles.z 
            * Mathf.Deg2Rad), 0);
    }

    public Quaternion GetHandCardRotation(int handCountIndex, int handCount)
    {
        return Quaternion.Euler(0, 0,
                -(handCountIndex - handCount / 2) * Services.CardConfig.HandCardRotation);
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
            cardsInPlay[i].controller.transform.SetSiblingIndex(i);
            cardsInPlay[i].controller.baseSiblingIndex = i;
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
        queueButton.interactable = false;
    }

    public void EnablePlayAll(int lockID)
    {
        if (playAllLockIDs.Contains(lockID))
        {
            playAllLockIDs.Remove(lockID);
            if (playAllLockIDs.Count == 0)
            {
                queueButton.interactable = true;
            }
        }
    }

    public void ForceUnlockPlayAll()
    {
        queueButton.interactable = true;
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

    public void UpdateDungeonTimer(float fillAmt)
    {
        dungeonTimer.fillAmount = fillAmt;
    }

    public void ToggleLevelComplete(bool status)
    {
        levelCompleteUI.SetActive(status);
    }

    public void ToggleLevelCompleteText(bool status)
    {
        levelCompleteText.SetActive(status);
    }

    public void ToggleChestArea(bool status)
    {
        chestAreaImage.gameObject.SetActive(status);
    }

    public void SetQueueButtonStatus(bool queue)
    {
        if (queue)
        {
            queueButtonText.text = "QUEUE ALL MOVEMENT";
        }
        else queueButtonText.text = "UNQUEUE ALL MOVEMENT";
    }
}
