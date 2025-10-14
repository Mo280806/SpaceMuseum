using UnityEngine;
using UnityEngine.UI;

public class moving : MonoBehaviour
{
    [Header("控制参数")]
    public float rotateSpeed = 5f;       // 旋转速度
    public float zoomSpeed = 5f;        // 缩放速度
    public float minDistance = 2f;      // 最小缩放距离
    public float maxDistance = 20f;     // 最大缩放距离

    [Header("关联组件")]
    public Button toggleControlButton;  // 用于启用/禁用控制的按钮
    public CameraTracker cameraTracker; // 相机追踪脚本引用

    private Camera mainCamera;          // 主相机引用
    private Transform targetModel;      // 目标模型
    private float currentDistance;      // 当前相机与模型的距离
    private Vector3 cameraOffset;       // 相机相对于模型的偏移
    private bool isControlling = false; // 控制功能是否激活

    void Start()
    {
        // 获取主相机
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("未找到主相机！");
            return;
        }

        // 自动获取CameraTracker（如果未指定）
        if (cameraTracker == null)
        {
            cameraTracker = FindObjectOfType<CameraTracker>();
        }

        // 绑定按钮事件
        if (toggleControlButton != null)
        {
            toggleControlButton.onClick.AddListener(ToggleControl);
            UpdateButtonState(); // 初始化按钮状态
        }
        else
        {
            Debug.LogWarning("未指定控制按钮，无法通过按钮控制脚本状态");
        }

        // 初始禁用控制
        enabled = false;
    }

    void Update()
    {
        // 自动更新目标模型（从CameraTracker获取）
        UpdateTargetModel();

        // 如果没有目标模型，不执行控制逻辑
        if (targetModel == null)
        {
            return;
        }

        // 鼠标左键拖动：围绕模型旋转
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
            
            // 绕Y轴水平旋转
            mainCamera.transform.RotateAround(targetModel.position, Vector3.up, mouseX);
            // 绕X轴垂直旋转
            mainCamera.transform.RotateAround(targetModel.position, mainCamera.transform.right, -mouseY);
            
            // 更新偏移量
            cameraOffset = mainCamera.transform.position - targetModel.position;
        }

    }

    // 从CameraTracker自动获取当前聚焦的目标模型
    private void UpdateTargetModel()
    {
        if (cameraTracker == null || cameraTracker.isMainView)
        {
            // 如果在主视角或没有追踪器，清除目标
            targetModel = null;
            return;
        }

        // 获取当前聚焦的目标
        Transform newTarget = cameraTracker.CurrentTarget;
        if (newTarget != null && newTarget != targetModel)
        {
            // 目标改变时更新参数
            targetModel = newTarget;
            currentDistance = Vector3.Distance(mainCamera.transform.position, targetModel.position);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            cameraOffset = mainCamera.transform.position - targetModel.position;
            
            Debug.Log($"已自动获取目标模型: {targetModel.name}");
        }
    }

    // 切换控制功能的启用/禁用状态
    public void ToggleControl()
    {
        isControlling = !isControlling;
        enabled = isControlling; // 启用/禁用Update方法
        UpdateButtonState();
        
        Debug.Log($"控制功能已{(isControlling ? "启用" : "禁用")}");
    }

    // 更新按钮显示状态
    private void UpdateButtonState()
    {
        if (toggleControlButton != null)
        {
            // 如果按钮有文本，可以更新文本内容
            Text buttonText = toggleControlButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = isControlling ? "禁用视角控制" : "启用视角控制";
            }
        }
    }

    // 外部调用：强制禁用控制
    public void DisableControl()
    {
        isControlling = false;
        enabled = false;
        UpdateButtonState();
    }

    // 外部调用：强制启用控制
    public void EnableControl()
    {
        isControlling = true;
        enabled = true;
        UpdateButtonState();
    }
}
