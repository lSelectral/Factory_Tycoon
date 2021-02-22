using UnityEngine;

[CreateAssetMenu(fileName ="New Artifact",menuName ="Create Artifact")]
public class ScriptableArtifact : ScriptableObject
{
    public string artifactName;
    [PreviewSprite] public Sprite icon;
    [SearchableEnum] public Age ageBelongsTo;

    [SearchableEnum] public ArtifactPart bodyPart;
    [SearchableEnum] public ArtifactTier rarity;
    [SearchableEnum] public ArtifacPower artifactPower;
    [SearchableEnum] public ArtifactSet setBelongsTo;

    [Range(0,3)] public float powerAmount;
}