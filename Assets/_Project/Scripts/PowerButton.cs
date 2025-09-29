using UnityEngine;

/// <summary>
/// Управляет визуальным состоянием (нажата/отпущена) кнопки питания.
/// Этот скрипт не содержит логики, только получает команды.
/// </summary>
public class PowerButton : MonoBehaviour
{
    [Header("Визуальные настройки")]
    [Tooltip("Transform визуальной части кнопки, которая будет двигаться. Если не указан, используется Transform этого же объекта.")]
    [SerializeField] private Transform buttonVisual;
    
    [Tooltip("Насколько кнопка утапливается внутрь по локальной оси Z.")]
    [SerializeField] private float pressedDepth = 0.005f;

    private Vector3 initialPosition;
    private Vector3 pressedPositionOffset;

    private void Awake()
    {
        // Если визуальный объект не назначен вручную, используем тот, на котором висит скрипт.
        if (buttonVisual == null)
        {
            buttonVisual = this.transform;
        }

        initialPosition = buttonVisual.localPosition;
        
        // Рассчитываем вектор смещения один раз при старте.
        // Движение происходит по локальной оси Z (вглубь).
        pressedPositionOffset = new Vector3(0, 0, -pressedDepth);
    }

    /// <summary>
    /// Устанавливает визуальное состояние кнопки.
    /// </summary>
    /// <param name="isPressed">True - утопить кнопку, False - вернуть в исходное положение.</param>
    public void SetPressedState(bool isPressed)
    {
        if (buttonVisual != null)
        {
            buttonVisual.localPosition = isPressed ? initialPosition + pressedPositionOffset : initialPosition;
        }
    }
}