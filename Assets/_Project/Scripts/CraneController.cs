using UnityEngine;

/// <summary>
/// Управляет движением, ограничениями и визуальными эффектами кран-балки.
/// </summary>
public class CraneController : MonoBehaviour
{
    [Header("Ссылки на компоненты (Transforms)")]
    [Tooltip("Логический объект для движения вдоль цеха (Север/Юг).")]
    [SerializeField] private Transform beamMover;
    [Tooltip("Логический объект для движения поперёк цеха (Запад/Восток).")]
    [SerializeField] private Transform carriageMover;
    [Tooltip("Логический объект для движения крюка (Вверх/Вниз).")]
    [SerializeField] private Transform hookMover;
    [Tooltip("Визуальный объект катушки для анимации вращения.")]
    [SerializeField] private Transform reelTransform;
    [Tooltip("AudioSource для звука работы лебёдки.")]
    [SerializeField] private AudioSource reelAudioSource;

    [Header("Настройки скорости")]
    [SerializeField] private float speedNorthSouth = 2f;
    [SerializeField] private float speedEastWest = 3f;
    [SerializeField] private float speedUpDown = 1f;
    [SerializeField] private float reelRotationSpeed = 100f;

    [Header("Ограничения движения (локальные координаты)")]
    [Tooltip("Пределы движения по оси Z (min/max).")]
    [SerializeField] private Vector2 zLimits = new Vector2(-9.5f, 8.5f);
    [Tooltip("Пределы движения по оси X (min/max).")]
    [SerializeField] private Vector2 xLimits = new Vector2(-5.1f, 3.3f);
    [Tooltip("Пределы движения по оси Y (min/max).")]
    [SerializeField] private Vector2 yLimits = new Vector2(-6.1f, -0.35f);

    private Vector3 currentMovementInput = Vector3.zero;
    private bool isReelSoundPlaying = false;

    private void Update()
    {
        ApplyMovement();
        ApplyLimits();
        UpdateReelAnimationAndSound();
    }

    /// <summary>
    /// Публичный метод для получения команд от кнопок через UnityEvent.
    /// Добавляет вектор направления к общему вектору движения.
    /// </summary>
    /// <param name="direction">Вектор движения от кнопки.</param>
    public void AddMovement(Vector3 direction)
    {
        currentMovementInput += direction;
    }

    /// <summary>
    /// Применяет рассчитанное смещение к движущимся частям крана.
    /// </summary>
    private void ApplyMovement()
    {
        if (currentMovementInput.sqrMagnitude < 0.001f) { return; }

        var movementThisFrame = new Vector3(
            currentMovementInput.x * speedEastWest * Time.deltaTime,
            currentMovementInput.y * speedUpDown * Time.deltaTime,
            currentMovementInput.z * speedNorthSouth * Time.deltaTime
        );

        beamMover.Translate(0, 0, movementThisFrame.z, Space.Self);
        carriageMover.Translate(movementThisFrame.x, 0, 0, Space.Self);
        hookMover.Translate(0, movementThisFrame.y, 0, Space.Self);
    }

    /// <summary>
    /// Проверяет и ограничивает позицию каждой части крана в ее локальных пределах.
    /// </summary>
    private void ApplyLimits()
    {
        beamMover.localPosition = new Vector3(
            beamMover.localPosition.x,
            beamMover.localPosition.y,
            Mathf.Clamp(beamMover.localPosition.z, zLimits.x, zLimits.y)
        );

        carriageMover.localPosition = new Vector3(
            Mathf.Clamp(carriageMover.localPosition.x, xLimits.x, xLimits.y),
            carriageMover.localPosition.y,
            carriageMover.localPosition.z
        );

        hookMover.localPosition = new Vector3(
            hookMover.localPosition.x,
            Mathf.Clamp(hookMover.localPosition.y, yLimits.x, yLimits.y),
            hookMover.localPosition.z
        );
    }

    /// <summary>
    /// Управляет вращением катушки и воспроизведением звука лебёдки.
    /// </summary>
    private void UpdateReelAnimationAndSound()
    {
        float verticalInput = currentMovementInput.y;
        bool shouldBeActive = Mathf.Abs(verticalInput) > 0.01f;

        // Анимация вращения
        if (shouldBeActive && reelTransform != null)
        {
            reelTransform.Rotate(Vector3.right, -verticalInput * reelRotationSpeed * Time.deltaTime, Space.Self);
        }

        // Логика звука
        if (reelAudioSource == null) { return; }
        
        if (shouldBeActive && !isReelSoundPlaying)
        {
            reelAudioSource.Play();
            isReelSoundPlaying = true;
        }
        else if (!shouldBeActive && isReelSoundPlaying)
        {
            reelAudioSource.Stop();
            isReelSoundPlaying = false;
        }
    }
}