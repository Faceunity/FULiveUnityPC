using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoHighlight3 : MonoBehaviour {

    public Color normalcolor;
    public Color highlightcolor;

    public Graphic[] imgs;

    void Awake()
    {
        this.GetComponent<AddPointEnterEvent>().AddListener(delegate
        {
            foreach (var img in imgs)
            {
                img.color = highlightcolor;
            }
        });
        this.GetComponent<AddPointExitEvent>().AddListener(delegate
        {
            foreach (var img in imgs)
            {
                img.color = normalcolor;
            }
        });
    }
}
