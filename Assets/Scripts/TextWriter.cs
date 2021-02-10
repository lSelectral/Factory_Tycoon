using TMPro;
using System;
using UnityEngine;

public class TextWriter : Singleton<TextWriter>
{
    TextMeshProUGUI textHolder;
    string textToWrite;
    float timePerCharacter;
    float timer;
    int characterIndex;
    bool invisibleCharacters;

    private void Update()
    {
        if (textHolder != null)
        {
            timer -= Time.deltaTime;
            while (timer <= 0f)
            {
                timer += timePerCharacter;
                characterIndex++;
                string text = textToWrite.Substring(0, characterIndex);
                if (invisibleCharacters)
                {
                    text += "<color=#00000000>" + textToWrite.Substring(characterIndex) + "</color>";
                }
                textHolder.text = text;
            }
            if (characterIndex >= textToWrite.Length)
            {
                textHolder = null;
                return;
            }
        }
    }

    public void AddWriter(TextMeshProUGUI textHolder, string textToWrite, float timePerCharacter, bool invisibleCharacters)
    {
        this.invisibleCharacters = invisibleCharacters;
        this.textHolder = textHolder;
        this.textToWrite = textToWrite;
        this.timePerCharacter = timePerCharacter;
        characterIndex = 0;
    }
}