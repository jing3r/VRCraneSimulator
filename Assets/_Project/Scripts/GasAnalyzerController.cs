using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HTC.UnityPlugin.Vive;

/// <summary>
/// Управляет всей логикой газоанализатора: включением/выключением,
/// отображением данных на дисплее и взаимодействием в VR.
/// </summary>
public class GasAnalyzerController : MonoBehaviour
{
    #region Inspector Fields

    [Header("Ссылки на компоненты")]
    [Tooltip("Визуальный объект индикатора питания.")]
    [SerializeField] private Renderer indicatorRenderer;
    [Tooltip("Корневой объект дисплея для его включения/выключения.")]
    [SerializeField] private GameObject displayScreenObject;
    [Tooltip("Текстовый компонент для отображения дистанции.")]
    [SerializeField] private TextMeshProUGUI distanceText;
    [Tooltip("UI Image для кругового индикатора концентрации.")]
    [SerializeField] private Image concentrationMeter;
    [Tooltip("Transform зонда, от которого измеряется дистанция.")]
    [SerializeField] private Transform probeTransform;
    [Tooltip("Компонент на физической кнопке питания для управления ее визуалом.")]
    [SerializeField] private PowerButton powerButton;

    [Header("Настройки")]
    [Tooltip("Время в секундах, которое нужно удерживать курок для вкл/выкл.")]
    [SerializeField] private float holdToToggleDuration = 3f;
    [Tooltip("Тег объектов, до которых измеряется дистанция.")]
    [SerializeField] private string dangerZoneTag = "DangerZone";
    [Tooltip("Цвет включенного индикатора и максимального прогресса.")]
    [SerializeField] private Color progressColor = Color.green;

    #endregion

    #region Private State Variables

    private bool isDeviceOn = false;
    private float pressTimer = 0f;
    private bool actionTriggeredThisPress = false;
    private bool isHeld = false;
    private HandRole heldByHand = HandRole.Invalid;
    private Color initialIndicatorColor;
    private Transform[] allDangerZones;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        FindAllDangerZones();

        if (indicatorRenderer != null)
        {
            initialIndicatorColor = indicatorRenderer.material.color;
        }

        UpdateDeviceVisuals(); // Устанавливаем начальное состояние (выключено)
    }

    private void Update()
    {
        if (isHeld && heldByHand != HandRole.Invalid)
        {
            HandleTriggerWithViveInput();
        }
        
        UpdateDisplay();
    }

    #endregion

    #region Grab Handling (Draggable Events)

    public void OnGrabbed(Draggable draggable)
    {
        isHeld = true;
        var pointerEvent = draggable.draggedEvent;
        if (pointerEvent != null)
        {
            var viveEventData = pointerEvent as VivePointerEventData;
            if (viveEventData != null)
            {
                if (viveEventData.viveRole.IsRole(HandRole.RightHand)) { heldByHand = HandRole.RightHand; }
                else if (viveEventData.viveRole.IsRole(HandRole.LeftHand)) { heldByHand = HandRole.LeftHand; }
                Debug.Log("VR: Газоанализатор взят в руку: " + heldByHand);
                return;
            }
        }
        heldByHand = HandRole.Invalid;
        Debug.LogWarning("VR: Газоанализатор взят, но не удалось определить руку.");
    }

    public void OnGrabbed(Draggable draggable, HandRole hand) // Overload for Editor Simulator
    {
        isHeld = true;
        heldByHand = hand;
        Debug.Log("СИМУЛЯЦИЯ: Газоанализатор взят в руку: " + heldByHand);
    }

    public void OnReleased(Draggable draggable)
    {
        isHeld = false;
        heldByHand = HandRole.Invalid;
        SimulateTriggerRelease(); // Сбрасываем таймеры и визуал
        Debug.Log("Газоанализатор отпущен");
    }

    #endregion

    #region Input Simulation

    public void SimulateTriggerHold(float deltaTime)
    {
        if (powerButton != null) { powerButton.SetPressedState(true); }

        if (actionTriggeredThisPress) { return; }

        pressTimer += deltaTime;

        if (indicatorRenderer != null)
        {
            float progress = Mathf.Clamp01(pressTimer / holdToToggleDuration);
            Color startColor = isDeviceOn ? progressColor : initialIndicatorColor;
            Color endColor = isDeviceOn ? initialIndicatorColor : progressColor;
            indicatorRenderer.material.color = Color.Lerp(startColor, endColor, progress);
        }

        if (pressTimer >= holdToToggleDuration)
        {
            TogglePower();
            actionTriggeredThisPress = true;
        }
    }

    public void SimulateTriggerRelease()
    {
        if (powerButton != null) { powerButton.SetPressedState(false); }
        
        if (!actionTriggeredThisPress && indicatorRenderer != null)
        {
            indicatorRenderer.material.color = isDeviceOn ? progressColor : initialIndicatorColor;
        }

        pressTimer = 0f;
        actionTriggeredThisPress = false;
    }

    #endregion

    #region Core Logic

    private void HandleTriggerWithViveInput()
    {
        if (ViveInput.GetPress(heldByHand, ControllerButton.Trigger))
        {
            SimulateTriggerHold(Time.deltaTime);
        }
        else
        {
            SimulateTriggerRelease();
        }
    }

    private void TogglePower()
    {
        isDeviceOn = !isDeviceOn;
        UpdateDeviceVisuals();
    }

    private void UpdateDeviceVisuals()
    {
        if (displayScreenObject != null)
        {
            displayScreenObject.SetActive(isDeviceOn);
        }

        if (indicatorRenderer != null)
        {
            indicatorRenderer.material.color = isDeviceOn ? progressColor : initialIndicatorColor;
        }
    }

    private void UpdateDisplay()
    {
        if (!isDeviceOn) { return; } // Если выключен, ничего не обновляем

        if (allDangerZones == null || allDangerZones.Length == 0)
        {
            if (distanceText != null) { distanceText.text = "NO ZONES"; }
            if (concentrationMeter != null) { concentrationMeter.fillAmount = 0; }
            return;
        }
        
        float minDistance = FindClosestDangerZone();
        
        if (distanceText != null)
        {
            distanceText.text = minDistance.ToString("F1");
        }
        
        if (concentrationMeter != null)
        {
            float concentrationPercent = 0f;
            if (minDistance < 1f) { concentrationPercent = 1.0f; }
            else if (minDistance < 10f) { concentrationPercent = (10 - (int)minDistance) / 10.0f; }
            
            concentrationMeter.fillAmount = concentrationPercent;
        }
    }

    private void FindAllDangerZones()
    {
        GameObject[] dangerZoneObjects = GameObject.FindGameObjectsWithTag(dangerZoneTag);
        if (dangerZoneObjects.Length > 0)
        {
            allDangerZones = new Transform[dangerZoneObjects.Length];
            for (int i = 0; i < dangerZoneObjects.Length; i++)
            {
                allDangerZones[i] = dangerZoneObjects[i].transform;
            }
        }
        else
        {
            Debug.LogError($"Опасные зоны с тегом '{dangerZoneTag}' не найдены!");
        }
    }

    private float FindClosestDangerZone()
    {
        float minDistance = float.MaxValue;
        foreach (Transform zone in allDangerZones)
        {
            float currentDistance = Vector3.Distance(probeTransform.position, zone.position);
            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
            }
        }
        return minDistance;
    }

    #endregion
}