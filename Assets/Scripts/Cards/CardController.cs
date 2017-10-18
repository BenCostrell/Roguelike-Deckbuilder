using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Card card { get; private set; }
    private Vector3 basePos;
    [HideInInspector]
    public Vector3 baseScale;
    private Transform baseParent;
    private RectTransform rect;
    public Image frame { get; private set; }
    public Color color
    {
        get
        {
            return frame.color;
        }
        set
        {
            prevColor = frame.color;
            frame.color = value;
        }
    }
    public Color prevColor { get; private set; }
    private Image art;
    private Vector3 artBaseLocalPos;
    private Text nameText;
    private Text effectText;
    private Vector3 mouseRelativePos;
    public bool selected;
    public static List<Card> currentlySelectedCards = new List<Card>();
    private bool inDiscardZone;
    private bool selectedLastFrame;
    private Player player { get { return Services.GameManager.player; } }
    public int baseSiblingIndex;
    private FSM<CardController> stateMachine;

    // Use this for initialization
    public void Init(Card card_)
    {
        card = card_;
        baseParent = transform.parent;
        rect = GetComponent<RectTransform>();
        Text[] textElements = GetComponentsInChildren<Text>();
        Image[] imageElements = GetComponentsInChildren<Image>();
        nameText = textElements[0];
        effectText = textElements[1];
        frame = imageElements[1];
        art = imageElements[2];
        nameText.text = card.info.Name;
        effectText.text = card.info.CardText;
        art.sprite = card.sprite;
        artBaseLocalPos = art.transform.localPosition;
        baseScale = transform.localScale;
        baseSiblingIndex = rect.GetSiblingIndex();
        stateMachine = new FSM<CardController>(this);
        //send machine to first state
        stateMachine.InitializeState<Disabled>();
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
        //if (selected && !(card is MovementCard))
        //{
        //    Drag();
        //    if (Input.GetMouseButtonDown(0) && selectedLastFrame) PlaceCard();
        //}
        //selectedLastFrame = selected;
    }

    public void Reposition(Vector3 pos, bool changeBasePos)
    {
        Reposition(pos, changeBasePos, false);
    }

    public void Reposition(Vector3 pos, bool changeBasePos, bool front)
    {
        transform.localPosition= pos;
        if (changeBasePos) {
            basePos = pos;
        }
        if (front) rect.SetAsLastSibling();
        else rect.SetSiblingIndex(baseSiblingIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        stateMachine.OnInputEnter();
        //if (((player.cardsSelected.Count == 0) ||
        //    (player.movementCardsSelected.Count != 0)) && 
        //    !Input.GetMouseButton(0))
        //{
        //    if (card.deckViewMode || card.collectionMode)
        //    {

        //        transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
        //        Vector3 offset = Services.CardConfig.OnHoverOffsetDeckViewMode;
        //        if(transform.position.y < 200)
        //        {
        //            offset = new Vector3(offset.x, -offset.y, offset.z);
        //        }
        //        Reposition(basePos + offset, false, true);

        //    }
        //    else if (card.playable && !selected)
        //    {
        //        transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
        //        Reposition(basePos + Services.CardConfig.OnHoverOffset, false, true);
        //        if(!(card is MovementCard)) card.OnSelect();
        //    }
        //    else if (card.chest != null)
        //    {
        //        transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
        //        Reposition(basePos + Services.CardConfig.OnHoverOffsetChestMode, false, true);

        //    }
        //    if (player.selectingCards)
        //    {
        //        OnHoverEnterForCardSelection();
        //    }
        //}
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        stateMachine.OnInputExit();
        //if (!selected)
        //{
        //    if (player.selectingCards)
        //    {
        //        OnHoverExitForCardSelection();
        //    }
        //    else if (card.deckViewMode || card.collectionMode)
        //    {
        //        DisplayInDeckViewMode();
        //    }
        //    else if (card.playable && !Input.GetMouseButton(0))
        //    {
        //        DisplayAtBasePos();
        //        if(!(card is MovementCard)) card.OnUnselect();
        //    }
        //    else if (card.chest != null)
        //    {
        //        DisplayAtBasePos();
        //    }
        //}
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        stateMachine.OnInputDown();
        //if (card.deckViewMode) { } //nothing right now
        //else if (card.collectionMode)
        //{
        //    Services.DeckConstruction.OnCardClicked(card);
        //}
        //else if (player.selectingCards) {
        //    Services.EventManager.Fire(new CardSelected(card));
        //}
        //else if (!selected)
        //{
        //    SelectCard();
        //}
        //else if (card is MovementCard)
        //{
        //    UnselectMovementCard();
        //}
    }

    public void UnselectMovementCard()
    {
        stateMachine.TransitionTo<Disabled>();
    }

    public void SelectCard()
    {
        selected = true;
        player.cardsSelected.Add(card);
        Vector3 mousePos = Input.mousePosition;
        mouseRelativePos = mousePos - transform.position;
        if (card is MovementCard && card.playable)
        {
            color = (Color.blue + Color.white) / 2;
            transform.localScale = baseScale * 1.1f;
            Reposition(basePos + Services.CardConfig.OnHoverOffset, false, true);
        }
        else
        {
            transform.SetParent(Services.UIManager.bottomCorner);
        }
        if (card.playable)
        {
            card.OnSelect();
        }
        if (card.chest != null)
        {
            card.chest.OnCardPicked(card);
            selected = false;
            player.cardsSelected.Remove(card);
        }

    }

    void Drag()
    {
        transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
        if (card.playable || card.deckViewMode)
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 newPos = mousePos - mouseRelativePos;
            Reposition(newPos, false, true);
            if (card.playable)
            {
                if (IsInDiscardZone())
                {
                    color = Color.red;
                    if (card is TileTargetedCard)
                    {
                        transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
                        SetCardFrameStatus(true);
                        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
                    }
                }
                else if (rect.anchoredPosition.y >= Services.CardConfig.CardPlayThresholdYPos
                    && card.CanPlay())
                {
                    color = Services.CardConfig.PlayableColor;
                    if (card is TileTargetedCard)
                    {
                        transform.localScale = baseScale;
                        SetCardFrameStatus(false);
                        Services.EventManager.Register<TileSelected>(OnTileSelected);
                    }
                }
                else
                {
                    color = Color.white;
                    if (card is TileTargetedCard)
                    {
                        transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
                        SetCardFrameStatus(true);
                        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
                    }
                }
            }
        }
    }

    public void SetCardFrameStatus(bool status)
    {
        nameText.enabled = status;
        effectText.enabled = status;
        frame.enabled = status;
        if (status)
        {
            art.GetComponent<RectTransform>().anchoredPosition = artBaseLocalPos;
        }
        else
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 mouseLocalPos = rect.InverseTransformPoint(mousePosition);
            art.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                mouseLocalPos.x,
                mouseLocalPos.y);
        }
    }

    void PlaceCard()
    {
        selected = false;
        player.cardsSelected.Remove(card);
        TryToPlayCard(false);
        
    }

    public bool TryToPlayCard(bool forcePlay)
    {
        card.OnUnselect();
        SetCardFrameStatus(true);
        if (IsInDiscardZone())
        {
            player.DiscardCardFromHand(card);
            return false;
        }
        else if (card.playable && card.CanPlay() && (forcePlay ||
            (rect.anchoredPosition.y >= Services.CardConfig.CardPlayThresholdYPos &&
            !(card is TileTargetedCard))))
        {
            Services.Main.taskManager.AddTask(player.PlayCard(card));
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
        transform.SetParent(Services.UIManager.inPlayZone.transform);
        transform.localScale = baseScale;
    }

    public void DisplayAtBasePos()
    {
        color = Color.white;
        transform.localScale = baseScale;
        transform.SetParent(baseParent);
        Reposition(basePos, false);
    }

    public void DisplayInDeckViewMode()
    {
        transform.localScale = baseScale;
        Reposition(basePos, false);
    }

    public void UnselectedForCard()
    {
        stateMachine.TransitionTo<SelectableForCardEffect>();
    }

    public void SelectedForCard()
    {
        stateMachine.TransitionTo<SelectedForCardEffect>();
    }

    void OnHoverEnterForCardSelection()
    {
        color = (Color.red + Color.white) / 2;
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
            bool successfullyPlayedCard = TryToPlayCard(true);
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
        player.cardsSelected.Remove(card);
        Services.EventManager.Unregister<TileSelected>(OnTileSelected);
    }

    public void Disable()
    {
        stateMachine.TransitionTo<Disabled>();
    }

    public void Enable()
    {
        stateMachine.TransitionTo<Playable>();
    }

    public void EnterDeckBuildingMode()
    {
        stateMachine.TransitionTo<Deckbuilding>();
    }

    public void EnterDeckViewMode()
    {
        stateMachine.TransitionTo<DeckView>();
    }

    public void EnterChestMode()
    {
        stateMachine.TransitionTo<Chest>();
    }

    //for dungeon deck use only
    public void EnterPlayedMode()
    {
        stateMachine.TransitionTo<Played>();
    }

    public void SelectMovementCard()
    {
        Debug.Assert(card is MovementCard);
        stateMachine.TransitionTo<MovementCardSelected>();
    }

    public void OnCardDisable()
    {
        color = Color.gray;
        color = Color.gray; //looks silly but it's so gray becomes the "base color"
    }

    public bool IsInDiscardZone()
    {
        RectTransform discardRT =
                    Services.UIManager.discardZone.GetComponent<RectTransform>();
        return discardRT.rect.Contains(discardRT.InverseTransformPoint(transform.position));
    }

    private abstract class CardState : FSM<CardController>.State
    {
        protected Player player { get { return Context.player; } }
        protected Card card { get { return Context.card; } }
        protected Transform transform { get { return Context.transform; } }
        protected Vector3 baseScale { get { return Context.baseScale; } }
    }

    private abstract class Hoverable : CardState
    {
        public override void OnInputEnter()
        {
            transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
            AddOffset();
        }

        protected virtual void AddOffset()
        {
            Context.Reposition(Context.basePos + Services.CardConfig.OnHoverOffset, false, true);
        }

        public override void OnInputExit()
        {
            Context.color = Color.white;
            transform.localScale = baseScale;
            transform.SetParent(Context.baseParent);
            Context.Reposition(Context.basePos, false);
        }
    }

    private class Playable : Hoverable
    {
        public override void OnEnter()
        {
            Context.color = Color.white;
            transform.localScale = baseScale;
            transform.SetParent(Context.baseParent);
            Context.SetCardFrameStatus(true);
            Context.Reposition(Context.basePos, false);
            //Debug.Log(card.cardType + " entering playable state at time " + Time.time);
        }

        public override void OnInputDown()
        {
            if (card is MovementCard) TransitionTo<MovementCardSelected>();
            else if (card is TileTargetedCard) TransitionTo<TargetedCardSelected>();
            else TransitionTo<Selected>();
        }

        public override void OnInputEnter()
        {
            base.OnInputEnter();
            if (!(card is MovementCard)) card.OnSelect();
        }

        public override void OnInputExit()
        {
            base.OnInputExit();
            if (!(card is MovementCard)) card.OnUnselect();
        }
    }

    private class Disabled : CardState
    {
        public override void OnEnter()
        {
            Context.color = Color.gray;
            transform.localScale = baseScale;
            transform.SetParent(Context.baseParent);
            Context.SetCardFrameStatus(true);
            Context.Reposition(Context.basePos, false);
        }
    }

    private class DeckView : Hoverable
    {
        public override void OnEnter()
        {
            Context.color = Color.white;
        }

        protected override void AddOffset()
        {
            Vector3 offset = Services.CardConfig.OnHoverOffsetDeckViewMode;
            if (transform.position.y < 200)
            {
                offset = new Vector3(offset.x, -offset.y, offset.z);
            }
            Context.Reposition(Context.basePos + offset, false, true);
        }
    }

    private class Deckbuilding : DeckView
    {
        public override void OnInputDown()
        {
            Services.DeckConstruction.OnCardClicked(card);
        }
    }

    private class SelectableForCardEffect : CardState
    {
        public override void OnEnter()
        {
            Context.color = Color.magenta;
        }

        public override void OnInputDown()
        {
            Services.EventManager.Fire(new CardSelected(card));
        }

        public override void OnInputEnter()
        {
            Context.color = Color.red;
        }

        public override void OnInputExit()
        {
            Context.color = Color.magenta;
        }
    }

    private class SelectedForCardEffect : CardState
    {
        public override void OnEnter()
        {
            Context.color = Color.red;
        }

        public override void OnInputDown()
        {
            Services.EventManager.Fire(new CardSelected(card));
        }

        public override void OnInputEnter()
        {
            Context.color = Color.magenta;
        }

        public override void OnInputExit()
        {
            Context.color = Color.red;
        }
    }

    private class Selected : CardState
    {
        private Vector3 mouseRelativePos;

        public override void OnEnter()
        {
            mouseRelativePos = Input.mousePosition - Context.transform.position;
            transform.SetParent(Services.UIManager.bottomCorner);
            transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
            card.OnSelect();
            player.cardsSelected.Add(card);
        }

        protected void Drag()
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 newPos = mousePos - mouseRelativePos;
            Context.Reposition(newPos, false, true);
        }

        protected virtual void OnDiscardableEffect()
        {
            Context.color = Color.red;
        }

        protected virtual void OnPlayableEffect()
        {
            Context.color = Services.CardConfig.PlayableColor;
        }

        protected virtual void OnUnplayableEffect()
        {
            Context.color = Color.white;
        }

        protected virtual void OnPlayed()
        {
            player.hand.Remove(card);
            player.cardsInFlux.Add(card);
            Services.UIManager.SortHand(player.hand);
            TransitionTo<Playing>();
        }

        public override void Update()
        {
            bool buttonDown = Input.GetMouseButtonDown(0);
            Drag();
            if (buttonDown) card.OnUnselect();
            if (Context.IsInDiscardZone())
            {
                OnDiscardableEffect();
                if (buttonDown) player.DiscardCardFromHand(card);
            }
            else if (Context.rect.anchoredPosition.y >= Services.CardConfig.CardPlayThresholdYPos &&
                card.CanPlay())
            {
                OnPlayableEffect();
                if (buttonDown) OnPlayed();
            }
            else
            {
                OnUnplayableEffect();
                if (buttonDown) TransitionTo<Playable>();
            }
        }

        public override void OnExit()
        {
            player.cardsSelected.Remove(card);
        }
    }

    private class MovementCardSelected : CardState
    {
        public override void OnEnter()
        {
            Services.EventManager.Register<TileSelected>(OnTileSelected);
            Context.color = (Color.blue + Color.white) / 2;
            transform.localScale = Context.baseScale * 1.1f;
            Context.Reposition(Context.basePos + Services.CardConfig.OnHoverOffset, false, true);
            card.OnSelect();
            player.cardsSelected.Add(card);
            player.movementCardsSelected.Add(card as MovementCard);
        }

        public override void OnInputDown()
        {
            card.OnUnselect();
            TransitionTo<Playable>();
        }

        void OnTileSelected(TileSelected e)
        {
            MovementCard moveCard = card as MovementCard;
            moveCard.OnMovementAct();
            player.hand.Remove(card);
            player.cardsInFlux.Add(card);
            Services.UIManager.SortHand(player.hand);
            TransitionTo<Playing>();
        }

        public override void OnExit()
        {
            Services.EventManager.Unregister<TileSelected>(OnTileSelected);
            player.cardsSelected.Remove(card);
            player.movementCardsSelected.Remove(card as MovementCard);
        }
    }

    private class TargetedCardSelected : Selected
    {
        protected override void OnDiscardableEffect()
        {
            base.OnDiscardableEffect();
            transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
            Context.SetCardFrameStatus(true);
            Services.EventManager.Unregister<TileSelected>(OnTileSelected);
        }

        protected override void OnPlayableEffect()
        {
            base.OnPlayableEffect();
            transform.localScale = Context.baseScale;
            Context.SetCardFrameStatus(false);
            Services.EventManager.Register<TileSelected>(OnTileSelected);
        }

        protected override void OnUnplayableEffect()
        {
            base.OnUnplayableEffect();
            transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
            Context.SetCardFrameStatus(true);
            Services.EventManager.Unregister<TileSelected>(OnTileSelected);
        }

        protected override void OnPlayed()
        {
            TransitionTo<Playable>();
        }

        void OnTileSelected(TileSelected e)
        {
            Tile tileSelected = e.tile;
            TileTargetedCard targetedCard = card as TileTargetedCard;
            card.OnUnselect();
            if (targetedCard.IsTargetValid(tileSelected))
            {
                targetedCard.OnTargetSelected(tileSelected);
                base.OnPlayed();
            }
            else TransitionTo<Playable>();
        }

        public override void OnExit()
        {
            base.OnExit();
            Services.EventManager.Unregister<TileSelected>(OnTileSelected);
        }
    }


    private class Played : CardState
    {
        public override void OnEnter()
        {
            Context.color = Color.white;
            Context.SetCardFrameStatus(true);
            transform.localScale = Context.baseScale;
        }
    }

    private class Playing : CardState
    {
        private float timeElapsed;
        private float duration;
        private Vector3 initialPos;
        private Vector3 targetPos;
        private Vector3 initialScale;
        private Vector3 targetScale;
        private int lockID;
        private RectTransform rect;

        public override void OnEnter()
        {
            timeElapsed = 0;
            duration = Services.CardConfig.PlayAnimDur;
            rect = Context.GetComponent<RectTransform>();
            Context.color = Color.white;
            transform.SetParent(Services.UIManager.inPlayZone.transform);
            initialPos = rect.anchoredPosition;
            targetPos = Services.UIManager.GetInPlayCardPosition(player.cardsInPlay.Count + 1);
            initialScale = transform.localScale;
            targetScale = baseScale;
            lockID = Services.UIManager.nextLockID;
            player.LockEverything(lockID);
            Services.SoundManager.CreateAndPlayAudio(Services.AudioConfig.CardPlayAudio, 0.3f);
        }

        public override void Update()
        {
            timeElapsed += Time.deltaTime;

            card.Reposition(Vector3.Lerp(initialPos, targetPos,
                Easing.QuadEaseOut(timeElapsed / duration)), false, true);
            transform.localScale = Vector3.Lerp(initialScale, targetScale,
                Easing.QuadEaseOut(timeElapsed / duration));

            if (timeElapsed >= duration) OnSuccess();
        }

        void OnSuccess()
        {
            player.cardsInFlux.Remove(card);
            player.cardsInPlay.Add(card);
            card.OnPlay();
            card.Reposition(targetPos, true);
            player.UnlockEverything(lockID);
            Services.UIManager.SortInPlayZone(player.cardsInPlay);
            TransitionTo<Played>();
        }
    }


    private class Chest : Hoverable
    {
        public override void OnEnter()
        {
            Context.color = Color.white;
        }

        public override void OnInputDown()
        {
            card.chest.OnCardPicked(Context.card);
            TransitionTo<Acquisition>();
        }

        protected override void AddOffset()
        {
            Context.Reposition(
                Context.basePos + Services.CardConfig.OnHoverOffsetChestMode, false, true);
        }
    }

    private class Acquisition : CardState
    {
        private float timeElapsed;
        private float duration;
        private Vector3 initialPos;
        private Vector3 targetPos;
        private Vector3 initialScale;
        private Vector3 targetScale;

        public override void OnEnter()
        {
            timeElapsed = 0;
            duration = Services.CardConfig.AcquireAnimDur;
            transform.SetParent(Services.UIManager.bottomCorner);
            initialPos = transform.position;
            targetPos = Services.UIManager.discardZone.transform.position;
            initialScale = card.controller.transform.localScale;
            targetScale = Vector3.zero;
        }

        public override void Update()
        {
            timeElapsed += Time.deltaTime;

            card.Reposition(Vector3.Lerp(initialPos, targetPos,
                Easing.QuartEaseIn(timeElapsed / duration)), false, true);
            transform.localScale = Vector3.Lerp(initialScale, targetScale,
                Easing.QuartEaseIn(timeElapsed / duration));

            if (timeElapsed >= duration) OnSuccess();
        }

        void OnSuccess()
        {
            card.DestroyPhysicalCard();
        }
    }

}





