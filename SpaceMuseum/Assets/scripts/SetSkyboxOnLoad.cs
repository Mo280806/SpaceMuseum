using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering; // 新增的命名空间引用，用于AmbientMode

public class SetSkyboxOnLoad : MonoBehaviour
{
    [Header("场景配置")]
    [Tooltip("指定要应用天空盒的场景名称，留空则应用于当前场景")]
    public string targetSceneName;

    [Header("天空盒材质")]
    [Tooltip("当前场景要使用的专属天空盒材质")]
    public Material targetSkybox;
    
    [Tooltip("离开场景时恢复的默认天空盒，留空则自动使用初始天空盒")]
    public Material defaultSkybox;

    private Material _initialSkybox; // 存储初始天空盒的缓存

    void Awake()
    {
        // 记录初始天空盒状态
        _initialSkybox = RenderSettings.skybox;
        
        // 若未设置默认天空盒，自动使用初始天空盒
        if (defaultSkybox == null)
        {
            defaultSkybox = _initialSkybox;
        }
    }

    void Start()
    {
        // 仅在目标场景且已设置天空盒材质时生效
        if (IsCurrentSceneTarget() && targetSkybox != null)
        {
            SwitchToTargetSkybox();
        }
    }

    // 切换到目标天空盒并刷新环境
    private void SwitchToTargetSkybox()
    {
        RenderSettings.skybox = targetSkybox;
        DynamicGI.UpdateEnvironment(); // 强制更新环境光，确保立即生效
        RenderSettings.ambientMode = AmbientMode.Skybox;
    }

    // 检查当前场景是否为目标场景
    private bool IsCurrentSceneTarget()
    {
        // 未指定场景名称时，默认应用于当前挂载的场景
        if (string.IsNullOrEmpty(targetSceneName))
        {
            return true;
        }
        
        // 匹配指定的场景名称
        return SceneManager.GetActiveScene().name == targetSceneName;
    }

    // 场景卸载时恢复默认天空盒
    void OnDestroy()
    {
        // 仅在当前使用的是目标天空盒时才恢复默认，避免干扰其他场景
        if (RenderSettings.skybox == targetSkybox && defaultSkybox != null)
        {
            RenderSettings.skybox = defaultSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    // 编辑器模式下实时预览（非运行时）
    void OnValidate()
    {
        if (!Application.isPlaying && targetSkybox != null && IsCurrentSceneTarget())
        {
            RenderSettings.skybox = targetSkybox;
        }
    }
}