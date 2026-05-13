using UnityEngine;

public static class SimpleVFX
{
    private static Sprite circleSprite;

    public static Sprite GetCircleSprite()
    {
        if (circleSprite == null)
        {
            circleSprite = CreateCircleSprite(128);
        }

        return circleSprite;
    }

    private static Sprite CreateCircleSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;

                if (distance <= 1f)
                {
                    float alpha = Mathf.Clamp01((1f - distance) * 1.45f);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );
    }

    public static void SpawnCircleBurst(Vector3 position, float radius, Color color, float duration = 0.35f, int orderInLayer = 50)
    {
        SpawnEllipseBurst(
            position,
            new Vector2(0.15f, 0.15f),
            new Vector2(radius * 2f, radius * 2f),
            color,
            duration,
            orderInLayer,
            0f
        );
    }

    public static void SpawnEllipseBurst(
        Vector3 position,
        Vector2 startSize,
        Vector2 endSize,
        Color color,
        float duration = 0.35f,
        int orderInLayer = 50,
        float rotationZ = 0f
    )
    {
        GameObject obj = new GameObject("VFX_EllipseBurst");
        obj.transform.position = position;
        obj.transform.rotation = Quaternion.Euler(0f, 0f, rotationZ);

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = GetCircleSprite();
        sr.color = color;
        sr.sortingLayerName = "VFX";
        sr.sortingOrder = orderInLayer;

        SimpleVFXObject vfx = obj.AddComponent<SimpleVFXObject>();
        vfx.Initialize(
            sr,
            new Vector3(startSize.x, startSize.y, 1f),
            new Vector3(endSize.x, endSize.y, 1f),
            color,
            duration
        );
    }

    public static void SpawnHitSpark(Vector3 position)
    {
        SpawnCircleBurst(
            position,
            0.32f,
            new Color(1f, 0.75f, 0.15f, 0.85f),
            0.16f,
            75
        );
    }

    public static void SpawnPunchImpact(Vector3 position, Vector2 direction, bool heavy)
    {
        float dir = direction.x >= 0f ? 1f : -1f;

        if (heavy)
        {
            SpawnEllipseBurst(
                position,
                new Vector2(0.15f, 0.12f),
                new Vector2(1.15f, 0.46f),
                new Color(1f, 0.38f, 0.05f, 0.78f),
                0.20f,
                85,
                0f
            );

            SpawnCircleBurst(
                position + new Vector3(dir * 0.18f, 0f, 0f),
                0.52f,
                new Color(1f, 0.85f, 0.1f, 0.78f),
                0.14f,
                90
            );

            ShakeCamera(0.08f, 0.055f);
        }
        else
        {
            SpawnEllipseBurst(
                position,
                new Vector2(0.10f, 0.08f),
                new Vector2(0.65f, 0.28f),
                new Color(1f, 0.82f, 0.25f, 0.68f),
                0.13f,
                80,
                0f
            );

            SpawnCircleBurst(
                position + new Vector3(dir * 0.08f, 0f, 0f),
                0.26f,
                new Color(1f, 0.95f, 0.45f, 0.65f),
                0.10f,
                85
            );
        }
    }

    public static void SpawnGroundShockwave(Vector3 groundPosition, float radius)
    {
        SpawnEllipseBurst(
            groundPosition,
            new Vector2(0.25f, 0.08f),
            new Vector2(radius * 2.35f, 0.38f),
            new Color(1f, 0.48f, 0.05f, 0.70f),
            0.34f,
            80,
            0f
        );

        SpawnEllipseBurst(
            groundPosition + Vector3.up * 0.06f,
            new Vector2(0.15f, 0.06f),
            new Vector2(radius * 1.45f, 0.20f),
            new Color(1f, 0.88f, 0.20f, 0.85f),
            0.22f,
            85,
            0f
        );

        SpawnCircleBurst(
            groundPosition + Vector3.up * 0.12f,
            0.55f,
            new Color(1f, 0.70f, 0.08f, 0.75f),
            0.18f,
            90
        );

        SpawnEllipseBurst(
            groundPosition + Vector3.left * (radius * 0.35f),
            new Vector2(0.12f, 0.06f),
            new Vector2(radius * 0.65f, 0.16f),
            new Color(1f, 0.30f, 0.05f, 0.55f),
            0.24f,
            82,
            0f
        );

        SpawnEllipseBurst(
            groundPosition + Vector3.right * (radius * 0.35f),
            new Vector2(0.12f, 0.06f),
            new Vector2(radius * 0.65f, 0.16f),
            new Color(1f, 0.30f, 0.05f, 0.55f),
            0.24f,
            82,
            0f
        );

        ShakeCamera(0.10f, 0.075f);
    }

    public static void SpawnUltimateBurst(Vector3 position, float radius)
    {
        SpawnCircleBurst(
            position,
            radius * 1.15f,
            new Color(1f, 0.12f, 0.02f, 0.78f),
            0.48f,
            100
        );

        SpawnCircleBurst(
            position,
            radius * 0.72f,
            new Color(1f, 0.65f, 0.05f, 0.88f),
            0.30f,
            105
        );

        SpawnCircleBurst(
            position,
            radius * 0.34f,
            new Color(1f, 0.95f, 0.25f, 0.95f),
            0.18f,
            110
        );

        SpawnEllipseBurst(
            position,
            new Vector2(0.35f, 0.12f),
            new Vector2(radius * 2.85f, 0.52f),
            new Color(1f, 0.25f, 0.02f, 0.62f),
            0.36f,
            102,
            0f
        );

        SpawnEllipseBurst(
            position + Vector3.up * 0.15f,
            new Vector2(0.20f, 0.10f),
            new Vector2(radius * 1.9f, 0.38f),
            new Color(1f, 0.82f, 0.06f, 0.72f),
            0.24f,
            106,
            0f
        );

        ShakeCamera(0.22f, 0.18f);
    }

    public static void SpawnWalkDust(Vector3 position, float directionX, float sizeMultiplier = 1f)
    {
        float dir = directionX >= 0f ? -1f : 1f;

        SpawnEllipseBurst(
            position + new Vector3(dir * 0.12f, 0f, 0f),
            new Vector2(0.05f, 0.025f) * sizeMultiplier,
            new Vector2(0.38f, 0.10f) * sizeMultiplier,
            new Color(0.72f, 0.48f, 0.28f, 0.38f),
            0.24f,
            35,
            0f
        );
    }

    public static void SpawnCollect(Vector3 position, Color color)
    {
        SpawnCircleBurst(
            position,
            0.45f,
            color,
            0.22f,
            70
        );
    }

    public static void SpawnLevelUp(Vector3 position)
    {
        SpawnCircleBurst(
            position,
            1.15f,
            new Color(1f, 0.85f, 0.15f, 0.72f),
            0.45f,
            80
        );

        SpawnCircleBurst(
            position + Vector3.up * 0.1f,
            0.55f,
            new Color(1f, 1f, 0.45f, 0.85f),
            0.28f,
            85
        );
    }

    public static void SpawnAfterimage(SpriteRenderer source, float duration, Color color)
    {
        if (source == null || source.sprite == null)
            return;

        GameObject obj = new GameObject("VFX_Afterimage");
        obj.transform.position = source.transform.position;
        obj.transform.rotation = source.transform.rotation;
        obj.transform.localScale = source.transform.lossyScale;

        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = source.sprite;
        sr.flipX = source.flipX;
        sr.flipY = source.flipY;
        sr.color = color;
        sr.sortingLayerName = "VFX";
        sr.sortingOrder = Mathf.Max(0, source.sortingOrder - 1);

        SimpleVFXObject vfx = obj.AddComponent<SimpleVFXObject>();
        vfx.Initialize(
            sr,
            obj.transform.localScale,
            obj.transform.localScale * 1.03f,
            color,
            duration
        );
    }

    public static void ShakeCamera(float duration, float strength)
    {
        Camera cam = Camera.main;

        if (cam == null)
            return;

        SimpleCameraShake shaker = cam.GetComponent<SimpleCameraShake>();

        if (shaker == null)
        {
            shaker = cam.gameObject.AddComponent<SimpleCameraShake>();
        }

        shaker.Shake(duration, strength);
    }
}

