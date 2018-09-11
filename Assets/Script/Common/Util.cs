using UnityEngine;
using System.Collections;

public class Util
{

    //这里需要注意一下安卓以外平台上要加file协议。  
    public static string GetStreamingAssetsPath()
    {
        string path = Application.streamingAssetsPath;
        if (Application.platform != RuntimePlatform.Android)
        {
            path = "file://" + path;
        }
        return path;
    }

    private static bool? isnexus6 = null;
    public static bool isNexus6()   //Nexus6前置摄像头的数据规格和常规安卓手机的不一样
    {
        if (isnexus6 == null)
        {
            string tmp = SystemInfo.deviceModel.ToLower();
            if (tmp.Contains("nexus") && tmp.Contains("6"))
                isnexus6 = true;
            else
                isnexus6 = false;
        }
        return isnexus6 == true;
    }

    private static bool? isnexus5x = null;
    public static bool isNexus5X()   //Nexus6前置摄像头的数据规格和常规安卓手机的不一样
    {
        if (isnexus5x == null)
        {
            string tmp = SystemInfo.deviceModel.ToLower();
            if (tmp.Contains("nexus") && tmp.Contains("5x"))
                isnexus5x = true;
            else
                isnexus5x = false;
        }
        return isnexus5x == true;
    }

    public static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();
    public static WaitForFixedUpdate _fixedupdate = new WaitForFixedUpdate();
}
