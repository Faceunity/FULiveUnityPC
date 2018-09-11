using UnityEngine;
using System.Collections;

public class Sprites : MonoBehaviour {

    public Sprite[] spritelist_0;
    public Sprite[] spritelist_1;
    public Sprite[] spritelist_2;
    public Sprite[] spritelist_3;
    public Sprite[] spritelist_4;
    public Sprite[] spritelist_5;
    public Sprite[] spritelist_6;
    public Sprite[] spritelist_7;
    public Sprite[] spritelist_8;
    public Sprite[] spritelist_9;
    public Sprite[] spritelist_10;
    public Sprite[] spritelist_11;

    Sprite[][] spritelist;

    private void Awake()
    {
        spritelist = new Sprite[12][];
        spritelist[0] = spritelist_0;
        spritelist[1] = spritelist_1;
        spritelist[2] = spritelist_2;
        spritelist[3] = spritelist_3;
        spritelist[4] = spritelist_4;
        spritelist[5] = spritelist_5;
        spritelist[6] = spritelist_6;
        spritelist[7] = spritelist_7;
        spritelist[8] = spritelist_8;
        spritelist[9] = spritelist_9;
        spritelist[10] = spritelist_10;
        spritelist[11] = spritelist_11;
    } 

    public Sprite GetSprite(int type,int id)
    {
        if (type >= 0 && type < spritelist.GetLength(0))
        {
            if (id >= 0 && id < spritelist[type].Length)
                return spritelist[type][id];
            else
                return null;
        }
        else
            return null;
    }
}
