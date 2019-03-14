using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine.Rendering;

public class FaceunityWorker : MonoBehaviour
{
    public class CFaceUnityCoefficientSet
    {
        public float[] m_data;
        public GCHandle m_handle;
        public IntPtr m_addr;
        public string m_name;
        public int m_addr_size;
        public int m_faceId = 0;

        public CFaceUnityCoefficientSet(string s, int num, int faceId = 0)
        {
            m_name = s;
            m_addr_size = num;
            m_faceId = faceId;
        }
        ~CFaceUnityCoefficientSet()
        {
            if (m_handle != null && m_handle.IsAllocated)
            {
                m_handle.Free();
                m_data = default(float[]);
            }
        }
        public void Update()
        {
            if (m_data == default(float[]))
            {
                m_data = new float[m_addr_size];
                m_handle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
                m_addr = m_handle.AddrOfPinnedObject();
            }
            fu_GetFaceInfo(m_faceId, m_addr, m_addr_size, m_name);
        }
    }

    // Unity editor doesn't unload dlls after 'preview'

    #region DllImport

    /////////////////////////////////////
    //native interfaces

    /**
	\brief Initialize and authenticate your SDK instance to the FaceUnity server, must be called exactly once before all other functions.
		The buffers should NEVER be freed while the other functions are still being called.
		You can call this function multiple times to "switch pointers".
	\param v3data should point to contents of the "v3.bin" we provide
	\param ardata should be NULL
	\param authdata is the pointer to the authentication data pack we provide. You must avoid storing the data in a file.
		Normally you can just `#include "authpack.h"` and put `g_auth_package` here.
	\param sz_authdata is the authenticafu_Cleartion data size, we use plain int to avoid cross-language compilation issues.
		Normally you can just `#include "authpack.h"` and put `sizeof(g_auth_package)` here.
	\return non-zero for success, zero for failure
	*/

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_Setup(IntPtr databuf, IntPtr licbuf, int licbuf_sz);


    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_LoadExtendedARData(IntPtr databuf, int databuf_sz);


    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_LoadAnimModel(IntPtr databuf, int databuf_sz);

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_LoadTongueModel(IntPtr databuf, int databuf_sz);

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_SetExpressionCalibration(int enable);


    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern void fu_setItemDataFromPackage(IntPtr databuf, int databuf_sz);


    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_getItemIdxFromPackage();

