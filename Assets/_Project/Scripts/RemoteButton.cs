using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Управляет поведением одной интерактивной кнопки на пульте управления краном.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class RemoteButton : MonoBehaviour
{
    [Header("Настройки Сигналов")]
    [Tooltip("Направление движения, которое будет отправлено при нажатии.")]
    [SerializeField] private Vector3 movementDirection = Vector3.zero;

    [System.Serializable]
    public class Vector3Event : UnityEvent<Vector3> { }

    [Tooltip("Событие, вызываемое при нажатии кнопки.")]
    public Vector3Event OnButtonPressed;
    [Tooltip("Событие, вызываемое при отпускании кнопки.")]
    public Vector3Event OnButtonReleased;

    [Header("Настройки Визуализации")]
    [Tooltip("Визуальный объект кнопки, который будет двигаться.")]
    [SerializeField] private Transform buttonVisual;
    [SerializeField] private MeshRenderer buttonRenderer;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color pressedColor = Color.green;
    [SerializeField] private float pressedDepth = 0.002f;

    [Header("Настройки Звука")]
    [Tooltip("Звук, проигрываемый при нажатии.")]
    [SerializeField] private AudioClip pressSound;

    private AudioSource audioSource;
    private Vector3 initialPosition;
    private Vector3 pressedPositionOffset;
    private bool isHovering = false;
    private bool isPressed = false;

    private void Awake()
    {
        InitializeVisuals();
        InitializeAudio();
    }

    private void InitializeVisuals()
    {
        if (buttonVisual == null) { buttonVisual = this.transform; }
        if (buttonRenderer != null) { buttonRenderer.material.color = normalColor; }

        initialPosition = buttonVisual.localPosition;
        pressedPositionOffset = new Vector3(0, 0, -pressedDepth);
    }

    private void InitializeAudio()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0; // 2D-звук, чтобы громкость не зависела от расстояния
    }

    /// <summary>
    /// Вызывается, когда VR-указатель входит в коллайдер кнопки.
    /// </summary>
    public void OnHoverEnter()
    {
        isHovering = true;
        UpdateVisualState();
    }

    /// <summary>
    /// Вызывается, когда VR-указатель покидает коллайдер кнопки.
    /// </summary>
    public void OnHoverExit()
    {
        isHovering = false;

        // Если указатель ушел с зажатой кнопки, принудительно отпускаем ее.
        if (isPressed)
        {
            ReleaseButton();
        }
        else
        {
            UpdateVisualState();
        }
    }

    /// <summary>
    /// Вызывается при нажатии на кнопку.
    /// </summary>
    public void PressButton()
    {
        if (isPressed) return; // Защита от двойного нажатия

        isPressed = true;
        OnButtonPressed.Invoke(movementDirection);
        PlayPressSound();
        UpdateVisualState();
    }

    /// <summary>
    /// Вызывается при отпускании кнопки.
    /// </summary>
    public void ReleaseButton()
    {
        if (!isPressed) return; // Защита от двойного отпускания

        isPressed = false;
        OnButtonReleased.Invoke(-movementDirection);
        UpdateVisualState();
    }

    /// <summary>
    /// Обновляет цвет и положение кнопки в зависимости от ее состояния.
    /// </summary>
    private void UpdateVisualState()
    {
        if (buttonVisual != null)
        {
            buttonVisual.localPosition = isPressed ? initialPosition + pressedPositionOffset : initialPosition;
        }

        if (buttonRenderer != null)
        {
            if (isPressed)
            {
                buttonRenderer.material.color = pressedColor;
            }
            else
            {
                buttonRenderer.material.color = isHovering ? hoverColor : normalColor;
            }
        }
    }

    private void PlayPressSound()
    {
        if (audioSource != null && pressSound != null)
        {
            audioSource.PlayOneShot(pressSound);
        }
    }
}