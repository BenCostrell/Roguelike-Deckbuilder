using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardController : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, 
    IPointerExitHandler, IPointerUpHandler, IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public Card card { get; private set; }
    private Vector3 basePos;
    [HideInInspector]
    public Vector3 baseScale;
    [HideInInspector]
    public Transform baseParent;
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
            frame.color = value;
        }
    }
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
    private Color baseColor;
    public bool isQueued { get { return stateMachine.CurrentState is MovementCardSelected; } }
    public Quaternion targetRotation { get; private set; }
    [SerializeField]
    private float discardDetectionRadius;

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
        baseColor = card.GetCardFrameColor();
        art = imageElements[2];
        nameText.text = card.info.Name;
        effectText.text = card.info.CardText;
        if(card is MonsterCard)
        {
            MonsterCard monsterCard = card as MonsterCard;
            MonsterInfo monsterInfo = 
                Services.MonsterConfig.GetMonsterOfType(monsterCard.monsterToSpawn);
            effectText.text +=
                "\nHP: " + monsterInfo.StartingHealth +
                "\nATTACK: " + monsterInfo.AttackDamage +
                "\nMOVE: " + monsterInfo.MovementSpeed +
                "\nRANGE: " + monsterInfo.AttackRange;
        }
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
    }

    public void RotateTo(Quaternion targetRot)
    {
        targetRotation = targetRot;
    }

    public void Reposition(Vector3 pos, bool changeBasePos)
    {
        Reposition(pos, changeBasePos, false);
    }

    public void Reposition(Vector3 pos, bool changeBasePos, bool front)
    {
        rect.anchoredPosition = pos;
        if (changeBasePos) {
            basePos = pos;
        }
        if (front) rect.SetAsLastSibling();
        else rect.SetSiblingIndex(baseSiblingIndex);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        stateMachine.OnInputEnter();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        stateMachine.OnInputExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        stateMachine.OnInputDown();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        stateMachine.OnInputClick();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        stateMachine.OnInputUp();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        stateMachine.OnBeginDrag();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        stateMachine.OnEndDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void UnselectMovementCard()
    {
        card.OnUnselect();
        stateMachine.TransitionTo<Playable>();
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

    public void DisplayInPlay()
    {
        color = baseColor;
        transform.SetParent(Services.UIManager.inPlayZone.transform);
        transform.localScale = baseScale;
    }

    public void UnselectedForCard()
    {
        stateMachine.TransitionTo<SelectableForCardEffect>();
    }

    public void SelectedForCard()
    {
        stateMachine.TransitionTo<SelectedForCardEffect>();
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

    public void EnterDungonHandMode()
    {
        stateMachine.TransitionTo<DungeonHand>();
    }

    public void EnterDungeonPlayingMode()
    {
        stateMachine.TransitionTo<DungeonPlaying>();
    }

    public void EnterAddToDungeonDeckMode()
    {
        stateMachine.TransitionTo<AddingToDungeonDeck>();
    }

    public void EnterDiscardMode()
    {
        stateMachine.TransitionTo<Discarding>();
    }

    public void EnterDrawingState()
    {
        stateMachine.TransitionTo<Drawing>();
    }

    public void EnterReshuffleState()
    {
        stateMachine.TransitionTo<Reshuffling>();
    }

    public void SelectMovementCard()
    {
        Debug.Assert(card is MovementCard);
        stateMachine.TransitionTo<MovementCardSelected>();
    }

    public bool IsInDiscardZone()
    {
        RectTransform discardRT = Services.UIManager.discardZone.GetComponent<RectTransform>();
        List<Vector3> directions = new List<Vector3>()
        {
            Vector3.up, Vector3.down, Vector3.left, Vector3.right
        };
        for (int i = 0; i < directions.Count; i++)
        {
            Vector3 point = transform.position + (discardDetectionRadius * directions[i]);
            if (discardRT.rect.Contains(discardRT.InverseTransformPoint(point))){ 
                return true;
            }
        }
        return false;
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
        protected Quaternion baseRotation;
        protected bool hovered;

        public override void OnEnter()
        {
            base.OnEnter();
            baseRotation = transform.localRotation;
        }

        public override void OnInputEnter()
        {
            transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
            transform.localRotation = Quaternion.identity;
            AddOffset();
            hovered = true;
        }

        protected virtual void AddOffset()
        {
            Context.Reposition(Context.basePos + Services.CardConfig.OnHoverOffset, false, true);
        }

        public override void OnInputExit()
        {
            Context.color = Context.baseColor;
            transform.localScale = baseScale;
            transform.localRotation = baseRotation;
            transform.SetParent(Context.baseParent);
            Context.Reposition(Context.basePos, false);
            hovered = false;
        }
    }

    private class Playable : Hoverable
    {
        private int tempLockFramesLeft;

        public override void OnEnter()
        {
            Services.UIManager.SortHand(player.hand);
            base.OnEnter();
            Context.color = Context.baseColor;
            transform.localScale = baseScale;
            transform.SetParent(Context.baseParent);
            Context.SetCardFrameStatus(true);
            Context.Reposition(Context.basePos, false);
            tempLockFramesLeft = 7;
        }

        public override void OnInputClick()
        {
            Select();
        }

        public override void OnBeginDrag()
        {
            Select();
        }

        void Select()
        {
            if (tempLockFramesLeft == 0)
            {
                if (card is MovementCard) TransitionTo<MovementCardSelected>();
                else if (card is TileTargetedCard) TransitionTo<TargetedCardSelected>();
                else TransitionTo<Selected>();
            }
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

        protected override void AddOffset()
        {
            Vector3 newPos = new Vector3(
                Context.basePos.x,
                Services.CardConfig.OnHoverOffset.y,
                0);
            Context.Reposition(newPos, false, true);
        }

        public override void Update()
        {
            if(!hovered && 
                Quaternion.Angle(transform.localRotation, Context.targetRotation) > 0.1f)
            {
                transform.localRotation = 
                    Quaternion.Lerp(transform.localRotation, Context.targetRotation, 
                    Services.CardConfig.HandCardRotationSpeed);
            }
            if (tempLockFramesLeft > 0)
            {
                tempLockFramesLeft -= 1;
            }
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

        public override void Update()
        {
            if (Quaternion.Angle(transform.localRotation, Context.targetRotation) > 0.1f)
            {
                transform.localRotation =
                    Quaternion.Lerp(transform.localRotation, Context.targetRotation,
                    Services.CardConfig.HandCardRotationSpeed);
            }
        }
    }

    private class DeckView : Hoverable
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.color = Context.baseColor;
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
        public override void OnInputClick()
        {
            Services.DeckConstruction.OnCardClicked(card);
        }
    }

    private class SelectableForCardEffect : CardState
    {
        public override void OnEnter()
        {
            Context.color = Color.magenta;
            transform.SetParent(Context.baseParent);
            transform.localScale = baseScale;
            Services.UIManager.SortHand(player.hand);
        }

        public override void OnInputClick()
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

        public override void OnInputClick()
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
        private Vector2 mouseRelativePos;
        private int lockID;
        private bool dragEnded;
        private Tile tileHovered;

        public override void OnEnter()
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x * 1600 / Screen.width,
                Input.mousePosition.y * 900 / Screen.height);
            transform.SetParent(Services.UIManager.bottomLeft);
            mouseRelativePos = mousePos - (Vector2)Context.GetComponent<RectTransform>().anchoredPosition;
            transform.localScale = Services.CardConfig.OnHoverScaleUp * baseScale;
            card.OnSelect();
            player.cardsSelected.Add(card);
            lockID = Services.UIManager.nextLockID;
            player.LockMovement(lockID);
        }

        protected void Drag()
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x * 1600 / Screen.width,
                Input.mousePosition.y * 900 / Screen.height);
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
            Context.color = Context.baseColor;
        }

        protected virtual void OnPlayed()
        {
            player.hand.Remove(card);
            player.ShowAvailableMoves();
            player.cardsInFlux.Add(card);
            Services.UIManager.SortHand(player.hand);
            Context.RotateTo(Quaternion.identity);
            TransitionTo<Playing>();
        }

        public override void OnEndDrag()
        {
            dragEnded = true;
        }

        public override void Update()
        {
            bool buttonDown = Input.GetMouseButtonDown(0) || dragEnded;
            if (dragEnded) dragEnded = false;
            Drag();
            if (buttonDown) card.OnUnselect();
            if (Context.IsInDiscardZone())
            {
                OnDiscardableEffect();
                if (buttonDown)
                    Services.Main.taskManager.AddTask(player.DiscardCardFromHand(card, 0));
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
            player.UnlockMovement(lockID);
        }
    }

    private class MovementCardSelected : CardState
    {
        public override void OnEnter()
        {
            Services.EventManager.Register<MovementInitiated>(OnMovementIntiated);
            Context.color = (Color.blue + Color.white) / 2;
            transform.localScale = Context.baseScale * 1.1f;
            transform.localRotation = Quaternion.identity;
            Vector3 newPos = new Vector3(
                Context.basePos.x,
                Services.CardConfig.OnHoverOffset.y,
                0);
            Context.Reposition(newPos, false, true);
            player.cardsSelected.Add(card);
            player.SelectMovementCard(card as MovementCard);
            card.OnSelect();
        }

        public override void OnInputClick()
        {
            card.OnUnselect();
            TransitionTo<Playable>();
        }

        void OnMovementIntiated(MovementInitiated e)
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
            Services.EventManager.Unregister<MovementInitiated>(OnMovementIntiated);
            player.cardsSelected.Remove(card);
            player.UnselectMovementCard(card as MovementCard);
        }
    }

    private class TargetedCardSelected : Selected
    {
        private Tile tileHovered;

        public override void OnEnter()
        {
            base.OnEnter();
            TileTargetedCard targetedCard = card as TileTargetedCard;
            targetedCard.ClearTargets();
        }

        public override void Update()
        {
            base.Update();
            tileHovered = null;
        }

        protected override void OnDiscardableEffect()
        {
            base.OnDiscardableEffect();
            transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
            Context.SetCardFrameStatus(true);
            Services.EventManager.Unregister<TileHovered>(OnTileHovered);
        }

        protected override void OnPlayableEffect()
        {
            base.OnPlayableEffect();
            transform.localScale = Context.baseScale;
            Context.SetCardFrameStatus(false);
            Services.EventManager.Register<TileHovered>(OnTileHovered);
        }

        protected override void OnUnplayableEffect()
        {
            base.OnUnplayableEffect();
            transform.localScale = baseScale * Services.CardConfig.OnHoverScaleUp;
            Context.SetCardFrameStatus(true);
            Services.EventManager.Unregister<TileHovered>(OnTileHovered);
        }

        void OnTileHovered(TileHovered e)
        {
            tileHovered = e.tile;
        }

        protected override void OnPlayed()
        {
            if (tileHovered == null) TransitionTo<Playable>();
            else OnTileSelected(tileHovered);
        }

        void OnTileSelected(Tile tileSelected)
        {
            TileTargetedCard targetedCard = card as TileTargetedCard;
            if (targetedCard.IsTargetValid(tileSelected))
            {
                targetedCard.OnTargetSelected(tileSelected);
                if (targetedCard.SelectionComplete())
                {
                    card.OnUnselect();
                    base.OnPlayed();
                }
            }
            else
            {
                card.OnUnselect();
                TransitionTo<Playable>();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            Services.EventManager.Unregister<TileHovered>(OnTileHovered);
            //Debug.Log("unregistering at time " + Time.time);
        }
    }

    private class DungeonHand : CardState
    {

    }

    private class DungeonPlaying : CardState
    {
        public override void OnEnter()
        {
            transform.localRotation = Quaternion.identity;
        }
    }


    private class Played : Hoverable
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.color = Context.baseColor;
            Context.SetCardFrameStatus(true);
            transform.localScale = Context.baseScale;
            transform.localRotation = Quaternion.identity;
            baseRotation = Quaternion.identity;
            if (!(card is DungeonCard)) player.OnCardFinishedPlaying(card);
        }

        protected override void AddOffset()
        {
            //if (!(card is DungeonCard))
            //{
                Context.Reposition(Context.basePos, false, true);
            //}
            //else
            //{
            //    Vector3 newPos = new Vector3(
            //        Context.basePos.x,
            //        -Services.CardConfig.OnHoverOffset.y + 100, 0);
            //    Context.Reposition(newPos, false, true);
            //}
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
            Context.color = Context.baseColor;
            transform.SetParent(Services.UIManager.inPlayZone.transform);
            initialPos = rect.anchoredPosition;
            targetPos = Services.UIManager.GetInPlayCardPosition(player.cardsInPlay.Count + 1);
            initialScale = transform.localScale;
            targetScale = baseScale;
            lockID = Services.UIManager.nextLockID;
            player.LockEverything(lockID);
            Context.SetCardFrameStatus(true);
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
            Context.baseParent = transform.parent;
            player.UnlockEverything(lockID);
            Services.UIManager.SortInPlayZone(player.cardsInPlay);
            Context.RotateTo(Quaternion.identity);
            TransitionTo<Played>();
        }
    }


    private class Chest : Hoverable
    {
        public override void OnEnter()
        {
            base.OnEnter();
            Context.color = Context.baseColor;
        }

        public override void OnInputClick()
        {
            card.chest.OnCardPicked(card);
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
            transform.SetParent(Services.UIManager.bottomLeft);
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
            Services.EventManager.Fire(new AcquisitionComplete());
        }
    }

    private class AddingToDungeonDeck : CardState
    {
        public override void OnEnter()
        {
            Context.color = Context.baseColor;
        }
    }

    private class Discarding : CardState
    {
        public override void OnEnter()
        {
            Context.color = Context.baseColor;
            transform.localRotation = Quaternion.identity;
        }
    }

    private class Drawing : CardState
    {
        public override void OnEnter()
        {
            Context.color = Context.baseColor;
        }

        public override void Update()
        {
            if (Quaternion.Angle(transform.localRotation, Context.targetRotation) > 0.1f)
            {
                transform.localRotation =
                    Quaternion.Lerp(transform.localRotation, Context.targetRotation, 
                    Services.CardConfig.HandCardRotationSpeed);
            }
        }
    }

    private class Reshuffling : CardState
    {
        public override void OnEnter()
        {
            Context.color = Context.baseColor;
        }
    }



}





