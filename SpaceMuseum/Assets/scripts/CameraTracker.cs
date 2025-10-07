using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CameraTracker : MonoBehaviour
{
    public static CameraTracker Instance { get; private set; }

    [Header("主视角参数")]
    public Vector3 mainViewPos;
    public Quaternion mainViewRot;
    public bool isMainView = true;

    [Header("聚焦视角参数")]
    [Tooltip("卫星在聚焦时的屏幕左侧偏移量")]
    public Vector3 satelliteOffset = new Vector3(-2f, 0, 0); // 左移2单位
    [Tooltip("聚焦时相机到卫星的距离")]
    public float focusDistance = 8f;

    [Header("UI 关联")]
    public GameObject trackUI; // 包含介绍文本和返回按钮的UI容器
    public TextMeshProUGUI introText; // 卫星介绍文本
    public Button returnToMainButton; // 返回按钮
    public TMP_StyleSheet satelliteStyleSheet; // 文本样式

    [Header("天体配置")]
    public List<SatelliteData> satelliteDataList; // 所有卫星数据
    public List<GameObject> otherComponents; // 需要隐藏的其他组件（如行星、背景等）

    // 组件映射字典
    private Dictionary<Transform, gongzhuan> satelliteOrbitDict = new Dictionary<Transform, gongzhuan>();
    private Dictionary<Transform, zizhuan> satelliteRotationDict = new Dictionary<Transform, zizhuan>();
    private List<Transform> allSatellites = new List<Transform>(); // 所有卫星的Transform

    private Transform targetSatellite; // 当前聚焦的卫星
    private Vector3 initialCamPos;
    private Quaternion initialCamRot;
    private bool isLerping = false;
    private Vector3 targetPos;
    private Quaternion targetRot;
    private float lerpTime = 0f;
    private const float LERP_DURATION = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        initialCamPos = transform.position;
        initialCamRot = transform.rotation;
        mainViewPos = initialCamPos;
        mainViewRot = initialCamRot;
    }

    void Start()
    {
        // 初始化卫星组件和列表
        InitializeSatelliteComponents();

        // 初始化返回按钮
        if (returnToMainButton != null)
        {
            returnToMainButton.onClick.RemoveAllListeners();
            returnToMainButton.onClick.AddListener(ReturnToMainView);
            returnToMainButton.gameObject.SetActive(false);
        }

        // 应用文本样式
        if (introText != null && satelliteStyleSheet != null)
        {
            introText.styleSheet = satelliteStyleSheet;
        }

        trackUI?.SetActive(false);
        // 确保初始状态下其他组件可见
        SetOtherComponentsActive(true);
    }

    // 初始化卫星的公转和自转组件
    private void InitializeSatelliteComponents()
    {
        satelliteOrbitDict.Clear();
        satelliteRotationDict.Clear();
        allSatellites.Clear();

        foreach (var data in satelliteDataList)
        {
            if (data.satellite == null) continue;

            allSatellites.Add(data.satellite);

            // 关联公转组件
            gongzhuan orbit = data.satellite.GetComponent<gongzhuan>();
            if (orbit != null)
            {
                satelliteOrbitDict[data.satellite] = orbit;
            }
            else
            {
                Debug.LogWarning($"卫星 {data.satellite.name} 缺少公转脚本(gongzhuan)");
            }

            // 关联自转组件
            zizhuan rotation = data.satellite.GetComponent<zizhuan>();
            if (rotation != null)
            {
                satelliteRotationDict[data.satellite] = rotation;
            }
            else
            {
                Debug.LogWarning($"卫星 {data.satellite.name} 缺少自转脚本(zizhuan)");
            }
        }
    }

    void Update()
    {
        if (isLerping)
        {
            LerpToTarget();
        }
    }

    // 平滑移动相机
    void LerpToTarget()
    {
        lerpTime += Time.deltaTime / LERP_DURATION;
        float t = Mathf.Clamp01(lerpTime);
        transform.position = Vector3.Lerp(transform.position, targetPos, t);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, t);

        if (t >= 1f)
        {
            isLerping = false;
            lerpTime = 0f;
        }
    }

    // 聚焦到指定卫星（核心功能）
    public void FocusOnSatellite(Transform satellite)
    {
        if (satellite == null) return;

        targetSatellite = satellite;
        isMainView = false;

        // 1. 停止所有卫星的运动
        StopAllSatelliteMovement();

        // 2. 只显示目标卫星，隐藏其他卫星
        SetOnlyTargetSatelliteActive(satellite);

        // 3. 隐藏其他组件（行星、背景等）
        SetOtherComponentsActive(false);

        // 4. 计算相机位置（让卫星在屏幕左侧）
        CalculateFocusCameraPosition(satellite);

        // 5. 显示介绍文本和返回按钮
        ShowSatelliteIntro(satellite);
        trackUI?.SetActive(true);
        returnToMainButton?.gameObject.SetActive(true);

        isLerping = true;
    }

    // 返回主视角
    public void ReturnToMainView()
    {
        targetSatellite = null;
        isMainView = true;

        // 1. 恢复所有卫星的运动
        ResumeAllSatelliteMovement();

        // 2. 显示所有卫星
        SetAllSatellitesActive(true);

        // 3. 显示其他组件
        SetOtherComponentsActive(true);

        // 4. 相机返回主视角
        targetPos = mainViewPos;
        targetRot = mainViewRot;
        isLerping = true;

        // 5. 隐藏UI
        trackUI?.SetActive(false);
        returnToMainButton?.gameObject.SetActive(false);
        if (introText != null) introText.text = "";
    }

    // 计算聚焦时的相机位置（让卫星在屏幕左侧）
    private void CalculateFocusCameraPosition(Transform satellite)
    {
        // 目标卫星的位置 + 左侧偏移
        Vector3 targetSatellitePos = satellite.position + satelliteOffset;
        
        // 相机位置 = 卫星目标位置 + 相机到卫星的距离（沿相机看向卫星的反方向）
        Vector3 direction = (targetSatellitePos - transform.position).normalized;
        targetPos = targetSatellitePos - direction * focusDistance;
        
        // 相机看向卫星
        targetRot = Quaternion.LookRotation(targetSatellitePos - targetPos);
    }

    // 只显示目标卫星，隐藏其他卫星
    private void SetOnlyTargetSatelliteActive(Transform target)
    {
        foreach (var sat in allSatellites)
        {
            sat.gameObject.SetActive(sat == target);
        }
    }

    // 设置所有卫星的激活状态
    private void SetAllSatellitesActive(bool active)
    {
        foreach (var sat in allSatellites)
        {
            if (sat != null) sat.gameObject.SetActive(active);
        }
    }

    // 设置其他组件的激活状态
    private void SetOtherComponentsActive(bool active)
    {
        foreach (var comp in otherComponents)
        {
            if (comp != null) comp.SetActive(active);
        }
    }

    // 停止所有卫星的运动
    private void StopAllSatelliteMovement()
    {
        foreach (var orbit in satelliteOrbitDict.Values)
        {
            if (orbit != null) orbit.enabled = false;
        }
        foreach (var rotation in satelliteRotationDict.Values)
        {
            if (rotation != null) rotation.enabled = false;
        }
    }

    // 恢复所有卫星的运动
    private void ResumeAllSatelliteMovement()
    {
        foreach (var orbit in satelliteOrbitDict.Values)
        {
            if (orbit != null) orbit.enabled = true;
        }
        foreach (var rotation in satelliteRotationDict.Values)
        {
            if (rotation != null) rotation.enabled = true;
        }
    }

    // 显示卫星介绍文本
    private void ShowSatelliteIntro(Transform satellite)
    {
        if (introText == null) return;

        var data = satelliteDataList.Find(item => item.satellite == satellite);
        if (data != null && !string.IsNullOrEmpty(data.introText))
        {
            introText.text = data.introText;
        }
        else
        {
            introText.text = "暂无该卫星介绍";
        }
    }
}

[System.Serializable]
public class SatelliteData
{
    public Transform satellite; // 卫星对象
    [TextArea] public string introText; // 卫星介绍文本
}
