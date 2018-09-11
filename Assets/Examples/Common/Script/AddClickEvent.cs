using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class AddClickEvent : MonoBehaviour,IPointerClickHandler
{
    public delegate void GoDelegate(GameObject go);
    public GoDelegate onClick;

    public void AddListener(GoDelegate callBack)
    {
        onClick += callBack;
    }

    public void RemoveListener(GoDelegate callBack)
    {
        onClick -= callBack;
    }

    public void RemoveAllListener()
    {
        onClick = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (onClick != null)
            onClick(gameObject);
    }
}
