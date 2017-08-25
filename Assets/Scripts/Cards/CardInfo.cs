using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Card Info")]
public class CardInfo : ScriptableObject
{
    [SerializeField]
    private Card.CardType cardType;
    public Card.CardType CardType { get { return cardType; } }

    [SerializeField]
    private string name_;
    public string Name { get { return name_; } }

    [SerializeField]
    [TextArea]
    private string cardText;
    public string CardText { get { return cardText.Replace("\\n", "\n"); } }

    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private int tier;
    public int Tier { get { return tier; } }

    [SerializeField]
    private bool isMonster;
    public bool IsMonster { get { return isMonster; } }

}
