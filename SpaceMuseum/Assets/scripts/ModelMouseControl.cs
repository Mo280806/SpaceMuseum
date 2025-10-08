using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
public class ModelMouseControl : MonoBehaviour
{
    public float rotateSpeed = 5f;
    public float zoomSpeed = 5f;
    public float minDistance = 2f;
    public float maxDistance = 20f;
    private Camera mainCamera;
    private Transform targetModel;
    private float currentDistance;
    private Vector3 cameraOffset;
    void Start()
    {
        mainCamera = Camera.main;
        // 初始时禁用，等待聚焦卫星后启用
        enabled = false;
    }
    public void SetupForModel(Transform model)
    {
        targetModel = model;
        if (targetModel != null)
        {
            currentDistance = Vector3.Distance(mainCamera.transform.position, targetModel.position);
            cameraOffset = mainCamera.transform.position - targetModel.position;
            enabled = true;
        }
    }
    void Update()
    {
        if (targetModel == null) return;
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
    public void DisableControl()
    {
        enabled = false;
        targetModel = null;
    }
}