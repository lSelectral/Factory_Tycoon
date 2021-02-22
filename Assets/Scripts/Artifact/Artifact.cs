//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Artifact : MonoBehaviour
//{
//    public ScriptableArtifact scriptableArtifact;

//    string artifactName;
//    Sprite icon;
//    float powerAmount;

//    Age ageBelongsTo;
//    ArtifactPart bodyPart;
//    ArtifactTier rarity;
//    ArtifacPower artifactPower;
//    ArtifactSet setBelongsTo;

//    public string ArtifactName { get => artifactName; set => artifactName = value; }
//    public Sprite Icon { get => icon; set => icon = value; }
//    public float PowerAmount { get => powerAmount; set => powerAmount = value; }
//    public Age AgeBelongsTo { get => ageBelongsTo; set => ageBelongsTo = value; }
//    public ArtifactPart BodyPart { get => bodyPart; set => bodyPart = value; }
//    public ArtifactTier Rarity { get => rarity; set => rarity = value; }
//    public ArtifacPower ArtifactPower { get => artifactPower; set => artifactPower = value; }
//    public ArtifactSet SetBelongsTo { get => setBelongsTo; set => setBelongsTo = value; }

//    public void Setup()
//    {
//        artifactName = scriptableArtifact.artifactName;
//        icon = scriptableArtifact.icon;
//        powerAmount = powerAmount = scriptableArtifact.powerAmount;
//        ageBelongsTo = scriptableArtifact.ageBelongsTo;
//        bodyPart = scriptableArtifact.bodyPart;
//        rarity = scriptableArtifact.rarity;
//        artifactPower = scriptableArtifact.power;
//        setBelongsTo = scriptableArtifact.setBelongsTo;
//        GetComponent<Image>().sprite = icon;
//    }
//}
