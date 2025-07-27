using System;
using UnityEngine;

public class AspectRatioController : MonoBehaviour
{
    [Header("Camera")] 
    public Camera mainCamera;

    [Header("Setting")] 
    public float targetAspect = 16.0f / 9.0f;
    public bool adjustFOV = true;
    public float baseFOV = 60f;

    private int lastScreenWidth;
    private int lastScreenHeight;
    private float lastAspect;

    private void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // 시작 시 화면 비율 조정
        AdjustForAspectRatio();
        
        // 현재 화면 크기 저장
        UpdateScreenSize();
    }

    private void Update()
    {
        // 화면 크기가 바뀌었는지 확인한다.
        if (HasScreenSizeChanged())
        {
            // 화면 크기가 바뀌면 다시 조정
            AdjustForAspectRatio();
            UpdateScreenSize();
        }
    }

    private bool HasScreenSizeChanged()
    {
        return Screen.width != lastScreenWidth || Screen.height != lastScreenHeight;
    }

    // 현재 화면 크기를 저장한다.
    private void UpdateScreenSize()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
        lastAspect = (float)Screen.width / (float)Screen.height;
    }

    // 화면 비율에 맞춰 카메라를 조정한다.
    private void AdjustForAspectRatio()
    {
        if (mainCamera == null)
        {
            return;
        }
        
        // 현재 화면의 가로 세로 비율을 계산한다.
        float currentAspect = (float)Screen.width / (float)Screen.height;

        if (adjustFOV)
        {
            if (currentAspect > targetAspect)
            {
                // 화면이 너무 넓다. 시야각을 줄인다.
                float factor = targetAspect / currentAspect;
                mainCamera.fieldOfView = baseFOV * factor;
            }
            else
            {
                // 화면이 너무 좁다. 시야각을 늘린다.
                float factor = currentAspect / targetAspect;
                mainCamera.fieldOfView = baseFOV / factor;
            }
        }
        
        Debug.Log($"화면 비율 조정: {Screen.width} x {Screen.height} (비율: {currentAspect:F2}");
    }

    public void ForceAdjustAspectRatio()
    {
        AdjustForAspectRatio();
    }

    public void SetTargetAspectRatio(float newAspect)
    {
        targetAspect = newAspect;
        AdjustForAspectRatio();
    }

    public void SetBaseFOV(float newFOV)
    {
        baseFOV = newFOV;
        AdjustForAspectRatio();
    }
}
