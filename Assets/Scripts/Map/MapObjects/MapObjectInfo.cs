using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Map Object Info")]
public class MapObjectInfo : ScriptableObject
{
    [SerializeField]
    private MapObject.ObjectType objectType;
    public MapObject.ObjectType ObjectType { get { return objectType; } }

    [SerializeField]
    private string name_;
    public string Name { get { return name_; } }

    [SerializeField]
    private Sprite sprite;
    public Sprite Sprite { get { return sprite; } }

    [SerializeField]
    private AudioClip onStepAudio;
    public AudioClip OnStepAudio { get { return onStepAudio; } }
}
