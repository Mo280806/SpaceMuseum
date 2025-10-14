using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScrollbarValueHandler : MonoBehaviour
{
    [Tooltip("关联的ScrollRect组件")]
    public ScrollRect scrollRect;
    
    [Tooltip("关联的滚动条组件")]
    public Scrollbar scrollbar;

    [Tooltip("关联的文本组件")]
    public TextMeshProUGUI textComponent;

    void Start()
    {
        // 自动获取组件（如果未手动指定）
        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>();
            
        if (scrollbar == null)
            scrollbar = GetComponent<Scrollbar>();
    }

    /// <summary>
    /// 当滚动条值变化时调用
    /// </summary>
    /// <param name="value">滚动条的当前值（0-1范围）</param>
    public void OnScrollbarValueChanged(float value)
    {
        if (scrollRect != null)
        {
            // 同步ScrollRect的滚动位置与滚动条值
            scrollRect.verticalNormalizedPosition = value;
            Debug.Log($"滚动条值变化: {value}, 文本滚动位置已同步");
        }

        // 可选：根据滚动位置执行其他操作
        // 例如：当滚动到顶部时执行某些逻辑
        if (Mathf.Abs(value - 1f) < 0.01f)
        {
            Debug.Log("已滚动到顶部");
        }
        // 当滚动到底部时执行某些逻辑
        else if (Mathf.Abs(value) < 0.01f)
        {
            Debug.Log("已滚动到底部");
        }
    }
}