    /**
\brief Destroy an accessory item.
    This function MUST be called in the same GLES context / thread as the original fuCreateItemFromPackage.
\param item is the handle to be destroyed
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_DestroyItem(int itemid);

    /**
\brief Destroy all accessory items ever created.
    This function MUST be called in the same GLES context / thread as the original fuCreateItemFromPackage.
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_DestroyAllItems();

    /**
	\brief Render a list of items on top of a GLES texture or a memory buffer.
		This function needs a GLES 2.0+ context. 
		Render will do in PluginEvent fu_GetRenderEventFunc
	\param idxbuf points to the list of items
	\param idxbuf_sz is the number of items
	*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_setItemIds(IntPtr idxbuf, int idxbuf_sz, IntPtr mask);//mask can be null

    /**
\brief Set the default rotationMode.
\param rotationMode is the default rotationMode to be set to, one of 0..3 should work.
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_SetDefaultRotationMode(int i);

    /**
\brief Get certificate permission code for modules
\param i - get i-th code, currently available for 0 and 1
\return The permission code
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_GetModuleCode(int i);

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_SetASYNCTrackFace(int i);

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_SetRuningMode(int runningMode);//refer to FURuningMode 

    /**
\brief Create Tex For Item
\param item specifies the item
\param name is the tex name
\param value is the tex rgba buffer to be set ,use GCHandle to get ptr
\param width is the tex width
\param height is the tex height
\return zero for failure, non-zero for success
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_CreateTexForItem(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name, IntPtr value, int width, int height);

    /**
\brief Delete Tex For Item
\param item specifies the item
\param name is the parameter name
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_DeleteTexForItem(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name);

    /**
\brief Set an item parameter to a double value
\param item specifies the item
\param name is the parameter name
\param value is the parameter value to be set
\return zero for failure, non-zero for success
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_ItemSetParamd(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name, double value);

    /**
\brief Set an item parameter to a double array
\param item specifies the item
\param name is the parameter name
\param value points to an array of doubles
\param n specifies the number of elements in value
\return zero for failure, non-zero for success
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_ItemSetParamdv(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name, IntPtr value, int value_sz);

    /**
\brief Set an item parameter to a string value
\param item specifies the item
\param name is the parameter name
\param value is the parameter value to be set
\return zero for failure, non-zero for success
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_ItemSetParams(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]string value);

    /**
\brief Set an item parameter to a string value
\param item specifies the item
\param name is the parameter name
\param value is the parameter value to be set
\return zero for failure, non-zero for success
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern double fu_ItemGetParamd(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name);

    /**
\brief Get an item parameter as a string
\param item specifies the item
\param name is the parameter name
\param buf receives the string value
\param sz is the number of bytes available at buf
\return the length of the string value, or -1 if the parameter is not a string.
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_ItemGetParams(int itemid, [MarshalAs(UnmanagedType.LPStr)]string name, [MarshalAs(UnmanagedType.LPStr)]byte[] buf, int buf_sz);

    /**
\brief Set the default orientation for face detection. The correct orientation would make the initial detection much faster.
\param rmode is the default orientation to be set to, one of 0..3 should work.
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_SetDefaultOrientation(int rmode);

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr fu_GetRenderEventFunc();

    /**
* if plugin inited
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int jc_part_inited();

    /**
* SetUseNatCam(1);
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetUseNatCam(int enable);

    /**
* if true,Pause the render pipeline
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void SetPauseRender(bool ifpause);

    /**
\brief Get the face tracking status
\return The number of valid faces currently being tracked
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_IsTracking();

    /**
\brief Set the maximum number of faces we track. The default value is 1.
\param n is the new maximum number of faces to track
\return The previous maximum number of faces tracked
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_SetMaxFaces(int num);

    /**
\brief Set the quality-performance tradeoff. 
\param quality is the new quality value. 
       It's a floating point number between 0 and 1.
       Use 0 for maximum performance and 1 for maximum quality.
       The default quality is 1 (maximum quality).
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_SetQualityTradeoff(float num);

    /**
\brief Get SDK version string
\return SDK version string in const char*
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr fu_GetVersion(); // Marshal.PtrToStringAnsi(fu_GetVersion());

    /**
\brief Get system error, which causes system shutting down
\return System error code represents one or more errors	
	Error code can be checked against following bitmasks, non-zero result means certain error
	This interface is not a callback, needs to be called on every frame and check result, no cost
	Inside authentication error (NAMA_ERROR_BITMASK_AUTHENTICATION), meanings for each error code are listed below:
	1 failed to seed the RNG
	2 failed to parse the CA cert
	3 failed to connect to the server
	4 failed to configure TLS
	5 failed to parse the client cert
	6 failed to parse the client key
	7 failed to setup TLS
	8 failed to setup the server hostname
	9 TLS handshake failed
	10 TLS verification failed
	11 failed to send the request
	12 failed to read the response
	13 bad authentication response
	14 incomplete authentication palette info
	15 not inited yet
	16 failed to create a thread
	17 authentication package rejected
	18 void authentication data
	19 bad authentication package
	20 certificate expired
	21 invalid certificate
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_GetSystemError();

    /**
\brief Interpret system error code
\param code - System error code returned by fuGetSystemError()
\return One error message from the code
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr fu_GetSystemErrorString(int code); // Marshal.PtrToStringAnsi();

    /**
\brief Call this function when the GLES context has been lost and recreated.
    That isn't a normal thing, so this function could leak resources on each call.
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_OnDeviceLost();

    /**
\brief Call this function to reset the face tracker on camera switches
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_OnCameraChange();


    /**
\brief clear camera frame data
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void ClearImages();

    /**
\brief provide camera frame data
        flags: FU_ADM_FLAG_FLIP_X = 32;
               FU_ADM_FLAG_FLIP_Y = 64; 翻转只翻转道具渲染，并不会翻转整个图像
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetImage(IntPtr imgbuf, int flags, bool isbgra, int w, int h);

    /**
\brief provide camera frame data android nv21 and texture id
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetDualInput(IntPtr nv21buf, int texid, int flags, int w, int h);

    /**
\brief provide camera frame data android nv21,only support Android.
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetNV21Input(IntPtr nv21buf, int flags, int w, int h);

    /**
\brief provide camera frame data via texid
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int SetImageTexId(int texid, int flags, int w, int h);

    /**
\brief Enable internal Log
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_EnableLog(bool isenable);

    /**
\brief get Rendered texture id, can be recreated in unity
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_GetNamaTextureId();

    /**
  \brief Get the unique identifier for each face during current tracking
    Lost face tracking will change the identifier, even for a quick retrack
  \param face_id is the id of face, index is smaller than which is set in fuSetMaxFaces
    If this face_id is x, it means x-th face currently tracking
  \return the unique identifier for each face
  */
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_GetFaceIdentifier(int face_id);