public class SimpleVFXObject : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Vector3 startScale;
    private Vector3 endScale;
    private Color startColor;
    private float duration;
    private float elapsed;

    public void Initialize(SpriteRenderer renderer, Vector3 initialScale, Vector3 finalScale, Color color, float lifeTime)
    {
        spriteRenderer = renderer;
        startScale = initialScale;
        endScale = finalScale;
        startColor = color;
        duration = Mathf.Max(0.01f, lifeTime);

        transform.localScale = startScale;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);
        float eased = 1f - Mathf.Pow(1f - t, 2f);

        transform.localScale = Vector3.Lerp(startScale, endScale, eased);

        if (spriteRenderer != null)
        {
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t);
            spriteRenderer.color = c;
        }

        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }
}

public class SimpleCameraShake : MonoBehaviour
{
    private float duration;
    private float strength;
    private float elapsed;
    private Vector3 currentOffset;

    public void Shake(float newDuration, float newStrength)
    {
        duration = Mathf.Max(duration, newDuration);
        strength = Mathf.Max(strength, newStrength);
        elapsed = 0f;
    }

    void LateUpdate()
    {
        if (currentOffset != Vector3.zero)
        {
            transform.localPosition -= currentOffset;
            currentOffset = Vector3.zero;
        }

        if (elapsed >= duration)
            return;

        elapsed += Time.deltaTime;

        float t = Mathf.Clamp01(elapsed / duration);
        float fade = 1f - t;

        currentOffset = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ) * strength * fade;

        transform.localPosition += currentOffset;

        if (elapsed >= duration)
        {
            duration = 0f;
            strength = 0f;
        }
    }

    void OnDisable()
    {
        if (currentOffset != Vector3.zero)
        {
            transform.localPosition -= currentOffset;
            currentOffset = Vector3.zero;
        }
    }
}