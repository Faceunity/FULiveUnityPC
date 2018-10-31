using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class RenderToTexture : MonoBehaviour
{
    //摄像头参数(INPUT)
    public string currentDeviceName;
    public int cameraWidth = 1280;
    public int cameraHeight = 720;
    public int cameraFrameRate = 30;

    WebCamTexture wtex;

    //Camera_TO_SDK
    int[] itemid_tosdk;
    GCHandle itemid_handle;
    IntPtr p_itemsid;

    //byte[] img_bytes;
    Color32[] webtexdata;
    GCHandle img_handle;
    IntPtr p_img_ptr;

    //SDK返回(OUTPUT)
    private int m_fu_texid = 0;
    private Texture2D m_rendered_tex;

    //标记参数
    private bool m_tex_created;

    private bool LoadingItem = false;
    private bool isMirroed;

    //渲染显示UI
    public RawImage RawImg_BackGroud;

    public const int SLOTLENGTH = 10;
    private struct slot_item
    {
        public string name;
        public int id;
    };
    private slot_item[] slot_items;


    public void SwitchCamera(string DeviceName)
    {
        FaceunityWorker.fu_OnCameraChange();

        if (wtex != null && wtex.isPlaying) wtex.Stop();
        currentDeviceName = DeviceName;
        wtex = new WebCamTexture(currentDeviceName, cameraWidth, cameraHeight, cameraFrameRate);
        wtex.Play();
        cameraWidth = wtex.width;
        cameraHeight = wtex.height;

        m_tex_created = false;
        SelfAdjusSize();

        for (int i = 0; i < SLOTLENGTH; i++)
        {
            SetItemMirror(i);
        }
    }

    public void SelfAdjusSize()
    {
        Vector2 targetResolution = RawImg_BackGroud.transform.parent.GetComponent<RectTransform>().rect.size;
        float currentResolutionX = wtex.width;
        float currentResolutionY = wtex.height;
        if(targetResolution.x / targetResolution.y >= currentResolutionX / currentResolutionY)
            RawImg_BackGroud.rectTransform.sizeDelta = new Vector2(targetResolution.y * currentResolutionX / currentResolutionY, targetResolution.y);
        else
            RawImg_BackGroud.rectTransform.sizeDelta = new Vector2(targetResolution.x, targetResolution.x * currentResolutionY / currentResolutionX);
    }

    public void SetMirroed()
    {
        isMirroed = !isMirroed;
        if (isMirroed)
            RawImg_BackGroud.uvRect = new Rect(1, 0, -1, 1);
        else
            RawImg_BackGroud.uvRect = new Rect(0, 0, 1, 1);
    }

    // 初始化摄像头 
    public IEnumerator InitCamera(string DeviceName)
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            SwitchCamera(DeviceName);
        }
    }

    //nama插件使用gles2.0，不支持glGetTexImage，因此只能用ReadPixels来读取数据
    public Texture2D CaptureCamera(Camera[] cameras, Rect rect)
    {
        // 创建一个RenderTexture对象  
        RenderTexture rt = new RenderTexture((int)rect.width, (int)rect.height, 0);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
        foreach (Camera cam in cameras)
        {
            cam.targetTexture = rt;
            cam.Render();
        }
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示  
        foreach (Camera cam in cameras)
        {
            cam.targetTexture = null;
        }
        RenderTexture.active = null; // JC: added to avoid errors  
        Destroy(rt);
        Debug.Log("截屏了一张照片");

        return screenShot;
    }

    //仅仅保存图片，并不通知图库刷新，因此请用文件浏览器在对应路径打开图片
    public void SaveTex2D(Texture2D tex)
    {
        byte[] bytes = tex.EncodeToPNG();
        if (Directory.Exists(Application.persistentDataPath + "/Photoes/") == false)
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/Photoes/");
        }
        string name = Application.persistentDataPath + "/Photoes/" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".png";
        File.WriteAllBytes(name, bytes);
        Debug.Log("保存了一张照片:" + name);
    }

    void Start()
    {
        FaceunityWorker.instance.OnInitOK += InitApplication;
        if (itemid_tosdk == null)
        {
            //默认slot槽长度为SLOTLENGTH=10
            itemid_tosdk = new int[SLOTLENGTH];
            itemid_handle = GCHandle.Alloc(itemid_tosdk, GCHandleType.Pinned);
            p_itemsid = itemid_handle.AddrOfPinnedObject();

            slot_items = new slot_item[SLOTLENGTH];
            for (int i = 0; i < SLOTLENGTH; i++)
            {
                slot_items[i].id = 0;
                slot_items[i].name = "";
            }
        }
    }

    void InitApplication(object source, EventArgs e)
    {
        //StartCoroutine(InitCamera(WebCamTexture.devices[0].name));
    }

    void Update()
    {
        if (wtex != null && wtex.isPlaying)
        {
            // pass data by byte buf, 
            if (wtex.didUpdateThisFrame)
            {
                if (webtexdata==null||webtexdata.Length != wtex.width * wtex.height)
                {
                    if (img_handle.IsAllocated)
                        img_handle.Free();
                    webtexdata = new Color32[wtex.width * wtex.height];
                    img_handle = GCHandle.Alloc(webtexdata, GCHandleType.Pinned);
                    p_img_ptr = img_handle.AddrOfPinnedObject();
                }
                wtex.GetPixels32(webtexdata);
                //Debug.LogFormat("data pixels:{0},img_btyes:{1}",data.Length,img_bytes.Length/4);
                //for (int i = 0; i < webtexdata.Length; i++)
                //{
                //    img_bytes[4 * i] = webtexdata[i].b;
                //    img_bytes[4 * i + 1] = webtexdata[i].g;
                //    img_bytes[4 * i + 2] = webtexdata[i].r;
                //    img_bytes[4 * i + 3] = 1;
                //}
                FaceunityWorker.SetImage(p_img_ptr, isMirroed?32:0, false, wtex.width, wtex.height);   //传输数据方法之一
            }
        }


        if (m_tex_created == false)
        {
            m_fu_texid = FaceunityWorker.fu_GetNamaTextureId();
            if (m_fu_texid > 0)
            {
                m_tex_created = true;
                m_rendered_tex = Texture2D.CreateExternalTexture(wtex.width, wtex.height, TextureFormat.RGBA32, false, true, (IntPtr)m_fu_texid);
                Debug.LogFormat("Texture2D.CreateExternalTexture:{0},width={1},height={2}\n", m_fu_texid, m_rendered_tex.width, m_rendered_tex.height);
                if (RawImg_BackGroud != null)
                {
                    RawImg_BackGroud.texture = m_rendered_tex;
                    RawImg_BackGroud.gameObject.SetActive(true);
                    Debug.Log("m_rendered_tex: " + m_rendered_tex.GetNativeTexturePtr());
                }
            }
        }
    }

    void OnApplicationPause(bool isPause)
    {

        if (isPause)
        {
            Debug.Log("Pause");
            m_tex_created = false;
            //FaceunityWorker.fu_OnDeviceLost();

        }
        else
        {
            Debug.Log("Start");
        }
    }

    #region LoadItem

    public delegate void LoadItemCallback(string message);
    public IEnumerator LoadItem(Item item, int slotid = 0, LoadItemCallback cb=null)
    {
        if (LoadingItem == false && item.fullname != null && item.fullname.Length != 0 && slotid >= 0 && slotid < SLOTLENGTH)
        {
            LoadingItem = true;
            int tempslot = GetSlotIDbyName(item.name);
            if (tempslot < 0)   //如果尚未载入道具数据
            {
                var bundledata = Resources.LoadAsync<TextAsset>(item.fullname);
                yield return bundledata;
                var data = bundledata.asset as TextAsset;
                byte[] bundle_bytes = data != null ? data.bytes : null;
                Debug.LogFormat("bundledata name:{0}, size:{1}", item.name, bundle_bytes.Length);
                GCHandle hObject = GCHandle.Alloc(bundle_bytes, GCHandleType.Pinned);
                IntPtr pObject = hObject.AddrOfPinnedObject();
                yield return FaceunityWorker.fu_CreateItemFromPackage(pObject, bundle_bytes.Length);
                hObject.Free();
                int itemid = FaceunityWorker.fu_getItemIdxFromPackage();

                UnLoadItem(slotid); //卸载上一个在这个slot槽内的道具，并非必要，但是为了节省内存还是清一下

                itemid_tosdk[slotid] = itemid;
                slot_items[slotid].id = itemid;
                slot_items[slotid].name = item.name;

                FaceunityWorker.fu_setItemIds(p_itemsid, SLOTLENGTH, IntPtr.Zero);
                Debug.Log("载入Item：" + item.name + " @slotid=" + slotid);
            }
            else if(tempslot != slotid)    //道具已载入，但是不在请求的slot槽内
            {
                UnLoadItem(slotid);

                itemid_tosdk[slotid] = slot_items[tempslot].id;
                slot_items[slotid].id = slot_items[tempslot].id;
                slot_items[slotid].name = slot_items[tempslot].name;

                itemid_tosdk[tempslot] = 0;
                slot_items[tempslot].id = 0;
                slot_items[tempslot].name = "";

                FaceunityWorker.fu_setItemIds(p_itemsid, SLOTLENGTH, IntPtr.Zero);
                Debug.Log("移动Item：" + item.name + " from tempslot=" + tempslot + " to slotid="+ slotid);
            }
            else    //tempslot == slotid 即重复载入同一个道具进同一个slot槽，直接跳过
            {
                Debug.Log("重复载入Item："+ item.name +"  slotid="+ slotid);
            }
            /*if (item.type == 1)
                flipmark = false;
            else
                flipmark = true;*/
            SetItemMirror(slotid);

            if (cb != null)
                cb(item.name);//触发载入道具完成事件
            
            LoadingItem = false;
        }
    }

    public int GetItemIDbyName(string name)
    {
        for(int i=0;i< SLOTLENGTH; i++)
        {
            if (string.Equals(slot_items[i].name, name))
                return slot_items[i].id;
        }
        return 0;
    }

    public string GetItemNamebySlotID(int slotid)
    {
        if (slotid >= 0 && slotid < SLOTLENGTH)
        {
            return slot_items[slotid].name;
        }
        return "";
    }

    public int GetSlotIDbyName(string name)
    {
        for (int i = 0; i < SLOTLENGTH; i++)
        {
            if (string.Equals(slot_items[i].name, name))
                return i;
        }
        return -1;
    }

    public bool UnLoadItem(string itemname)
    {
        return UnLoadItem(GetSlotIDbyName(itemname));
    }

    public bool UnLoadItem(int slotid)
    {
        if (slotid >= 0 && slotid< SLOTLENGTH)
        {
            if (slot_items[slotid].id == 0)
                return true;
            Debug.Log("UnLoadItem name=" + slot_items[slotid].name+ " slotid="+ slotid);

            FaceunityWorker.fu_DestroyItem(slot_items[slotid].id);
            itemid_tosdk[slotid] = 0;
            slot_items[slotid].id = 0;
            slot_items[slotid].name = "";
            return true;
        }
        Debug.LogWarning("UnLoadItem Faild!!!");
        return false;
    }

    public void UnLoadAllItems()
    {
        Debug.Log("UnLoadAllItems");
        FaceunityWorker.fu_DestroyAllItems();

        for (int i = 0; i < SLOTLENGTH; i++)
        {
            itemid_tosdk[i] = 0;
            slot_items[i] = new slot_item { name = "", id = 0 };
        }
    }

    public void SetItemParamd(string itemname, string paramdname, double value)
    {
        SetItemParamd(GetSlotIDbyName(itemname), paramdname, value);
    }

    public void SetItemParamd(int slotid, string paramdname, double value)
    {
        if (slotid >= 0 && slotid< SLOTLENGTH)
        {
            FaceunityWorker.fu_ItemSetParamd(slot_items[slotid].id, paramdname, value);
        }
    }

    public double GetItemParamd(string itemname, string paramdname)
    {

        return GetItemParamd(GetSlotIDbyName(itemname), paramdname);
    }

    public double GetItemParamd(int slotid, string paramdname)
    {
        if (slotid >= 0 && slotid < SLOTLENGTH)
        {
            return FaceunityWorker.fu_ItemGetParamd(slot_items[slotid].id, paramdname);
        }
        return 0;
    }

    public void SetItemParams(string itemname, string paramdname, string value)
    {
        SetItemParams(GetSlotIDbyName(itemname), paramdname, value);
    }

    public void SetItemParams(int slotid, string paramdname, string value)
    {
        if (slotid >= 0 && slotid < SLOTLENGTH)
        {
            FaceunityWorker.fu_ItemSetParams(slot_items[slotid].id, paramdname, value);
        }
    }

    public string GetItemParams(string itemname, string paramdname)
    {
        return GetItemParams(GetSlotIDbyName(itemname),paramdname);
    }

    public string GetItemParams(int slotid, string paramdname)
    {
        if (slotid >= 0 && slotid < SLOTLENGTH)
        {
            byte[] bytes = new byte[32];
            int i = FaceunityWorker.fu_ItemGetParams(slot_items[slotid].id, paramdname, bytes, 32);
            return System.Text.Encoding.Default.GetString(bytes).Replace("\0", "");
        }
        return "";
    }
    #endregion

    bool flipmark = true;
    public void SetItemMirror(int slotid)
    {
        if (slotid < 0 || slotid >= SLOTLENGTH)
            return;
        int itemid = slot_items[slotid].id;
        if (itemid <= 0)
            return;
        FaceunityWorker.fu_ItemSetParamd(itemid, "camera_change", 1.0);

        int param = isMirroed && flipmark ? 1 : 0;

        //is3DFlipH 参数是用于对3D道具的镜像
        FaceunityWorker.fu_ItemSetParamd(itemid, "is3DFlipH", param);
        //isFlipExpr 参数是用于对人像驱动道具的镜像
        FaceunityWorker.fu_ItemSetParamd(itemid, "isFlipExpr", param);
        //loc_y_flip与loc_x_flip 参数是用于对手势识别道具的镜像
        FaceunityWorker.fu_ItemSetParamd(itemid, "loc_y_flip", param);
        FaceunityWorker.fu_ItemSetParamd(itemid, "loc_x_flip", param);
    }

    private void OnApplicationQuit()
    {
        UnLoadAllItems();
        //这些数据必须常驻，直到应用结束才能释放
        if (itemid_handle != null && itemid_handle.IsAllocated)
        {
            itemid_handle.Free();
        }

        if (img_handle != null && img_handle.IsAllocated)
        {
            img_handle.Free();
        }
    }
}
