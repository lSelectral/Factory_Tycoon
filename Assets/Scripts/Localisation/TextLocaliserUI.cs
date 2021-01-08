using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLocaliserUI : MonoBehaviour
{
    TextMeshProUGUI text;

    public LocalisedString localisedString;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = localisedString.Value;
    }
}
