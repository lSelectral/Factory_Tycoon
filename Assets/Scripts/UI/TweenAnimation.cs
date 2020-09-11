using UnityEngine;

/// <summary>
/// UI Animator Class
/// </summary>
public class TweenAnimation : Singleton<TweenAnimation>
{
    private GameObject openPanel;

    /// <summary>
    /// Animate UI object while detect object statue
    /// </summary>
    /// <param name="obj">Object to control</param>
    public void ShowHideElement(GameObject obj)
    {
        //if (openPanel != null && openPanel != obj)
        //    OnDeActivate(openPanel);
        if (obj.activeInHierarchy)
        {
            OnDeActivate(obj);
        }
        else if (!obj.activeInHierarchy)
        {
            OnActivate(obj);
            openPanel = obj;
        }
    }

    /// <summary>
    /// Used animation for opening any UI panel
    /// </summary>
    /// <param name="obj"></param>
    public void OnActivate(GameObject obj)
    {
        LeanTween.rotateZ(obj, 0f, .25f);
        LeanTween.scale(obj, new Vector3(1, 1, 1), .4f).setOnStart(() => ShowHide(obj));
    }

    /// <summary>
    /// Used animation for closing any UI panel
    /// <paramref name="obj"/>
    /// </summary>
    public void OnDeActivate(GameObject obj)
    {
        LeanTween.rotateAround(obj, Vector3.forward, 270, .4f);
        LeanTween.scale(obj, new Vector3(0, 0, 0), .4f).setOnComplete(() => ShowHide(obj));
    }


    public void PopUpFromLeft(GameObject obj)
    {
        LeanTween.moveX(obj, 4f, 1f);
    }

    /// <summary>
    /// Show and hide element, per object statue
    /// </summary>
    /// <param name="obj"></param>
    private void ShowHide(GameObject obj)
    {
        obj.SetActive(!obj.activeInHierarchy);
    }

    public void SmoothDestroy(GameObject obj)
    {
        Destroy(obj);


        //for (int i = 0; i < obj.transform.childCount; i++)
        //{
        //    LeanTween.alphaVertex(obj.transform.GetChild(i).gameObject, .5f, .5f);
        //}
        //LeanTween.alpha(obj.GetComponent<RectTransform>(), .5f, .5f);
        //Debug.Log("Smooth destroy");
    }

    public void SmoothHide(GameObject obj)
    {
        LeanTween.alpha(obj.GetComponent<RectTransform>(), 0f, 1.2f).setOnComplete(() => { LeanTween.alpha(obj.GetComponent<RectTransform>(), 1f, 1f); });
    }

    public LTDescr MoveTool(GameObject obj, Vector3? startAngle = null, Vector3? endAngle = null, float time = 1f)
    {
        startAngle = new Vector3(0,0,-15);
        endAngle = new Vector3(0, 0, 70);
        LeanTween.rotate(obj, startAngle.Value, 0f);
        var t = LeanTween.rotate(obj, endAngle.Value, time).setOnComplete(() => LeanTween.rotate(obj,startAngle.Value, 0f)).setLoopPingPong();
        return t;
    }
}
