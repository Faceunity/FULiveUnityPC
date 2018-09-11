using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SliderWithInputField : MonoBehaviour {
    public Slider sl;
    public InputField ifd;
    private void Awake()
    {
        if (sl && ifd)
        {
            ifd.onEndEdit.AddListener(delegate (string v)
            {
                int x = 0;
                int.TryParse(v, out x);
                x = x < sl.minValue ? (int)sl.minValue : x;
                x = x > sl.maxValue ? (int)sl.maxValue : x;
                sl.value = x;
                ifd.text = sl.value.ToString();
            });
            sl.onValueChanged.AddListener(delegate (float v)
            {
                ifd.text = v.ToString();
            });
        }
    }
}
