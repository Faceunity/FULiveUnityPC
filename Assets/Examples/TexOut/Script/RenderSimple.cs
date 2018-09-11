using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;

public class RenderSimple : MonoBehaviour {

    //摄像头参数(INPUT)
    public string currentDeviceName;
    public int cameraWidth=1280;
    public int cameraHeight=720;
    public int cameraFrameRate=30;

    WebCamTexture wtex;
    //byte[] img_bytes;
    Color32[] webtexdata;
    GCHandle img_handle;
    IntPtr p_img_ptr;

    byte[] img_nv21;
    GCHandle img_nv21_handle;
    IntPtr p_img_nv21_ptr;

    //SDK返回(OUTPUT)
    int m_fu_texid = 0;
    Texture2D m_rendered_tex;

    //标记参数
    bool m_tex_created;

    //渲染显示UI
    public RawImage RawImg_BackGroud;


    public void SwitchCamera()
    {
        foreach (WebCamDevice device in WebCamTexture.devices)
        {
            if (currentDeviceName != device.name)
            {
                if (wtex != null && wtex.isPlaying) wtex.Stop();
                currentDeviceName = device.name;
                wtex = new WebCamTexture(currentDeviceName, cameraWidth, cameraHeight, cameraFrameRate);
                wtex.Play();
                break;
            }
        }
    }



    // 初始化摄像头 
    public IEnumerator InitCamera()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices == null)
            {
                Debug.Log("No Camera detected!");
            }
            else
            {
                currentDeviceName = devices[0].name;
                wtex = new WebCamTexture(currentDeviceName, cameraWidth, cameraHeight, cameraFrameRate);
                wtex.Play();
            }
        }


        if (img_handle.IsAllocated)
            img_handle.Free();
        webtexdata = new Color32[wtex.width * wtex.height];
        img_handle = GCHandle.Alloc(webtexdata, GCHandleType.Pinned);
        p_img_ptr = img_handle.AddrOfPinnedObject();

        img_nv21 = new byte[wtex.width * wtex.height * 3 / 2];
        img_nv21_handle = GCHandle.Alloc(img_nv21, GCHandleType.Pinned);
        p_img_nv21_ptr = img_nv21_handle.AddrOfPinnedObject();
    }

    void Start()
    {
        FaceunityWorker.instance.OnInitOK += InitApplication;
    }

    void InitApplication(object source, EventArgs e)
    {
        StartCoroutine(InitCamera());
    }

    void Update()
    {
		
        if (wtex != null && wtex.isPlaying)
        {
            if (wtex.didUpdateThisFrame)
            {
				if (webtexdata.Length != wtex.width * wtex.height) {
					if (img_handle.IsAllocated)
						img_handle.Free();
					webtexdata = new Color32[wtex.width * wtex.height];
					img_handle = GCHandle.Alloc(webtexdata, GCHandleType.Pinned);
					p_img_ptr = img_handle.AddrOfPinnedObject();
				}
                wtex.GetPixels32(webtexdata);
                //int[] argb = new int[wtex.width * wtex.height];       //模拟NV21模式
                //for (int i = 0; i < webtexdata.Length; i++)
                //{
                //    argb[i] = 0;
                //    argb[i] |= (webtexdata[i].a << 24);
                //    argb[i] |= (webtexdata[i].r << 16);
                //    argb[i] |= (webtexdata[i].g << 8);
                //    argb[i] |= (webtexdata[i].b);

                //}
                //encodeYUV420SP(img_nv21, argb, wtex.width, wtex.height);
                //UpdateData(p_img_nv21_ptr, 0, wtex.width, wtex.height);
                //UpdateData(p_img_nv21_ptr, (int)wtex.GetNativeTexturePtr(), wtex.width, wtex.height);
                UpdateData(p_img_ptr, 0, wtex.width, wtex.height);
                //UpdateData(IntPtr.Zero, (int)wtex.GetNativeTexturePtr(), wtex.width, wtex.height);
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (img_handle != null && img_handle.IsAllocated)
        {
            img_handle.Free();
        }
        if (img_nv21_handle != null && img_nv21_handle.IsAllocated)
        {
            img_nv21_handle.Free();
        }
    }


    void OnApplicationPause(bool isPause)
    {
        if (isPause)
        {
            Debug.Log("Pause");
            m_tex_created = false;
        }
        else
        {
            Debug.Log("Start");
        }
    }

    //输入数据接口案例
    public void UpdateData(IntPtr ptr,int texid,int w,int h)
    {
        //FaceunityWorker.SetNV21Input(ptr, 0, w, h);
        //FaceunityWorker.SetDualInput(ptr, texid, 0, w, h);
        FaceunityWorker.SetImage(ptr,0, false, w, h);   //传输数据方法之一
        //FaceunityWorker.SetImageTexId(texid, 0, w, h);
        if (m_tex_created == false)
        {
            m_fu_texid = FaceunityWorker.fu_GetNamaTextureId();
            if (m_fu_texid > 0)
            {
                m_rendered_tex = Texture2D.CreateExternalTexture(w, h, TextureFormat.RGBA32, false, true, (IntPtr)m_fu_texid);
                Debug.LogFormat("Texture2D.CreateExternalTexture:{0}\n", m_fu_texid);
                if (RawImg_BackGroud != null)
                {
                    RawImg_BackGroud.texture = m_rendered_tex;
                    RawImg_BackGroud.gameObject.SetActive(true);
                    Debug.Log("m_rendered_tex: " + m_rendered_tex.GetNativeTexturePtr());
                }
                m_tex_created = true;
            }
        }
    }

    // untested function
    // byte[] yuv = new byte[inputWidth * inputHeight * 3 / 2];
    //    encodeYUV420SP(yuv, argb, inputWidth, inputHeight);
    void encodeYUV420SP(byte[] yuv420sp, int[] argb, int width, int height)
    {
        int frameSize = width * height;

        int yIndex = 0;
        int uvIndex = frameSize;

        int a, R, G, B, Y, U, V;
        int index = 0;
        for (int j = 0; j < height; j++)
        {
            for (int i = 0; i < width; i++)
            {

                a = (int)(argb[index] & 0xff000000) >> 24; // a is not used obviously
                R = (argb[index] & 0xff0000) >> 16;
                G = (argb[index] & 0xff00) >> 8;
                B = (argb[index] & 0xff) >> 0;

                // well known RGB to YUV algorithm
                Y = ((66 * R + 129 * G + 25 * B + 128) >> 8) + 16;
                U = ((-38 * R - 74 * G + 112 * B + 128) >> 8) + 128;
                V = ((112 * R - 94 * G - 18 * B + 128) >> 8) + 128;

                // NV21 has a plane of Y and interleaved planes of VU each sampled by a factor of 2
                //    meaning for every 4 Y pixels there are 1 V and 1 U.  Note the sampling is every other
                //    pixel AND every other scanline.
                yuv420sp[yIndex++] = (byte)((Y < 0) ? 0 : ((Y > 255) ? 255 : Y));
                if (j % 2 == 0 && index % 2 == 0)
                {
                    yuv420sp[uvIndex++] = (byte)((V < 0) ? 0 : ((V > 255) ? 255 : V));
                    yuv420sp[uvIndex++] = (byte)((U < 0) ? 0 : ((U > 255) ? 255 : U));
                }

                index++;
            }
        }
    }
}
