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
    private Sprite[] sprites;
    public Sprite[] Sprites { get { return sprites; } }

    [SerializeField]
    private AudioClip onStepAudio;
    public AudioClip OnStepAudio { get { return onStepAudio; } }
}
