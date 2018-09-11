using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoSizeLayoutPadding : MonoBehaviour {

    public GridLayoutGroup lg;
    public RectTransform rt;

    void RePadding()
    {
        float contentwidth = rt.rect.width;
        Vector2 cellseize = lg.cellSize;
        Vector2 spacing = lg.spacing;
        int column = (int)((contentwidth + spacing.x) / (cellseize.x + spacing.x));
        int left = (int)((contentwidth - column * cellseize.x - (column - 1) * spacing.x)*0.5f);
        lg.padding = new RectOffset(left, left, lg.padding.top, lg.padding.bottom);
    }

    private void Start()
    {
        RePadding();
    }
}
