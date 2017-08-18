using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Card Info")]
public class CardInfo : ScriptableObject
{
    [SerializeField]
    private Card.Type type;
    public Card.Type Type { get { return type; } }

    [SerializeField]
    private string name_;
    public string Name { get { return name_; } }

    [SerializeField]
    private string cardText;
    public string CardText { get { return cardText; } }

    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

}
