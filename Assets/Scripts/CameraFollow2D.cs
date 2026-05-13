using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow2D : MonoBehaviour
{
    [Header("Alvo da câmera")]
    [SerializeField] private Transform target;

    [Header("Movimento da câmera")]
    [SerializeField] private bool followY = false;
    [SerializeField] private float fixedY = 0f;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField] private float zPosition = -10f;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private bool snapOnStart = true;

    [Header("Limites horizontais da fase")]
    [SerializeField] private bool useHorizontalLimits = true;
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;
    [SerializeField] private float fallbackMinX = -40f;
    [SerializeField] private float fallbackMaxX = 40f;

    private Camera cameraComponent;
    private Vector3 velocity;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
    }

    private void Start()
    {
        if (snapOnStart)
        {
            transform.position = CalculateDesiredPosition();
            velocity = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = CalculateDesiredPosition();

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );
    }

    private Vector3 CalculateDesiredPosition()
    {
        if (target == null)
        {
            return transform.position;
        }

        float desiredX = target.position.x + offset.x;

        if (useHorizontalLimits)
        {
            float minX = leftLimit != null ? leftLimit.position.x : fallbackMinX;
            float maxX = rightLimit != null ? rightLimit.position.x : fallbackMaxX;

            if (minX > maxX)
            {
                float temporary = minX;
                minX = maxX;
                maxX = temporary;
            }

            float halfCameraWidth = cameraComponent.orthographicSize * cameraComponent.aspect;

            float minCameraX = minX + halfCameraWidth;
            float maxCameraX = maxX - halfCameraWidth;

            if (minCameraX > maxCameraX)
            {
                desiredX = (minX + maxX) / 2f;
            }
            else
            {
                desiredX = Mathf.Clamp(desiredX, minCameraX, maxCameraX);
            }
        }

        float desiredY = followY ? target.position.y + offset.y : fixedY + offset.y;

        return new Vector3(desiredX, desiredY, zPosition);
    }
}