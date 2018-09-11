using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AddPointEnterEvent : MonoBehaviour,IPointerEnterHandler {

    public delegate void GoDelegate(GameObject go);
    public GoDelegate onPointEnter;

    public void AddListener(GoDelegate callBack)
    {
        onPointEnter += callBack;
    }

    public void RemoveListener(GoDelegate callBack)
    {
        onPointEnter -= callBack;
    }

    public void RemoveAllListener()
    {
        onPointEnter = null;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onPointEnter != null)
            onPointEnter(gameObject);
    }
}