    /**
\brief Scale the rendering perspectivity (focal length, or FOV)
   Larger scale means less projection distortion
   This scale should better be tuned offline, and set it only once in runtime
\param scale - default is 1.f, keep perspectivity invariant
   <= 0.f would be treated as illegal input and discard	
*/
    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern void fu_SetFocalLengthScale(float scale);


    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    private static extern void RegisterDebugCallback(DebugCallback callback);

    [DllImport("faceplugin", CallingConvention = CallingConvention.Cdecl)]
    public static extern int fu_GetFaceInfo(int face_id, IntPtr ret, int szret, [MarshalAs(UnmanagedType.LPStr)]string name);

    #endregion

    /**
 \brief Create an accessory item from a binary package, you can discard the data after the call.
     This function MUST be called in the same GLES context / thread as fuRenderItems.
 \param data is the pointer to the data
 \param sz is the data size, we use plain int to avoid cross-language compilation issues
 \return an integer handle represefu_Clearnting the item
 */
    public static IEnumerator fu_CreateItemFromPackage(IntPtr databuf, int databuf_sz)
    {
        fu_setItemDataFromPackage(databuf, databuf_sz);
        //GL.IssuePluginEvent(fu_GetRenderEventFunc(), 101);
        yield return Util._endOfFrame;
        yield return Util._endOfFrame;   //等待道具异步加载完毕
    }

    public enum FURuningMode
    {
        FU_Mode_None = 0,
        FU_Mode_RenderItems, //face tracking and render item (beautify is one type of item) ,item means 'daoju'
        FU_Mode_Beautification,//non face tracking, beautification only.
        FU_Mode_Masked,//when tracking multi-people, different perple　can use different item, give mask in function fu_setItemIds  
        FU_Mode_TrackFace//tracking face only, get face infomation, but do not render item.it's very fast.
    };

    public static FURuningMode currentMode = FURuningMode.FU_Mode_None;

    public static void SetRunningMode(FURuningMode mode)
    {
        currentMode = mode;
        fu_SetRuningMode((int)mode);
    }

    ///////////////////////////////
    //public int m_camera_native_texid = 0;
    //singleton checks
    public static FaceunityWorker instance = null;
    void Awake()
    {
        //singleton checks
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

    }
    ///////////////////////////////
    //persistent data, DO NOT EVER FREE ANY OF THESE!
    //we must keep the GC handles to keep the arrays pinned to the same addresses
    [HideInInspector]
    public bool m_plugin_inited = false;

