using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen : Scene<MainTransitionData> {

	// Use this for initialization
	void Start () {
        Services.EventManager.Register<ButtonPressed>(StartGame);
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButtonDown(0))
            Services.EventManager.Fire(new ButtonPressed("X", 1));
	}

    internal override void OnEnter(MainTransitionData data)
    {
        Services.SoundManager.SetMusicVolume(0.02f);
        Services.GameManager.currentCamera = GetComponentInChildren<Camera>();
        Services.GameManager.currentCanvas = GetComponentInChildren<Canvas>();
    }

    void StartGame(ButtonPressed e)
    {
        Services.EventManager.Unregister<ButtonPressed>(StartGame);
        List<Card> startingDeck = new List<Card>();
        List<Card> startingCollection = new List<Card>();
        foreach (CardInfo cardInfo in Services.CardConfig.StartingDeck)
        {
            Card card = Services.CardConfig.CreateCardOfType(cardInfo.CardType);
            startingDeck.Add(card);
        }
        for (int i = 0; i < 4; i++)
        {
            startingCollection.Add(Services.CardConfig.CreateCardOfType(
                Card.CardType.AppleTree));
        }


        Services.SceneStackManager.Swap<LevelTransition>(
            new MainTransitionData(startingDeck, new List<Card>(), startingCollection,
            Services.GameManager.playerStartingHealth,
            Services.GameManager.playerStartingHealth, 1, false));
    }
}
