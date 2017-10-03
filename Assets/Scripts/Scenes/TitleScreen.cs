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
		
	}

    internal override void OnEnter(MainTransitionData data)
    {
        Services.SoundManager.SetMusicVolume(0.02f);
    }

    void StartGame(ButtonPressed e)
    {
        Services.EventManager.Unregister<ButtonPressed>(StartGame);
        List<Card> startingDeck = new List<Card>();
        foreach (CardInfo cardInfo in Services.CardConfig.StartingDeck)
        {
            Card card = Services.CardConfig.CreateCardOfType(cardInfo.CardType);
            startingDeck.Add(card);
        }
        Services.SceneStackManager.Swap<LevelTransition>(
            new MainTransitionData(startingDeck, new List<Card>(), new List<Card>(),
            Services.GameManager.playerStartingHealth, 1, false));
    }
}
