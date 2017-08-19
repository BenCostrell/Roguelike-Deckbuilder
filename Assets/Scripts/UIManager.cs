using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    [SerializeField]
    private GameObject moveCounter;
    [SerializeField]
    private GameObject deckCounter;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    public void UpdateMoveCounter(int movesAvailable)
    {
        moveCounter.GetComponent<Text>().text = "Moves Available: " + movesAvailable;
    }

    public void UpdateDeckCounter(int cardsInDeck)
    {
        deckCounter.GetComponent<Text>().text = cardsInDeck.ToString();
        Debug.Log(cardsInDeck);
    }
}
