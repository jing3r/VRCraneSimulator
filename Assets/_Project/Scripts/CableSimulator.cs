using UnityEngine;

/// <summary>
/// Имитирует провисающий кабель между двумя точками с помощью Line Renderer.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class CableSimulator : MonoBehaviour
{
    [Header("Точки крепления")]
    [Tooltip("Transform, откуда кабель исходит (например, корпус прибора).")]
    public Transform startPoint;

    [Tooltip("Transform, куда кабель приходит (например, зонд).")]
    public Transform endPoint;

    [Header("Настройки провисания")]
    [Tooltip("Количество сегментов для сглаживания кривой.")]
    [Range(3, 20)]
    [SerializeField] private int segments = 10;
    
    [Tooltip("Насколько сильно кабель провисает под действием 'гравитации'.")]
    [SerializeField] private float sagAmount = 0.5f;

    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();

        // Включаем Line Renderer при запуске, чтобы избежать залипания в редакторе.
        lineRenderer.enabled = true;
    }

    // Используем LateUpdate, чтобы кабель обновлял свою позицию
    // после того, как все расчеты физики и движения в Update уже завершены.
    private void LateUpdate()
    {
        if (startPoint == null || endPoint == null)
        {
            // Если одна из точек не задана, скрываем кабель.
            if (lineRenderer.positionCount > 0) { lineRenderer.positionCount = 0; }
            return;
        }
        
        DrawCable();
    }

    /// <summary>
    /// Рассчитывает и рисует кривую кабеля.
    /// </summary>
    private void DrawCable()
    {
        // Устанавливаем количество точек в Line Renderer.
        lineRenderer.positionCount = segments;

        Vector3 pos1 = startPoint.position;
        Vector3 pos2 = endPoint.position;
        
        // Рассчитываем точки кривой Безье
        // Контрольные точки делаем ниже, чтобы имитировать провисание
        Vector3 controlPoint1 = pos1 + Vector3.down * sagAmount;
        Vector3 controlPoint2 = pos2 + Vector3.down * sagAmount;

        for (int i = 0; i < segments; i++)
        {
            // t - это прогресс вдоль кривой от 0 до 1
            float t = (float)i / (segments - 1);
            
            // Расчет точки на кубической кривой Безье
            Vector3 point = CalculateCubicBezierPoint(t, pos1, controlPoint1, controlPoint2, pos2);
            
            lineRenderer.SetPosition(i, point);
        }
    }

    /// <summary>
    /// Рассчитывает точку на кубической кривой Безье.
    /// </summary>
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0; // (1-t)^3 * P0
        p += 3 * uu * t * p1; // 3 * (1-t)^2 * t * P1
        p += 3 * u * tt * p2; // 3 * (1-t) * t^2 * P2
        p += ttt * p3;        // t^3 * P3

        return p;
    }
}