    public int MAXFACE = 1;
    public bool EnableExpressionLoop = true;
    public string LICENSE = "";

    [HideInInspector]
    public int m_need_blendshape_update = 0;
    public CFaceUnityCoefficientSet[] m_translation;// = new CFaceUnityCoefficientSet("translation", 3); //3D translation of face in camera space - 3 float
    public CFaceUnityCoefficientSet[] m_rotation;// = new CFaceUnityCoefficientSet("rotation", 4); //rotation quaternion - 4 float
    public CFaceUnityCoefficientSet[] m_rotation_mode;// = new CFaceUnityCoefficientSet("rotation_mode", 1); //the relative orientaion of face agains phone, 0-3 - 1 float
    public CFaceUnityCoefficientSet[] m_expression;// = new CFaceUnityCoefficientSet("expression", 46);
    //public CFaceUnityCoefficientSet[] m_landmarks;// =new CFaceUnityCoefficientSet("landmarks",75*2); //2D landmarks coordinates in image space - 75*2 float
    //public CFaceUnityCoefficientSet[] m_landmarks_ar;// =new CFaceUnityCoefficientSet("landmarks_ar",75*3); //3D landmarks coordinates in camera space - 75*3 float
    //public CFaceUnityCoefficientSet[] m_projection_matrix;// =new CFaceUnityCoefficientSet("projection_matrix",16); //the transform matrix from camera space to image space - 16 float
    //public CFaceUnityCoefficientSet[] m_eye_rotation;// =new CFaceUnityCoefficientSet("eye_rotation",4); //eye rotation quaternion - 4 float
    //public CFaceUnityCoefficientSet[] m_face_rect;// =new CFaceUnityCoefficientSet("face_rect",4); //the rectangle of tracked face in image space, (xmin,ymin,xmax,ymax) - 4 float
    //public CFaceUnityCoefficientSet[] m_failure_rate;// =new CFaceUnityCoefficientSet("failure_rate",1); //the failure rate of face tracking, the less the more confident about tracking result - 1 float
    public CFaceUnityCoefficientSet[] m_pupil_pos;// = new CFaceUnityCoefficientSet("pupil_pos", 4);
    public CFaceUnityCoefficientSet[] m_focallength;// = new CFaceUnityCoefficientSet("focal_length", 1);

    public static float FocalLengthScale = 1f;

    public event EventHandler OnInitOK;
    private delegate void DebugCallback(string message);

    ///////////////////////////////
    void InitCFaceUnityCoefficientSet()
    {
        m_translation = new CFaceUnityCoefficientSet[MAXFACE];
        m_rotation = new CFaceUnityCoefficientSet[MAXFACE];
        m_rotation_mode = new CFaceUnityCoefficientSet[MAXFACE];
        m_expression = new CFaceUnityCoefficientSet[MAXFACE];
        m_pupil_pos = new CFaceUnityCoefficientSet[MAXFACE];
        m_focallength = new CFaceUnityCoefficientSet[MAXFACE];
        //m_landmarks = new CFaceUnityCoefficientSet[MAXFACE];

        for (int i = 0; i < MAXFACE; i++)
        {
            m_translation[i] = new CFaceUnityCoefficientSet("translation", 3, i);
            m_rotation[i] = new CFaceUnityCoefficientSet("rotation", 4, i);
            m_rotation_mode[i] = new CFaceUnityCoefficientSet("rotation_mode", 1, i);
            m_expression[i] = new CFaceUnityCoefficientSet("expression", 46, i);
            m_pupil_pos[i] = new CFaceUnityCoefficientSet("pupil_pos", 4, i);
            m_focallength[i] = new CFaceUnityCoefficientSet("focal_length", 1, i);
            //m_landmarks[i] = new CFaceUnityCoefficientSet("landmarks", 75 * 2, i);
        }
    }

