using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Chest Info")]
public class ChestInfo : MapObjectInfo
{
    [SerializeField]
    private int numCards;
    public int NumCards { get { return numCards; } }

    [SerializeField]
    private Vector3 leftmostCardPosition;
    public Vector3 LeftmostCardPosition { get { return leftmostCardPosition; } }

    [SerializeField]
    private Vector3 cardSpacing;
    public Vector3 CardSpacing { get { return cardSpacing; } }

    [SerializeField]
    private float scaleFactor;
    public float ScaleFactor { get { return scaleFactor; } }
}
