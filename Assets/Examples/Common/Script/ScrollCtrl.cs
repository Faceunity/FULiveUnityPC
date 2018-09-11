using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScrollCtrl : MonoBehaviour {

    public ScrollRect sr;
    public Graphic arrowleft;
    public Graphic arrowright;
    public Color normalcolor;
    public Color disablecolor;
    public void srmove(bool toleft)
    {
        if (sr)
        {
            float target = sr.horizontalNormalizedPosition;
            if (toleft)
            {
                if(target > 0)
                {
                    target -= 4.0f / sr.content.childCount;
                    sr.horizontalNormalizedPosition = target < 0 ? 0 : target;
                }
            }
            else
            {
                if (target < 1)
                {
                    target += 4.0f / sr.content.childCount;
                    sr.horizontalNormalizedPosition = target > 1 ? 1 : target;
                }
            }
        }
    }
    private void Awake()
    {
        if(sr.horizontalNormalizedPosition == 0)
        {
            arrowleft.color = disablecolor;
            arrowright.color = normalcolor;
        }
        if (sr.horizontalNormalizedPosition == 1)
        {
            arrowleft.color = normalcolor;
            arrowright.color = disablecolor;
        }
        sr.onValueChanged.AddListener(delegate (Vector2 v)
        {
            if(v.x<0.01f)
            {
                arrowleft.color = disablecolor;
                arrowright.color = normalcolor;
            }
            else if(v.x>0.999f)
            {
                arrowleft.color = normalcolor;
                arrowright.color = disablecolor;
            }
        });
    }
}
