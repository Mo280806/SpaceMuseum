using UnityEngine;
using UnityEngine.UI;

public class ButtonActivatedRotation : MonoBehaviour
{
    [Header("控制参数")]
    public float rotateSpeed = 20f; // 进一步提高旋转速度
    public float zoomSpeed = 0.5f;
    public float minDistance = 2f;
    public float maxDistance = 20f;

    [Header("按钮引用")]
    public Button rotateToggleButton;
    public Button zoomInButton;
    public Button zoomOutButton;
    public Button returnToMainButton;

    [Header("关联组件")]
    public CameraTracker cameraTracker;
    public GameObject controlPanel;
    public Camera mainCamera;
    // 新增：强制旋转的备选方案开关
    public bool useAlternativeRotation = false;

    private Transform targetModel;
    private float currentDistance;
    private Vector3 cameraOffset;
    private bool isRotationActive = false;
    private bool isDragging = false;
    // 新增：记录鼠标初始位置
    private Vector3 lastMousePosition;

    void Start()
    {
        // 相机设置
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("未找到相机！");
                return;
            }
        }

        // 获取CameraTracker
        if (cameraTracker == null)
        {
            cameraTracker = FindObjectOfType<CameraTracker>();
            if (cameraTracker == null)
            {
                Debug.LogError("未找到CameraTracker组件！");
                return;
            }
        }

        // 绑定按钮事件
        BindButtonEvents();
        
        // 初始隐藏控制面板
        if (controlPanel != null)
            controlPanel.SetActive(false);
    }

    private void BindButtonEvents()
    {
        if (rotateToggleButton != null)
        {
            rotateToggleButton.onClick.RemoveAllListeners();
            rotateToggleButton.onClick.AddListener(ToggleRotationActive);
            Debug.Log("旋转按钮事件已绑定");
        }
        
        if (zoomInButton != null)
        {
            zoomInButton.onClick.RemoveAllListeners();
            zoomInButton.onClick.AddListener(ZoomIn);
        }
        
        if (zoomOutButton != null)
        {
            zoomOutButton.onClick.RemoveAllListeners();
            zoomOutButton.onClick.AddListener(ZoomOut);
        }

        if (returnToMainButton != null)
        {
            returnToMainButton.onClick.RemoveAllListeners();
            returnToMainButton.onClick.AddListener(ReturnToMainView);
        }
    }

    void Update()
    {
        if (mainCamera == null || cameraTracker == null)
            return;

        // 处理视角状态
        if (cameraTracker.isMainView)
        {
            if (controlPanel != null)
                controlPanel.SetActive(false);
            targetModel = null;
            isRotationActive = false;
            UpdateRotationButtonState();
            return;
        }
        else
        {
            if (controlPanel != null && !controlPanel.activeSelf)
                controlPanel.SetActive(true);
        }

        // 确保目标模型已设置
        if (targetModel == null)
        {
            SetTargetModel();
            return;
        }

        // 持续更新相机偏移
        cameraOffset = mainCamera.transform.position - targetModel.position;
        
        // 旋转处理
        if (isRotationActive)
        {
            // 检测鼠标输入（两种方式确保捕获）
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
                Debug.Log("鼠标按下，开始拖动");
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                Debug.Log("鼠标释放，结束拖动");
            }

            // 处理拖动旋转
            if (isDragging)
            {
                HandleRotation();
            }
        }
    }

    private void SetTargetModel()
    {
        if (cameraTracker.CurrentTarget != null)
        {
            targetModel = cameraTracker.CurrentTarget;
            
            if (targetModel == null)
            {
                Debug.LogError("CameraTracker的CurrentTarget为空");
                return;
            }
            
            if (!targetModel.gameObject.activeInHierarchy)
            {
                Debug.LogError($"目标 {targetModel.name} 未激活");
                return;
            }
            
            currentDistance = Vector3.Distance(mainCamera.transform.position, targetModel.position);
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            cameraOffset = mainCamera.transform.position - targetModel.position;
            
            Debug.Log($"成功设置目标: {targetModel.name}, 距离: {currentDistance}");
        }
        else
        {
            Debug.LogWarning("CameraTracker中没有当前目标");
        }
    }

    private void HandleRotation()
    {
        // 方法1：使用Input轴（原方法）
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        // 方法2：使用鼠标位置差（备选方案）
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 mouseDelta = currentMousePosition - lastMousePosition;
        lastMousePosition = currentMousePosition;

        // 输出两种方法的输入值，用于对比
        Debug.Log($"鼠标轴输入: X={mouseX}, Y={mouseY} | 位置差: X={mouseDelta.x}, Y={mouseDelta.y}");

        // 执行旋转（根据开关选择方式）
        if (useAlternativeRotation)
        {
            // 备选旋转方法：直接使用位置差
            mainCamera.transform.RotateAround(targetModel.position, Vector3.up, mouseDelta.x * rotateSpeed * 0.1f);
            mainCamera.transform.RotateAround(targetModel.position, mainCamera.transform.right, -mouseDelta.y * rotateSpeed * 0.1f);
        }
        else
        {
            // 原旋转方法：使用Input轴
            mainCamera.transform.RotateAround(targetModel.position, Vector3.up, mouseX * rotateSpeed);
            mainCamera.transform.RotateAround(targetModel.position, mainCamera.transform.right, -mouseY * rotateSpeed);
        }
    }

    public void ToggleRotationActive()
    {
        isRotationActive = !isRotationActive;
        Debug.Log($"旋转功能已{(isRotationActive ? "启用" : "禁用")}");
        UpdateRotationButtonState();
    }

    private void UpdateRotationButtonState()
    {
        if (rotateToggleButton != null)
        {
            ColorBlock colors = rotateToggleButton.colors;
            colors.normalColor = isRotationActive ? Color.green : Color.white;
            rotateToggleButton.colors = colors;
        }
    }

    // 保持缩放和返回方法不变...
    public void ZoomIn()
    {
        if (targetModel == null)
        {
            Debug.LogWarning("放大失败：目标模型不存在");
            SetTargetModel();
            return;
        }
        
        currentDistance = Mathf.Max(minDistance, currentDistance - zoomSpeed);
        mainCamera.transform.position = targetModel.position + cameraOffset.normalized * currentDistance;
        Debug.Log($"放大，当前距离: {currentDistance}");
    }

    public void ZoomOut()
    {
        if (targetModel == null)
        {
            Debug.LogWarning("缩小失败：目标模型不存在");
            SetTargetModel();
            return;
        }
        
        currentDistance = Mathf.Min(maxDistance, currentDistance + zoomSpeed);
        mainCamera.transform.position = targetModel.position + cameraOffset.normalized * currentDistance;
        Debug.Log($"缩小，当前距离: {currentDistance}");
    }

    public void ReturnToMainView()
    {
        if (cameraTracker != null)
        {
            cameraTracker.ReturnToMainView();
            isRotationActive = false;
            UpdateRotationButtonState();
        }
    }
}
