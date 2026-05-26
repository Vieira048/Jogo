using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect lastSafeArea;
    private Vector2 lastScreenSize;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        ApplyCurrentSafeArea();
    }

    private void Update()
    {
        ApplyCurrentSafeArea();
    }

    public void ApplyCurrentSafeArea()
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        Rect safeArea = Screen.safeArea;

        if (safeArea == lastSafeArea && screenSize == lastScreenSize)
            return;

        ApplySafeArea(safeArea, screenSize);
    }

    public void ApplySafeArea(Rect safeArea, Vector2 screenSize)
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (screenSize.x <= 0f || screenSize.y <= 0f)
            return;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x = Mathf.Clamp01(anchorMin.x / screenSize.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y / screenSize.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x / screenSize.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y / screenSize.y);

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;
    }
}

