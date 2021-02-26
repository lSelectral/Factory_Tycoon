using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TutorialManager : Singleton<TutorialManager>
{
    [SerializeField] GameObject character; // Disabled as default
    TextMeshProUGUI bubbleText;
    Button bubbleBtn;

    [SerializeField] GameObject tutorialBlockPanel; // Disabled as default
    [SerializeField] Canvas canvas;

    public bool isTutorialActive;
    bool isCharacterTextCompleted;
    bool isEligibleForNexText; // If player didn't do the specified action, it is false.

    int currentIndex; // Hold value of text array. Used when character have multiple things to say.

    [SerializeField] GameObject debugPanelREMOVE;

    private void Awake()
    {
        bubbleText = character.transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        bubbleBtn = character.transform.GetChild(0).GetComponent<Button>();
    }

    private void Start()
    {
        //ShowTutorialForPanel(debugPanelREMOVE,
        //    new string[] { "HELLO WORLD", "KNEEL BEFORE ME. I'm your great lord. I came this world over eons ago. And now I will conquer all lands" });
    }

    public void ShowTutorialForPanel(GameObject panel, string[] textArray)
    {
        isTutorialActive = true;

        var oldParent = panel.transform.parent;
        var oldSiblingIndex = panel.transform.GetSiblingIndex();

        tutorialBlockPanel.SetActive(true);
        panel.transform.SetParent(canvas.transform);
        panel.transform.SetAsLastSibling();

        ShowCharacter(textArray, () => 
        {
            isTutorialActive = true; // When character dissappear, it sets variable to false

            if (isCharacterTextCompleted)
            {
                tutorialBlockPanel.SetActive(false);
                panel.transform.SetParent(oldParent);
                panel.transform.SetSiblingIndex(oldSiblingIndex);

                isTutorialActive = false;
                isCharacterTextCompleted = false;
            }
        });
        
    }

    public void ShowCharacter(string[] textArray, UnityAction action = null)
    {
        isTutorialActive = true;

        bubbleText.text = textArray[currentIndex];
        bubbleBtn.onClick.RemoveAllListeners();
        bubbleBtn.onClick.AddListener(() => NextText(textArray, action));
        character.SetActive(true);
        TextWriter.Instance.AddWriter(bubbleText, textArray[currentIndex], .07f, true);
    }

    bool NextText(string[] textArray, UnityAction action = null)
    {
        currentIndex += 1;
        bubbleText.text = "";
        if (currentIndex < textArray.Length && textArray[currentIndex] != null && textArray[currentIndex] != "")
        {
            TextWriter.Instance.AddWriter(bubbleText, textArray[currentIndex], .07f, true);
            return false;
        }
        else
        {
            isCharacterTextCompleted = true;
            if (action != null)
                action.Invoke();

            character.SetActive(false);
            currentIndex = 0;
            isTutorialActive = false;
            return true;
        }
    }

}