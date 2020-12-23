using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    [Range(0f, 1f)] public float alphaThreshold;
    [SerializeField] bool isAlphaThresholdActive;
    Image image;
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        image.alphaHitTestMinimumThreshold = alphaThreshold;
    }
}
