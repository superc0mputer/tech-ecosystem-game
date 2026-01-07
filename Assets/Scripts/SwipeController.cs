using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class SwipeController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("Dependencies")]
    public GameLoopManager gameLoopManager;
    
    [Header("UI References")]
    // We get these from GameUIController now to avoid manual dragging
    private CanvasGroup optionAGroup;
    private CanvasGroup optionBGroup;

    [Header("Settings")]
    [SerializeField] private float threshold = 100f;       
    [SerializeField] private float maxDragDistance = 300f; 
    [SerializeField] private float rotationStrength = 0.05f; 
    [SerializeField] private float snapBackSpeed = 20f;    
    
    [Header("Visual Feedback")]
    [Range(0f, 1f)] [SerializeField] private float defaultAlpha = 0.3f; 
    [Range(0f, 1f)] [SerializeField] private float highlightAlpha = 1.0f; 

    private RectTransform rectTransform;
    private Vector2 startPos;
    private bool isDragging = false;
    private int lastPreviewDirection = 0; // 0=None, -1=Left, 1=Right

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;
    }

    private void Start()
    {
        // Grab references directly from the UI Controller so you don't have to drag them
        if(gameLoopManager != null && gameLoopManager.uiController != null)
        {
            optionAGroup = gameLoopManager.uiController.optionACanvasGroup;
            optionBGroup = gameLoopManager.uiController.optionBCanvasGroup;
        }
        ResetVisuals();
    }

    private void OnEnable()
    {
        ResetCardPosition();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!gameLoopManager.isGameActive) return;
        StopAllCoroutines();
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        // 1. Movement Logic
        float rawNewX = rectTransform.anchoredPosition.x + eventData.delta.x;
        float offsetX = rawNewX - startPos.x;
        float clampedOffsetX = Mathf.Clamp(offsetX, -maxDragDistance, maxDragDistance);
        
        rectTransform.anchoredPosition = new Vector2(startPos.x + clampedOffsetX, startPos.y);
        rectTransform.localRotation = Quaternion.Euler(0, 0, clampedOffsetX * -rotationStrength);

        // 2. Highlight Logic
        UpdateOptionVisuals(clampedOffsetX);
        
        // 3. Resource Preview Logic (NEW)
        CheckPreview(clampedOffsetX);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        // Clear Previews immediately when letting go
        gameLoopManager.ClearPreview();
        lastPreviewDirection = 0;

        CheckSwipe();
    }

    private void CheckPreview(float offsetX)
    {
        // 10% drag is enough to trigger preview
        float previewThreshold = threshold * 0.1f; 

        if (offsetX < -previewThreshold) // Dragging Left
        {
            if (lastPreviewDirection != -1)
            {
                gameLoopManager.ShowPreview(true); // Preview Left Option
                lastPreviewDirection = -1;
            }
        }
        else if (offsetX > previewThreshold) // Dragging Right
        {
            if (lastPreviewDirection != 1)
            {
                gameLoopManager.ShowPreview(false); // Preview Right Option
                lastPreviewDirection = 1;
            }
        }
        else // In the middle
        {
            if (lastPreviewDirection != 0)
            {
                gameLoopManager.ClearPreview();
                lastPreviewDirection = 0;
            }
        }
    }

    private void UpdateOptionVisuals(float offsetX)
    {
        if (optionAGroup == null || optionBGroup == null) return;

        float progress = Mathf.Clamp01(Mathf.Abs(offsetX) / threshold);

        if (offsetX < 0) 
        {
            optionAGroup.alpha = Mathf.Lerp(defaultAlpha, highlightAlpha, progress);
            optionBGroup.alpha = defaultAlpha;
        }
        else if (offsetX > 0) 
        {
            optionAGroup.alpha = defaultAlpha;
            optionBGroup.alpha = Mathf.Lerp(defaultAlpha, highlightAlpha, progress);
        }
    }

    private void CheckSwipe()
    {
        float difference = rectTransform.anchoredPosition.x - startPos.x;

        if (difference < -threshold)
        {
            MoveCardOffScreen(-1);
            gameLoopManager.OnPlayerChoice(true); 
            ResetVisuals(); 
        }
        else if (difference > threshold)
        {
            MoveCardOffScreen(1);
            gameLoopManager.OnPlayerChoice(false); 
            ResetVisuals(); 
        }
        else
        {
            StartCoroutine(SnapBackRoutine());
        }
    }

    private void MoveCardOffScreen(int direction)
    {
        rectTransform.anchoredPosition = new Vector2(startPos.x + (direction * 1000), startPos.y);
    }

    private IEnumerator SnapBackRoutine()
    {
        Quaternion targetRot = Quaternion.identity;
        while (Vector2.Distance(rectTransform.anchoredPosition, startPos) > 0.1f)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, startPos, Time.deltaTime * snapBackSpeed);
            rectTransform.localRotation = Quaternion.Lerp(rectTransform.localRotation, targetRot, Time.deltaTime * snapBackSpeed);
            
            if(optionAGroup) optionAGroup.alpha = Mathf.Lerp(optionAGroup.alpha, defaultAlpha, Time.deltaTime * snapBackSpeed);
            if(optionBGroup) optionBGroup.alpha = Mathf.Lerp(optionBGroup.alpha, defaultAlpha, Time.deltaTime * snapBackSpeed);

            yield return null;
        }

        rectTransform.anchoredPosition = startPos;
        rectTransform.localRotation = targetRot;
        ResetVisuals();
    }

    public void ResetCardPosition()
    {
        StopAllCoroutines();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        
        rectTransform.anchoredPosition = startPos != Vector2.zero ? startPos : Vector2.zero;
        rectTransform.localRotation = Quaternion.identity;
        ResetVisuals();
    }

    private void ResetVisuals()
    {
        if (optionAGroup) optionAGroup.alpha = defaultAlpha;
        if (optionBGroup) optionBGroup.alpha = defaultAlpha;
    }
}