using TMPro;
using UnityEngine;

public class PageManager : Singleton<PageManager>
{
    public TextMeshProUGUI minePageInfoText;

    public int index = 0;

    Vector3 touchStart;

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
    //    {
    //        touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //    }

    //    if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
    //    {
    //        Vector3 direction = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        if (Mathf.Abs(direction.x) > panMinX || Mathf.Abs(direction.y) > panMinY)
    //    }
    //}
}
