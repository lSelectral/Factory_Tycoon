using UnityEngine;

[CreateAssetMenu(fileName = "New Mercenary", menuName = "Create Mercenary")]
public class ScriptableMercenary : ScriptableObject
{
    public string mercenaryName;

    public string description;

    [PreviewSprite] public Sprite icon;

    public long health;

    public long attack;

    public long defense;

    public Age ageBelongsTo;
}
