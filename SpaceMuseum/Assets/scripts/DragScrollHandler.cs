using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events; // 添加这一行引用UnityAction所在的命名空间

[RequireComponent(typeof(EventTrigger))]
public class DragScrollHandler : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    private ScrollRect scrollRect;
    private bool isScrolling = false;

    void Awake()
    {
        if (!GetComponent<EventTrigger>())
        {
            gameObject.AddComponent<EventTrigger>();
        }
    }

    void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>();
        
        if (scrollRect != null)
        {
            Debug.Log($"找到ScrollRect组件: {scrollRect.gameObject.name}");
            BindEvents();
        }
        else
        {
            Debug.LogError("未找到父级的ScrollRect组件！请检查UI层级结构");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 这里可以添加拖动时的逻辑，比如之前处理滚动的代码
        if (scrollRect != null)
        {
            scrollRect.OnDrag(eventData);
            // 也可以添加其他拖动相关的调试或业务逻辑
            Debug.Log("正在拖动");
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 这里可以添加拖动结束时的逻辑，比如相关的状态重置等
        if (scrollRect != null)
        {
            scrollRect.OnEndDrag(eventData);
            Debug.Log("拖动结束");
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 这里可以添加拖动开始时的逻辑，比如相关的状态初始化等
        if (scrollRect != null)
        {
            scrollRect.OnBeginDrag(eventData);
            Debug.Log("开始拖动");
        }
    }

    private void BindEvents()
    {
        EventTrigger trigger = GetComponent<EventTrigger>();
        trigger.triggers.Clear();

        AddEvent(trigger, EventTriggerType.BeginDrag, OnBeginDrag);
        AddEvent(trigger, EventTriggerType.Drag, OnDrag);
        AddEvent(trigger, EventTriggerType.EndDrag, OnEndDrag);
    }

    private void AddEvent(EventTrigger trigger, EventTriggerType type, UnityAction<BaseEventData> action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }

    public void OnBeginDrag(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            isScrolling = true;
            if (scrollRect != null)
            {
                scrollRect.OnBeginDrag(pointerEventData);
                Debug.Log("开始拖动");
            }
        }
    }

    public void OnDrag(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null && isScrolling && scrollRect != null)
        {
            Vector2 dragDelta = pointerEventData.delta;
            scrollRect.OnDrag(pointerEventData);
            Debug.Log($"拖动中: {dragDelta}");
        }
    }

    public void OnEndDrag(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            isScrolling = false;
            if (scrollRect != null)
            {
                scrollRect.OnEndDrag(pointerEventData);
                Debug.Log("结束拖动");
            }
        }
    }
}