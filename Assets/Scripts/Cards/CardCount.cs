using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class CardCount
{
    public int count { get; private set; }
    private bool infinite;
    private TextMeshProUGUI counter;
    private readonly Vector3 offset = new Vector3(0, -200f, 0);
    public readonly Card card;

    public CardCount(int count_, Card card_)
    {
        if (count_ == -1) infinite = true;
        else count = count_;
        card = card_;
    }

    public void CreateCounter(Transform cardTransform)
    {
        counter = GameObject.Instantiate(Services.Prefabs.CardCounter, cardTransform.position,
            Quaternion.identity, cardTransform).GetComponent<TextMeshProUGUI>();
        counter.GetComponent<RectTransform>().anchoredPosition = offset;
        if (infinite) counter.text = "infinite";
        else counter.text = "x " + count;
    }

    public bool Remove()
    {
        if (!infinite)
        {
            count -= 1;
            counter.text = "x " + count;
            if (count == 0) return false; // there are no more copies remaining
        }
        return true; // there are still copies remaining
    }

    public void Add()
    {
        if (!infinite)
        {
            count += 1;
            counter.text = "x " + count;
        }
    }
}
