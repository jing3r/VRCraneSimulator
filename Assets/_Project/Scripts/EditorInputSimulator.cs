using UnityEngine;

/// <summary>
/// Симулятор для тестирования интерактивных кнопок в редакторе Unity.
/// Оставлен для удобства отладки, не используется в сборке.
/// </summary>
public class EditorInputSimulator : MonoBehaviour
{
    [Tooltip("Кнопка мыши для имитации нажатия на курок VR-контроллера.")]
    [SerializeField] private KeyCode triggerKey = KeyCode.Mouse0;

    private Camera mainCamera;
    private RemoteButton lastHoveredCraneButton;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("EditorInputSimulator не может найти Main Camera на сцене!", this);
            enabled = false;
        }
    }

    private void Update()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Глобально обрабатываем отпускание кнопки мыши.
        // Это гарантирует, что кнопка отожмётся, даже если курсор уведён с нее.
        if (Input.GetKeyUp(triggerKey))
        {
            // Отпускаем кнопку крана, если она была зажата
            if (lastHoveredCraneButton != null)
            {
                lastHoveredCraneButton.ReleaseButton();
            }

            // Отпускаем триггер на газоанализаторе
            var analyzer = FindObjectOfType<GasAnalyzerController>();
            if (analyzer != null)
            {
                analyzer.SimulateTriggerRelease();
            }
        }

        // Если луч ни во что не попал, сбрасываем состояние наведения и выходим.
        if (!Physics.Raycast(ray, out hit))
        {
            if (lastHoveredCraneButton != null)
            {
                lastHoveredCraneButton.OnHoverExit();
                lastHoveredCraneButton = null;
            }
            return;
        }
        
        // Проверка попадания в кнопку крана
        var craneButton = hit.collider.GetComponent<RemoteButton>();
        if (craneButton != null)
        {
            if (craneButton != lastHoveredCraneButton)
            {
                if (lastHoveredCraneButton != null) { lastHoveredCraneButton.OnHoverExit(); }
                craneButton.OnHoverEnter();
                lastHoveredCraneButton = craneButton;
            }

            if (Input.GetMouseButtonDown(0)) { craneButton.PressButton(); }
        }
        else if (lastHoveredCraneButton != null) // Если курсор уведён с кнопки крана
        {
            lastHoveredCraneButton.OnHoverExit();
            lastHoveredCraneButton = null;
        }
        
        // Проверка попадания в кнопку питания газоанализатора
        var powerButton = hit.collider.GetComponent<PowerButton>();
        if (powerButton != null)
        {
            var analyzerController = powerButton.GetComponentInParent<GasAnalyzerController>();
            if (analyzerController != null)
            {
                // Для кнопки питания используется удержание
                if (Input.GetKey(triggerKey))
                {
                    analyzerController.SimulateTriggerHold(Time.deltaTime);
                }
            }
        }
    }
}