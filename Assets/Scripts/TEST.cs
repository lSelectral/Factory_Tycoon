using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TEST : MonoBehaviour, IPointerDownHandler
{

    public void OnPointerDown(PointerEventData eventData)
    {


    }

    private void Start()
    {
        Debug.Log( (new BNum(15468.3, 7) + new BNum(219.3333,6) ).ToString());
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Alpha1))
        //{
        //    SaveSystem.Instance.Save();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha2))
        //{
        //    SaveSystem.Instance.Load();
        //}
        //if (Input.GetKeyDown(KeyCode.Alpha3))
        //{

        //}
    }
}

