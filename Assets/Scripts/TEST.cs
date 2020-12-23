using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class TEST : MonoBehaviour, IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)
    {
        //Vector2 localCursor;
        //if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), eventData.position, eventData.pressEventCamera, out localCursor))
        //    return;

        //Debug.Log(image.sprite.texture.GetPixel((int)localCursor.x,(int)localCursor.y));

        //Vector2 localCursor;
        //var rect1 = GetComponent<RectTransform>();
        //var pos1 = eventData.position;
        //if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect1, pos1,
        //    null, out localCursor))
        //    return;

        //int xpos = (int)(localCursor.x);
        //int ypos = (int)(localCursor.y);

        //if (xpos < 0) xpos = xpos + (int)rect1.rect.width / 2;
        //else xpos += (int)rect1.rect.width / 2;

        //if (ypos > 0) ypos = ypos + (int)rect1.rect.height / 2;
        //else ypos += (int)rect1.rect.height / 2;

        //Debug.Log(image.sprite.texture.GetPixel(xpos, ypos));
        //image.sprite.texture.SetPixel(xpos, ypos, Color.green);

        //image.sprite.texture.Apply();

        ////var q = Instantiate(ResourceManager.Instance.resourceIconPrefab, new Vector3(xpos, ypos, 0), Quaternion.identity);
        ////q.transform.SetParent(transform);
        //Debug.Log("Correct Cursor Pos: " + xpos + " " + ypos);

    }

    private void Start()
    {
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

        //if (Input.GetKeyDown(KeyCode.F4))
        //{
        //    foreach (var _mine in ProductionManager.Instance.mineList)
        //    {

        //    }

        //    foreach (var comp in ProductionManager.Instance.compoundList)
        //    {

        //    }
        //}
    }
}