    //working methods
    IEnumerator Start()
    {
        if (EnvironmentCheck())
        {
            Debug.Log("jc_part_inited:   " + jc_part_inited());
            if (m_plugin_inited == false)
            {
                Debug.LogFormat("FaceunityWorker Init");

                //RegisterDebugCallback(new DebugCallback(DebugMethod));
                fu_EnableLog(false);
                ClearImages();

                //fu_Setup init nama sdk
                if (jc_part_inited() == 0)  //防止Editor下二次Play导致崩溃的bug
                {
                    //load license data
                    if (LICENSE == null || LICENSE == "")
                    {
                        Debug.LogError("LICENSE is null! please paste the license data to the TextField named \"LICENSE\" in FaceunityWorker");
                    }
                    else
                    {
                        sbyte[] m_licdata_bytes;
                        GCHandle m_licdata_handle;
                        string[] sbytes = LICENSE.Split(',');
                        if (sbytes.Length <= 7)
                        {
                            Debug.LogError("License Format Error");
                        }
                        else
                        {
                            m_licdata_bytes = new sbyte[sbytes.Length];
                            Debug.LogFormat("length:{0}", sbytes.Length);
                            for (int i = 0; i < sbytes.Length; i++)
                            {
                                //Debug.Log(sbytes[i]);
                                m_licdata_bytes[i] = sbyte.Parse(sbytes[i]);
                                //Debug.Log(m_licdata_bytes[i]);
                            }
                            m_licdata_handle = GCHandle.Alloc(m_licdata_bytes, GCHandleType.Pinned);
                            IntPtr licptr = m_licdata_handle.AddrOfPinnedObject();

                            //load nama sdk data
                            string fnv3 = Util.GetStreamingAssetsPath() + "/faceunity/v3.bytes";
                            WWW v3data = new WWW(fnv3);
                            yield return v3data;
                            byte[] m_v3data_bytes = v3data.bytes;
                            GCHandle m_v3data_handle = GCHandle.Alloc(m_v3data_bytes, GCHandleType.Pinned); //pinned avoid GC
                            IntPtr dataptr = m_v3data_handle.AddrOfPinnedObject(); //pinned addr

                            fu_Setup(dataptr, licptr, sbytes.Length); //要查看license是否有效请打开插件log（fu_EnableLog(true);）

                            m_licdata_handle.Free();
                            m_v3data_handle.Free();
                            m_plugin_inited = true;
                        }
                    }
                }
                else
                {
                    fu_OnDeviceLost();  //清理残余，防止崩溃
                    m_plugin_inited = true;
                }

                if (m_plugin_inited == true)
                {
                    string ardata_ex = Util.GetStreamingAssetsPath() + "/faceunity/ardata_ex.bytes";    //高精度AR数据
                    WWW ardata_exdata = new WWW(ardata_ex);
                    yield return ardata_exdata;
                    byte[] ardata_exdata_bytes = ardata_exdata.bytes;
                    GCHandle ardata_exdata_handle = GCHandle.Alloc(ardata_exdata_bytes, GCHandleType.Pinned);
                    IntPtr ardata_exdataptr = ardata_exdata_handle.AddrOfPinnedObject();
                    fu_LoadExtendedARData(ardata_exdataptr, ardata_exdata_bytes.Length);
                    ardata_exdata_handle.Free();

                    string anim_model = Util.GetStreamingAssetsPath() + "/faceunity/anim_model.bytes";    //优化面部跟踪数据
                    WWW anim_modeldata = new WWW(anim_model);
                    yield return anim_modeldata;
                    byte[] anim_model_bytes = anim_modeldata.bytes;
                    GCHandle anim_model_handle = GCHandle.Alloc(anim_model_bytes, GCHandleType.Pinned);
                    IntPtr anim_modeldataptr = anim_model_handle.AddrOfPinnedObject();
                    fu_LoadAnimModel(anim_modeldataptr, anim_model_bytes.Length);
                    anim_model_handle.Free();

                    string tongue = Util.GetStreamingAssetsPath() + "/faceunity/tongue.bytes";    //舌头检测
                    WWW tonguedata = new WWW(tongue);
                    yield return tonguedata;
                    byte[] tongue_bytes = tonguedata.bytes;
                    GCHandle tongue_handle = GCHandle.Alloc(tongue_bytes, GCHandleType.Pinned);
                    IntPtr tonguedataptr = tongue_handle.AddrOfPinnedObject();
                    fu_LoadTongueModel(tonguedataptr, tongue_bytes.Length);
                    tongue_handle.Free();

                    SetRunningMode(FURuningMode.FU_Mode_RenderItems);   //默认模式，随时可以改
                    fu_SetFocalLengthScale(FocalLengthScale);   //默认值是1
                    Debug.LogFormat("fu_SetFocalLengthScale({0})", FocalLengthScale);

                    if (OnInitOK != null)
                        OnInitOK(this, null);//触发初始化完成事件

                    if (EnableExpressionLoop)
                        InitCFaceUnityCoefficientSet();

                    //Debug.Log("错误：" + fu_GetSystemError() +","+ Marshal.PtrToStringAnsi(fu_GetSystemErrorString(fu_GetSystemError())));
                    Debug.Log("SDK Version:" + Marshal.PtrToStringAnsi(fu_GetVersion()));

                    yield return StartCoroutine("CallPluginAtEndOfFrames");
                }
            }
        }
        else
        {
            Debug.LogError("please check your Graphics API,this plugin only supports OpenGL!");
            yield return null;
        }
    }
    private IEnumerator CallPluginAtEndOfFrames()
    {
        while (true)
        {
            yield return Util._endOfFrame;
            ////////////////////////////////
            fu_SetMaxFaces(MAXFACE);
            GL.IssuePluginEvent(fu_GetRenderEventFunc(), 1);// cal for sdk render
            if (EnableExpressionLoop)
            {
                //only update other stuff when there is new data
                int num = fu_IsTracking();
                m_need_blendshape_update = num < MAXFACE ? num : MAXFACE;
                for (int i = 0; i < m_need_blendshape_update; i++)
                {
                    m_translation[i].Update();
                    m_rotation[i].Update();
                    m_rotation_mode[i].Update();
                    m_expression[i].Update();
                    //m_landmarks[i].Update();

                    m_pupil_pos[i].Update();
                    m_focallength[i].Update();
                    //Debug.Log("m_focallength["+ i + "]=" + m_focallength[i].m_data[0]);
                    ////////////////////////
                    //post-process the coefficients
                    m_expression[i].m_data[6] = m_expression[i].m_data[7] = m_pupil_pos[i].m_data[0];
                    m_expression[i].m_data[10] = m_expression[i].m_data[11] = -m_pupil_pos[i].m_data[0];

                    m_expression[i].m_data[12] = m_expression[i].m_data[13] = m_pupil_pos[i].m_data[1];
                    m_expression[i].m_data[4] = m_expression[i].m_data[5] = -m_pupil_pos[i].m_data[1];
                }
            }
        }
    }

    private static void DebugMethod(string message)
    {
        Debug.Log("From Dll: " + message);
    }

    private void OnApplicationQuit()
    {
        if (m_plugin_inited == true)
            fu_OnDeviceLost();
        ClearImages();
    }

    bool EnvironmentCheck()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGL2)
            return true;
        else
            return false;
#elif UNITY_STANDALONE_OSX||UNITY_EDITOR_OSX
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGL2)
            return true;
        else
            return false;
#elif UNITY_ANDROID
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
            return true;
        else
            return false;
#elif UNITY_IOS
        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3
            || SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2)
            return true;
        else
            return false;
#endif
    }
}
