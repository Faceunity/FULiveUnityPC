using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public class UIManagerForTexOut : MonoBehaviour
{

    RenderToTexture rtt;

    public GameObject Canvas_FrontUI; //前景UI
    public Camera Camera_FrontUI;

    //Area_0
    public GameObject Stats;
    public Text Statsinfo;
    public GameObject CloseStats;
    public GameObject OpenStats;
    public GameObject CameraDetect;
    public GameObject MarkText;
    public Text marktext;

    public GameObject ItemContent;
    public Transform itemcontent;
    public ToggleGroup Item_ToggleGroup;
    public GameObject Item_UIExample;

    public GameObject ItemSelector;
    public Toggle[] ItemSelectors;

    //Area_1
    public Dropdown CamereDropdown;
    public Toggle VirtualCamera;
    public GameObject OpenHelp;
    public GameObject SetMirroed;

    //Area_2
    public Toggle[] BeautySelectors;
    public GameObject area_beauty;
    public GameObject Button_reset;
    public GameObject[] beautyskins;
    public GameObject[] beautyshapes;
    public GameObject area_filter_style;
    public GameObject value_filter;
    public Slider filterslider;
    public GameObject[] filters;
    public GameObject[] styles;



    Coroutine musiccor = null;
    AudioSource audios;

    string BeautySkinItemName;
    bool needupdateinfo;
    int currentBeautyShape = -1;
    float[] BeautyShapeValue_0 = { 40, 40 };
    float[] BeautyShapeValue_1 = { 40, 40 };

    enum BeautySkinType
    {
        None = 0,
        BeautySkin = 1,
        BeautyShape,
        Filter,
        Style,
    }
    BeautySkinType currentBeautySkinType = BeautySkinType.None;


    enum ItemType
    {
        None = -1,
        Beauty = 0,
        Animoji = 1,
        ItemSticker,
        ARMask,
        ChangeFace,
        ExpressionRecognition,
        MusicFilter,
        BackgroundSegmentation,
        GestureRecognition,
        MagicMirror,
        PortraitLightEffect,
        PortraitDrive,
        MakeUp,
    }
    ItemType currentItemType = ItemType.None;
    string currentItemName = "";

    private static int[] permissions_code = {
            0x1,                    //美颜
            0x10,                   //Animoji
            0x2 | 0x4,              //道具贴纸
            0x20 | 0x40,            //AR面具
            0x80,                   //换脸
            0x800,                  //表情识别
            0x20000,                //音乐滤镜
            0x100,                  //背景分割
            0x200,                  //手势识别
            0x10000,                //哈哈镜
            0x4000,                 //人像光效
            0x8000,                 //人像驱动
            0x80000                 //美妆
    };
    private static bool[] permissions;
    Sprites uisprites;

    enum SlotForItems   //道具的槽位
    {
        Beauty = 0,
        Item = 1,
    };

    void Awake()
    {
        rtt = GetComponent<RenderToTexture>();
        audios = GetComponent<AudioSource>();
        uisprites = GetComponent<Sprites>();
    }

    void Start()
    {
        FaceunityWorker.instance.OnInitOK += InitApplication;
        CloseItemUI();
    }

    void InitApplication(object source, EventArgs e)
    {
        StartCoroutine(Authentication());
    }

    float deltaTime = 0.0f;

    float marktextTime = 0.0f;
    bool trackingface = false;
    int currentBeautyFilter = -1;

    void SetMarkText(string s,float time)
    {
        marktext.text = s;
        MarkText.SetActive(true);
        marktextTime = time;
    }

    void Update()
    {
        if (FaceunityWorker.fu_IsTracking() > 0)
        {
            trackingface = true;
            marktextTime -= Time.deltaTime;
            if (marktextTime <= 0)
            {
                MarkText.SetActive(false);
            }
        }
        else
        {
            SetMarkText("未识别到人脸",0);
        }

        if (needupdateinfo)
        {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
            Statsinfo.text = string.Format("FPS:{0:0.}\nResolution:{1:D}*{2:D}\nRenderTime:{3:0.}ms", 1.0f / deltaTime, rtt.cameraWidth, rtt.cameraHeight, deltaTime*1000);
        }
        
    }

    IEnumerator Authentication()
    {
        while (FaceunityWorker.jc_part_inited() == 0)
            yield return Util._endOfFrame;
        int code = FaceunityWorker.fu_GetModuleCode(0);
        Debug.Log("fu_GetModuleCode:" + code);
        permissions = new bool[permissions_code.Length];
        for (int i = 0; i < permissions_code.Length; i++)
        {
            if ((code & permissions_code[i]) == permissions_code[i])
            {
                permissions[i] = true;
            }
            else
            {
                permissions[i] = false;
                Debug.Log("权限未获取:" + permissions_code[i]);
            }
        }
        if (permissions[0])
        {
            yield return rtt.LoadItem(ItemConfig.beautySkin[0], (int)SlotForItems.Beauty);
            BeautySkinItemName = ItemConfig.beautySkin[0].name;
            for (int i = 0; i < BeautyConfig.beautySkin_1.Length; i++)
            {
                rtt.SetItemParamd(BeautySkinItemName, BeautyConfig.beautySkin_1[i].paramword, BeautyConfig.beautySkin_1[i].defaultvalue);
            }
            for (int i = 0; i < BeautyConfig.beautySkin_2.Length; i++)
            {
                rtt.SetItemParamd(BeautySkinItemName, BeautyConfig.beautySkin_2[i].paramword, BeautyConfig.beautySkin_2[i].defaultvalue);
            }
        }
        RegisterUIFunc();
    }

    void InitCamera()
    {
        CamereDropdown.ClearOptions();
        WebCamDevice[] devices = WebCamTexture.devices;
        var tempNames = new List<string>();
        if (devices.Length == 0)
        {
            tempNames.Add("无摄像头");
            CamereDropdown.AddOptions(tempNames);
            CamereDropdown.interactable = false;
            CameraDetect.SetActive(true);
        }
        else
        {
            for (int i = 0; i < devices.Length; i++)
            {
                tempNames.Add(devices[i].name);
            }
            CamereDropdown.AddOptions(tempNames);
            CamereDropdown.onValueChanged.AddListener(delegate (int v)
            {
                rtt.SwitchCamera(CamereDropdown.options[v].text);
            });
            StartCoroutine(rtt.InitCamera(devices[0].name));
        }
    }

    //for循环配合delegate是个坑，小心。
    void RegisterUIFunc()
    {
        InitCamera();
        OpenHelp.GetComponent<AddClickEvent>().AddListener(delegate
        {
            string path = Application.streamingAssetsPath + "/readme.md";
            //string path = @"F:\FUDemo-Unity-PC\Assets\StreamingAssets\test.txt";
            path = path.Replace("/", @"\");
            System.Diagnostics.Process.Start("explorer.exe", path);
        });
        SetMirroed.GetComponent<AddClickEvent>().AddListener(delegate
        {
            rtt.SetMirroed();
        });
        ItemSelectors[0].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.Animoji);
            else
                CloseItemUI();
        });
        ItemSelectors[1].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.ItemSticker);
            else
                CloseItemUI();
        });
        ItemSelectors[2].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.ARMask);
            else
                CloseItemUI();
        });
        ItemSelectors[3].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.ChangeFace);
            else
                CloseItemUI();
        });
        ItemSelectors[4].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.ExpressionRecognition);
            else
                CloseItemUI();
        });
        ItemSelectors[5].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.MusicFilter);
            else
                CloseItemUI();
        });
        ItemSelectors[6].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.BackgroundSegmentation);
            else
                CloseItemUI();
        });
        ItemSelectors[7].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.GestureRecognition);
            else
                CloseItemUI();
        });
        ItemSelectors[8].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.MagicMirror);
            else
                CloseItemUI();
        });
        ItemSelectors[9].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
                OpenItemsUI(ItemType.PortraitDrive);
            else
                CloseItemUI();
        });
        BeautySelectors[0].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                foreach (var go in beautyskins)
                {
                    go.SetActive(true);
                }
                foreach (var go in beautyshapes)
                {
                    go.SetActive(false);
                }
                area_beauty.SetActive(true);
                area_filter_style.SetActive(false);
                currentBeautySkinType = BeautySkinType.BeautySkin;
            }
        });
        BeautySelectors[1].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                foreach (var go in beautyskins)
                {
                    go.SetActive(false);
                }
                foreach (var go in beautyshapes)
                {
                    go.SetActive(true);
                }
                area_beauty.SetActive(true);
                area_filter_style.SetActive(false);
                currentBeautySkinType = BeautySkinType.BeautyShape;
            }
        });
        BeautySelectors[2].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                foreach (var go in filters)
                {
                    go.SetActive(true);
                }
                foreach (var go in styles)
                {
                    go.SetActive(false);
                }
                area_beauty.SetActive(false);
                area_filter_style.SetActive(true);
                currentBeautySkinType = BeautySkinType.Filter;
            }
        });
        BeautySelectors[3].onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                foreach (var go in filters)
                {
                    go.SetActive(false);
                }
                foreach (var go in styles)
                {
                    go.SetActive(true);
                }
                area_beauty.SetActive(false);
                area_filter_style.SetActive(true);
                currentBeautySkinType = BeautySkinType.Style;
            }
        });

        beautyskins[0].transform.Find("Button_On").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[0].paramword, 1.0f);
                SwitchImg(beautyskins[0], true);
            }
        });
        beautyskins[0].transform.Find("Button_Off").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[0].paramword, 0.0f);
                SwitchImg(beautyskins[0], false);
            }
        });
        beautyskins[1].transform.Find("Button_On").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[1].paramword, 0.0f);
                SwitchImg(beautyskins[1], true);
            }
        });
        beautyskins[1].transform.Find("Button_Off").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[1].paramword, 1.0f);
                SwitchImg(beautyskins[1], false);
            }
        });
        beautyskins[2].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[2].paramword, v/100.0f * 6);
            SwitchImg(beautyskins[2], v != 0);
        });
        beautyskins[3].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[3].paramword, v / 100.0f);
            SwitchImg(beautyskins[3], v != 0);
        });
        beautyskins[4].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[4].paramword, v / 100.0f);
            SwitchImg(beautyskins[4], v != 0);
        });
        beautyskins[5].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[5].paramword, v / 100.0f);
            SwitchImg(beautyskins[5], v != 0);
        });
        beautyskins[6].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_1[6].paramword, v / 100.0f);
            SwitchImg(beautyskins[6], v != 0);
        });


        beautyshapes[0].transform.Find("4").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            currentBeautyShape = 4;
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[0].paramword, 4);
            for(int i =1;i< beautyshapes.Length;i++)
            {
                beautyshapes[i].SetActive(true);
            }
            beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_0[0];
            beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_0[1];
        });
        beautyshapes[0].transform.Find("3").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            currentBeautyShape = 3;
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[0].paramword, 3);
            for (int i = 1; i < beautyshapes.Length; i++)
            {
                if(i<3)
                    beautyshapes[i].SetActive(true);
                else
                    beautyshapes[i].SetActive(false);
            }
            beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[0];
            beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[1];
        });
        beautyshapes[0].transform.Find("0").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            currentBeautyShape = 0;
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[0].paramword, 0);
            for (int i = 1; i < beautyshapes.Length; i++)
            {
                if (i < 3)
                    beautyshapes[i].SetActive(true);
                else
                    beautyshapes[i].SetActive(false);
            }
            beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[0];
            beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[1];
        });
        beautyshapes[0].transform.Find("1").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            currentBeautyShape = 1;
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[0].paramword, 1);
            for (int i = 1; i < beautyshapes.Length; i++)
            {
                if (i < 3)
                    beautyshapes[i].SetActive(true);
                else
                    beautyshapes[i].SetActive(false);
            }
            beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[0];
            beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[1];
        });
        beautyshapes[0].transform.Find("2").GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            currentBeautyShape = 2;
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[0].paramword, 2);
            for (int i = 1; i < beautyshapes.Length; i++)
            {
                if (i < 3)
                    beautyshapes[i].SetActive(true);
                else
                    beautyshapes[i].SetActive(false);
            }
            beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[0];
            beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyShapeValue_1[1];
        });
        beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[1].paramword, v / 100.0f);
            if (currentBeautyShape == 4)
                BeautyShapeValue_0[0] = v;
            else
                BeautyShapeValue_1[0] = v;
            SwitchImg(beautyshapes[1], v != 0);
        });
        beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[2].paramword, v / 100.0f);
            if (currentBeautyShape == 4)
                BeautyShapeValue_0[1] = v;
            else
                BeautyShapeValue_1[1] = v;
            SwitchImg(beautyshapes[2], v != 0);
        });
        beautyshapes[3].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[3].paramword, (v + 50) / 100.0f);
            SwitchImg(beautyshapes[3], v != 0);
        });
        beautyshapes[4].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[4].paramword, (v + 50) / 100.0f);
            SwitchImg(beautyshapes[4], v != 0);
        });
        beautyshapes[5].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[5].paramword, v / 100.0f);
            SwitchImg(beautyshapes[5], v != 0);
        });
        beautyshapes[6].transform.Find("Slider").GetComponent<Slider>().onValueChanged.AddListener(delegate (float v)
        {
            rtt.SetItemParamd((int)SlotForItems.Beauty, BeautyConfig.beautySkin_2[6].paramword, (v+50) / 100.0f);
            SwitchImg(beautyshapes[6], v != 0);
        });

        Button_reset.GetComponent<AddClickEvent>().AddListener(delegate
        {
            SetDefaultUI(currentBeautySkinType);
        });

        //首次打开美肤
        SetDefaultUI(BeautySkinType.BeautySkin);
        SetDefaultUI(BeautySkinType.BeautyShape);
        foreach (var go in beautyskins)
        {
            go.SetActive(true);
        }
        foreach (var go in beautyshapes)
        {
            go.SetActive(false);
        }
        area_beauty.SetActive(true);
        area_filter_style.SetActive(false);
        currentBeautySkinType = BeautySkinType.BeautySkin;
        currentBeautyShape = 4;

        filterslider.value = 100;
        filterslider.onValueChanged.AddListener(delegate (float v)
        {
            if (currentBeautySkinType == BeautySkinType.Filter && currentBeautyFilter >= 0 && currentBeautyFilter < BeautyConfig.beautySkin_3.Length)
                BeautyConfig.beautySkin_3[currentBeautyFilter].currentvalue= v / 100.0f;
            else if(currentBeautySkinType == BeautySkinType.Style && currentBeautyFilter >= 0 && currentBeautyFilter < BeautyConfig.beautySkin_4.Length)
                BeautyConfig.beautySkin_4[currentBeautyFilter].currentvalue = v / 100.0f;
            rtt.SetItemParamd((int)SlotForItems.Beauty, "filter_level", v / 100.0f);
        });

        filters[0].GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                rtt.SetItemParams((int)SlotForItems.Beauty, "filter_name", BeautyConfig.beautySkin_3[0].paramword);
                value_filter.SetActive(false);
                foreach (var go in styles)
                {
                    go.GetComponent<Toggle>().isOn = false;
                }
            }
        });
        for (int j=1;j< filters.Length; j++)
        {
            SetFilters(j);
        }
        styles[0].GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                rtt.SetItemParams((int)SlotForItems.Beauty, "filter_name", BeautyConfig.beautySkin_4[0].paramword);
                value_filter.SetActive(false);
                foreach (var go in filters)
                {
                    go.GetComponent<Toggle>().isOn = false;
                }
            }
        });
        for (int j = 1; j < styles.Length; j++)
        {
            SetStyles(j);
        }

        OpenStats.GetComponent<AddClickEvent>().AddListener(delegate
        {
            Stats.SetActive(true);
            needupdateinfo = true;
            OpenStats.SetActive(false);
        });
        CloseStats.GetComponent<AddClickEvent>().AddListener(delegate
        {
            Stats.SetActive(false);
            needupdateinfo = false;
            OpenStats.SetActive(true);
        });
    }

    void SwitchImg(GameObject go,bool ison)
    {
        go.transform.Find("Image_on").gameObject.SetActive(ison);
        go.transform.Find("Image_off").gameObject.SetActive(!ison);
    }

    void SetFilters(int i)
    {
        filters[i].GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                currentBeautyFilter = i;
                rtt.SetItemParams((int)SlotForItems.Beauty, "filter_name", BeautyConfig.beautySkin_3[i].paramword);
                filterslider.value = BeautyConfig.beautySkin_3[i].currentvalue * 100;
                value_filter.SetActive(true);
                foreach (var go in styles)
                {
                    go.GetComponent<Toggle>().isOn = false;
                }
            }
        });
    }
    void SetStyles(int i)
    {
        styles[i].GetComponent<Toggle>().onValueChanged.AddListener(delegate (bool v)
        {
            if (v)
            {
                currentBeautyFilter = i;
                rtt.SetItemParams((int)SlotForItems.Beauty, "filter_name", BeautyConfig.beautySkin_4[i].paramword);
                filterslider.value = BeautyConfig.beautySkin_4[i].currentvalue * 100;
                value_filter.SetActive(true);
                foreach (var go in filters)
                {
                    go.GetComponent<Toggle>().isOn = false;
                }
            }
        });
    }

    void SetDefaultUI(BeautySkinType bst)
    {
        if (bst == BeautySkinType.BeautySkin)
        {
            beautyskins[0].transform.Find("Button_On").GetComponent<Toggle>().isOn = true;
            beautyskins[0].GetComponent<ToggleGroup>().NotifyToggleOn(beautyskins[0].transform.Find("Button_On").GetComponent<Toggle>());
            beautyskins[1].transform.Find("Button_On").GetComponent<Toggle>().isOn = true;
            beautyskins[1].GetComponent<ToggleGroup>().NotifyToggleOn(beautyskins[1].transform.Find("Button_On").GetComponent<Toggle>());
            beautyskins[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_1[2].defaultvalue / 6 * 100;
            beautyskins[3].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_1[3].defaultvalue * 100;
            beautyskins[4].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_1[4].defaultvalue * 100;
            beautyskins[5].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_1[5].defaultvalue * 100;
            beautyskins[6].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_1[6].defaultvalue * 100;
        }
        else if (bst == BeautySkinType.BeautyShape)
        {
            beautyshapes[0].transform.Find("4").GetComponent<Toggle>().isOn = true;
            beautyshapes[0].GetComponent<ToggleGroup>().NotifyToggleOn(beautyshapes[0].transform.Find("4").GetComponent<Toggle>());
            BeautyShapeValue_0[0] = BeautyConfig.beautySkin_2[1].defaultvalue * 100;
            BeautyShapeValue_1[0] = BeautyConfig.beautySkin_2[1].defaultvalue * 100;
            BeautyShapeValue_0[1] = BeautyConfig.beautySkin_2[2].defaultvalue * 100;
            BeautyShapeValue_1[1] = BeautyConfig.beautySkin_2[2].defaultvalue * 100;
            beautyshapes[1].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_2[1].defaultvalue * 100;
            beautyshapes[2].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_2[2].defaultvalue * 100;
            beautyshapes[3].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_2[3].defaultvalue * 100 - 50;
            beautyshapes[4].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_2[4].defaultvalue * 100 - 50;
            beautyshapes[5].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_2[5].defaultvalue * 100;
            beautyshapes[6].transform.Find("Slider").GetComponent<Slider>().value = BeautyConfig.beautySkin_2[6].defaultvalue * 100 - 50;
        }
    }

    #region ItemsUI

    void OpenItemsUI(ItemType it)
    {
        currentItemType = it;
        itemcontent.localPosition = Vector3.zero;
        ClearItemsOptions();
        switch (it)
        {
            case ItemType.Animoji:
                AddItemOptions(ItemConfig.item_1);
                break;
            case ItemType.ItemSticker:
                AddItemOptions(ItemConfig.item_2);
                break;
            case ItemType.ARMask:
                AddItemOptions(ItemConfig.item_3);
                break;
            case ItemType.ChangeFace:
                AddItemOptions(ItemConfig.item_4);
                break;
            case ItemType.ExpressionRecognition:
                AddItemOptions(ItemConfig.item_5);
                break;
            case ItemType.MusicFilter:
                AddItemOptions(ItemConfig.item_6);
                break;
            case ItemType.BackgroundSegmentation:
                AddItemOptions(ItemConfig.item_7);
                break;
            case ItemType.GestureRecognition:
                AddItemOptions(ItemConfig.item_8);
                break;
            case ItemType.MagicMirror:
                AddItemOptions(ItemConfig.item_9);
                break;
            case ItemType.PortraitLightEffect:
                AddItemOptions(ItemConfig.item_10);
                break;
            case ItemType.PortraitDrive:
                AddItemOptions(ItemConfig.item_11);
                break;
            default:
                break;
        }
        
        ItemContent.SetActive(true);
    }

    void AddItemOptions(Item[] items)
    {
        for(int i=0;i< items.Length;i++)
        {
            var itemobj = AddItemOption(items[i]);
            if (string.Equals(currentItemName, items[i].name))
            {
                itemobj.GetComponent<Toggle>().isOn = true;
            }
        }
    }

    GameObject AddItemOption(Item item)
    {
        GameObject option = Instantiate(Item_UIExample);
        option.transform.SetParent(itemcontent, false);
        option.transform.localScale = Vector3.one;
        option.transform.localPosition = Vector3.zero;
        option.name = item.name;
        option.transform.Find("Image").GetComponent<Image>().sprite = uisprites.GetSprite(item.type, item.iconid);
        var toggle = option.GetComponent<Toggle>();
        toggle.isOn = false;
        toggle.group = Item_ToggleGroup;
        Item_ToggleGroup.RegisterToggle(toggle);

        toggle.onValueChanged.AddListener(delegate(bool v)
        {
            if (musiccor != null)
            {
                StopCoroutine(musiccor);
                musiccor = null;
                audios.Stop();
            }
            if (v)
            {
                StartCoroutine(rtt.LoadItem(item,(int)SlotForItems.Item, new RenderToTexture.LoadItemCallback(OnItemLoaded)));
                if (item.mark.Length > 0)
                {
                    SetMarkText(item.mark, 3.0f);
                }
                else
                {
                    marktextTime = 0;
                }
            }
            else if(!Item_ToggleGroup.AnyTogglesOn())
            {
                rtt.UnLoadItem((int)SlotForItems.Item);
                currentItemName = "";
                marktextTime = 0;
            }
        });
        return option;
    }

    IEnumerator PlayMusic(string name)
    {
        bool isMusicFilter = false;
        foreach(Item item in ItemConfig.item_6)
        {
            if (string.Equals(name, item.name))
            {
                isMusicFilter = true;
                break;
            }
        }
        if (isMusicFilter)
        {
            var audiodata = Resources.LoadAsync<AudioClip>("items/MusicFilter/douyin");
            yield return audiodata;
            audios.clip = audiodata.asset as AudioClip;
            audios.loop = true;
            audios.Play();
            while (true)
            {
                rtt.SetItemParamd(name, "music_time", audios.time * 1000);
                //Debug.Log(audios.time);
                yield return Util._endOfFrame;
            }
        }
        musiccor = null;
        audios.Stop();
    }

    void ClearItemsOptions()
    {
        foreach (Transform childTr in itemcontent)
        {
            var toggle = childTr.GetComponent<Toggle>();
            toggle.onValueChanged.RemoveAllListeners();
            Item_ToggleGroup.UnregisterToggle(toggle);
            Destroy(childTr.gameObject);
        }
    }

    void OnItemLoaded(string name)
    {
        currentItemName = name;
        musiccor = StartCoroutine(PlayMusic(name));
    }

    void CloseItemUI()
    {
        ItemContent.SetActive(false);
    }
    #endregion
}
