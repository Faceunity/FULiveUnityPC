using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoHighlight : MonoBehaviour {

    public Toggle tg;
    public Graphic[] offs;
    public GameObject on;
    public Color normalcolor;
    public Color highlightcolor;

    private void Awake()
    {
        if (tg && on)
        {
            foreach (var off in offs)
                off.gameObject.SetActive(!tg.isOn);
            on.SetActive(tg.isOn);

            tg.onValueChanged.AddListener(delegate (bool value)
            {
                if (value)
                {
                    foreach (var off in offs)
                        off.gameObject.SetActive(false);
                    on.SetActive(true);
                }
                else
                {
                    foreach (var off in offs)
                        off.gameObject.SetActive(true);
                    on.SetActive(false);
                }
            });

            this.GetComponent<AddPointEnterEvent>().AddListener(delegate
            {
                foreach (var off in offs)
                    off.color = highlightcolor;
            });

            this.GetComponent<AddPointExitEvent>().AddListener(delegate
            {
                foreach (var off in offs)
                    off.color = normalcolor;
            });
        }
    }
}
