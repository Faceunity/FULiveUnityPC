using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class AddPointExitEvent : MonoBehaviour,IPointerExitHandler {

    public delegate void GoDelegate(GameObject go);
    public GoDelegate onPointExit;

    public void AddListener(GoDelegate callBack)
    {
        onPointExit += callBack;
    }

    public void RemoveListener(GoDelegate callBack)
    {
        onPointExit -= callBack;
    }

    public void RemoveAllListener()
    {
        onPointExit = null;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (onPointExit != null)
            onPointExit(gameObject);
    }
}
