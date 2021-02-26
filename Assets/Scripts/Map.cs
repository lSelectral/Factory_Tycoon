using UnityEngine;
using UnityEngine.EventSystems;

public class Map : MonoBehaviour
{
    Vector3 touchStart;

    // Minimum amount pan to move camera
    public float panMinX = 0f;
    public float panMinY = 0f;
    public float panSpeed = 1f;

    Transform leftTopPoint;
    Transform rightBottomPoint;
    Transform parentLeftTop;
    Transform parentRightBottom;
    RectTransform mapRect;

    private void Start()
    {
        mapRect = GetComponent<RectTransform>();
        leftTopPoint = transform.Find("LeftTop");
        rightBottomPoint = transform.Find("RightBottom");
        parentLeftTop = transform.parent.Find("LeftTop");
        parentRightBottom = transform.parent.Find("RightBottom");
    }

    private void Update()
    {
        // Don't update when page is not active. This line should be at top. This increase performance a little bit.
        if (GameManager.Instance.VisiblePanelForPlayer != transform.parent.parent.parent.gameObject) return;

        if (Input.GetMouseButtonDown(0) && EventSystem.current.CompareTag(gameObject.tag))
        {
            touchStart = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }

        #region Zoom for PC
        //#if UNITY_ANDROID || UNITY_IOS
        if (Input.touchCount == 2 && !EventSystem.current.IsPointerOverGameObject())
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPosition = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPosition = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPosition - touchOnePrevPosition).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = currentMagnitude - prevMagnitude;
            if (difference != 0)
            {
                if (difference > 0)
                    transform.localScale = new Vector3(transform.localScale.x + difference, transform.localScale.y + difference, 0);
                else if (difference < 0)
                    transform.localScale = new Vector3(transform.localScale.x - difference, transform.localScale.y - difference, 0);
                // Bring map to center for not exposing the emty parts when zooming.
                //mapRect.offsetMin = new Vector2(0, 0);
                //mapRect.offsetMax = new Vector2(1, 1);
                    mapRect.localPosition = new Vector3(.5f, .5f);
                transform.localScale = new Vector3(Mathf.Clamp(transform.localScale.x, .67f, 5f), Mathf.Clamp(transform.localScale.y, .67f, 5f));
            }
        }
        //#elif UNITY_EDITOR || UNITY_PC || UNITY_WEBGL
        var wheelScrollForce = Input.GetAxis("Mouse ScrollWheel");
                if (EventSystem.current.IsPointerOverGameObject() && EventSystem.current.CompareTag(gameObject.tag) && wheelScrollForce != 0)
                {
                    if (wheelScrollForce > 0)
                        transform.localScale = new Vector3(transform.localScale.x + .2f, transform.localScale.y + .2f, 0);
                    else if (wheelScrollForce < 0)
                        transform.localScale = new Vector3(transform.localScale.x - .2f, transform.localScale.y - .2f, 0);
            // Bring map to center for not exposing the emty parts when zooming.
            //mapRect.offsetMin = new Vector2(0, 0);
            //mapRect.offsetMax = new Vector2(1, 1);
                    mapRect.localPosition = new Vector3(.5f, .5f);
                    transform.localScale = new Vector3(Mathf.Clamp(transform.localScale.x, .67f, 5f), Mathf.Clamp(transform.localScale.y, .67f, 5f));
                }
        //#endif
        #endregion

        if (Input.GetMouseButton(0) && EventSystem.current.CompareTag(gameObject.tag))
        {
            Vector3 direction = touchStart - Camera.main.ScreenToViewportPoint(Input.mousePosition);
            if (Mathf.Abs(direction.x) > panMinX || Mathf.Abs(direction.y) > panMinY)
            {
                if (parentLeftTop.position.x>= leftTopPoint.position.x - direction.x * panSpeed &&
                    parentRightBottom.position.x <= rightBottomPoint.position.x - direction.x * panSpeed &&
                    parentLeftTop.position.y <= leftTopPoint.position.y - direction.y * panSpeed &&
                    parentRightBottom.position.y >= rightBottomPoint.position.y - direction.y * panSpeed
                    )
                {
                    transform.position -= direction * panSpeed;
                }

            }
        }
    }
}
