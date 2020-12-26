using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Map : MonoBehaviour
{
    //bool isScaling;
    //float touchDistanceOrigin;
    //Vector3 originalScale;
    //float touchDistance = 0f;

    //float scrollSpeed = 5f;

    Vector3 touchStart;

    // Minimum amount pan to move camera
    public float panMinX = 0f;
    public float panMinY = 0f;
    public float panSpeed = 1f;

    [SerializeField] float minX;
    [SerializeField] float maxX;
    [SerializeField] float minY;
    [SerializeField] float maxY;

    private void Start()
    {
    }

    private void Update()
    {
        #region Zoom for PC
        var wheelScrollForce = Input.GetAxis("Mouse ScrollWheel");
        if (EventSystem.current.IsPointerOverGameObject() && EventSystem.current.CompareTag(gameObject.tag))
        {
            if (wheelScrollForce > 0)
                transform.localScale = new Vector3(transform.localScale.x + .2f, transform.localScale.y + .2f, 0);
            else if (wheelScrollForce < 0)
                transform.localScale = new Vector3(transform.localScale.x - .2f, transform.localScale.y - .2f, 0);
            transform.localScale = new Vector3(Mathf.Clamp(transform.localScale.x, 1f, 5f), Mathf.Clamp(transform.localScale.y, 1f, 5f));
        }
        #endregion

        if (Input.GetMouseButtonDown(0) && EventSystem.current.CompareTag(gameObject.tag))
        {
            //Debug.Log("Scrolling on map");
            touchStart = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0) && EventSystem.current.CompareTag(gameObject.tag))
        {
            //Debug.Log("Map is moving");
            Vector3 direction = touchStart - Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Debug.Log(direction);
            if (Mathf.Abs(direction.x) > panMinX || Mathf.Abs(direction.y) > panMinY)
            {
                //if (!CameraMovement.Instance.isCameraInBorder)
                //{
                //    Camera.main.transform.position += direction;
                //    if (CameraMovement.Instance.IsCameraInBorder(direction))
                //        Camera.main.transform.position -= direction;
                //}
                transform.position -= direction * panSpeed;
                
            }
        }

        //if (Input.touchCount == 2)
        //{
        //    //The distance between the 2 touches is checked and subsequently used to scale the
        //    //object by moving the 2 fingers further, or closer form eachother.
        //    Touch touch0 = Input.GetTouch(0);
        //    Touch touch1 = Input.GetTouch(1);
        //    if (isScaling)//this will only be done if scaling is true
        //    {
        //        float currentTouchDistance = touchDistance;
        //        float deltaTouchDistance = currentTouchDistance - touchDistanceOrigin;
        //        float scalePercentage = (deltaTouchDistance / 1200f) + 1f;

        //        Vector3 scaleTemp = transform.localScale;
        //        scaleTemp.x = scalePercentage * originalScale.x;
        //        scaleTemp.y = scalePercentage * originalScale.y;
        //        scaleTemp.z = scalePercentage * originalScale.z;

        //        //to make the object snap to 100% a check is being done to see if the object scale is close to 100%,
        //        //if it is the scale will be put back to 100% so it snaps to the normal scale.
        //        //this is a quality of life feature, so its easy to get the original size of the object.
        //        if (scaleTemp.x * 100 < 102 && scaleTemp.x * 100 > 98)
        //        {
        //            scaleTemp.x = 1;
        //            scaleTemp.y = 1;
        //            scaleTemp.z = 1;
        //        }
        //        //here we apply the calculation done above to actually make the object bigger/smaller.
        //        transform.localScale = scaleTemp;


        //    }
        //    else
        //    {
        //        //if 2 fingers are touching the screen but isScaling is not true we are going to see if
        //        //the middle of the screen is looking at the object and if it is set isScalinf to true;
        //        Ray ray;
        //        RaycastHit hitTouch;
        //        ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //        if (Physics.Raycast(ray, out hitTouch, 100f))
        //        {
        //            if (hitTouch.transform == transform)
        //            {
        //                isScaling = true;
        //                //make sure that the distance between the fingers on initial contact is used as the original distance
        //                touchDistanceOrigin = touchDistance;
        //                originalScale = transform.localScale;
        //            }
        //        }
        //    }
        //}
    }
}
