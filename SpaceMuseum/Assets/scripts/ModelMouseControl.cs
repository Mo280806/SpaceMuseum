using UnityEngine;

public class ModelMouseControl : MonoBehaviour
{
    public float rotateSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minDistance = 2f;
    public float maxDistance = 20f;
    
    // 功能开关，默认设为true
    [Tooltip("是否启用鼠标控制功能")]
    public bool isControlEnabled = true;

    // 新增：默认目标模型（可选，可在Inspector指定）
    [Tooltip("默认控制的目标模型，可不指定")]
    public Transform defaultTarget;

    private Camera mainCamera;
    private Transform targetModel;
    private float currentDistance;
    private Vector3 cameraOffset;

    void Start()
    {
        mainCamera = Camera.main;
        
        // 默认启用脚本
        enabled = true;
        
        // 如果指定了默认目标，自动设置
        if (defaultTarget != null)
        {
            SetupForModel(defaultTarget);
        }
        else
        {
            // 没有默认目标时，仍保持脚本启用状态
            targetModel = null;
        }
    }

    public void SetupForModel(Transform model)
    {
        targetModel = model;
        if (targetModel != null)
        {
            currentDistance = Vector3.Distance(mainCamera.transform.position, targetModel.position);
            cameraOffset = mainCamera.transform.position - targetModel.position;
            // 确保启用控制
            isControlEnabled = true;
        }
    }

    void Update()
    {
        // 功能开关和目标模型检查
        if (targetModel == null || !isControlEnabled) 
            return;

        // 鼠标旋转模型
        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * rotateSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotateSpeed;
            mainCamera.transform.RotateAround(targetModel.position, Vector3.up, mouseX);
            mainCamera.transform.RotateAround(targetModel.position, mainCamera.transform.right, -mouseY);
            cameraOffset = mainCamera.transform.position - targetModel.position;
        }

        // 滚轮缩放模型
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
            mainCamera.transform.position = targetModel.position + cameraOffset.normalized * currentDistance;
        }
    }

    // 保持原有的完全禁用方法（停止脚本并清除目标）
    public void DisableControl()
    {
        enabled = false;
        targetModel = null;
    }

    // 临时禁用功能（仅关闭控制逻辑，不停止脚本）
    public void ToggleControl(bool enable)
    {
        isControlEnabled = enable;
    }

    public void DisableTemporarily()
    {
        isControlEnabled = false;
    }

    public void EnableTemporarily()
    {
        isControlEnabled = true;
    }
}
