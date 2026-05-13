using UnityEngine;
using System.Collections;

public class CharacterVisualFeedback : MonoBehaviour
{
    [Header("Referências")]
    public Transform visualRoot;
    public SpriteRenderer spriteRenderer;

    [Header("Idle simples")]
    public bool enableIdleMotion = true;
    public float idleBobHeight = 0.035f;
    public float idleBobSpeed = 2.5f;

    [Header("Caminhada procedural")]
    public bool enableWalkMotion = true;
    public float movementThreshold = 0.05f;
    public float walkBobHeight = 0.055f;
    public float walkBobSpeed = 10f;
    public float walkTiltAngle = 3.5f;
    public float walkForwardTilt = 2.5f;
    public float walkSquashAmount = 0.045f;
    public float walkHorizontalSway = 0.025f;

    [Header("Poeira de caminhada")]
    public bool enableWalkDust = true;
    public float walkDustInterval = 0.18f;
    public float walkDustSizeMultiplier = 1f;
    public float walkDustVerticalVelocityLimit = 0.25f;

    [Header("Hit Flash")]
    public Color hitFlashColor = new Color(1f, 0.15f, 0.15f, 1f);
    public float hitFlashDuration = 0.08f;

    [Header("Ataque leve")]
    public float lightAttackMoveDistance = 0.12f;
    public float lightAttackDuration = 0.10f;

    [Header("Ataque pesado")]
    public float heavyWindupDistance = 0.12f;
    public float heavyStrikeDistance = 0.34f;
    public float heavyAttackDuration = 0.26f;

    [Header("Aura / Skill 3")]
    public Color auraColor = new Color(0.25f, 0.85f, 1f, 0.45f);
    public float auraSilhouetteScale = 1.18f;
    public float auraPulseAmount = 0.07f;
    public float auraPulseSpeed = 6f;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private Quaternion baseLocalRotation;
    private Color baseColor;

    private Coroutine hitRoutine;
    private Coroutine attackRoutine;
    private Coroutine pulseRoutine;
    private Coroutine deathRoutine;

    private GameObject auraObject;
    private SpriteRenderer auraRenderer;
    private bool auraActive = false;
    private float auraTimer = 0f;

    private Rigidbody2D rb;
    private Collider2D mainCollider;
    private PlayerController playerController;

    private Vector3 previousWorldPosition;
    private float horizontalSpeed;
    private float verticalSpeed;
    private float walkPhase;
    private float nextDustTime;

    void Awake()
    {
        ResolveReferences();
        CaptureBaseValues();

        previousWorldPosition = transform.position;
    }

    void OnEnable()
    {
        ResolveReferences();
        CaptureBaseValues();

        previousWorldPosition = transform.position;
        walkPhase = 0f;
        nextDustTime = 0f;
    }

    void Update()
    {
        UpdateMovementInfo();
        UpdatePoseMotion();
        UpdateAura();
    }

    void ResolveReferences()
    {
        if (visualRoot == null)
        {
            Transform foundVisual = transform.Find("Visual");

            if (foundVisual != null)
            {
                visualRoot = foundVisual;
            }
        }

        if (spriteRenderer == null)
        {
            if (visualRoot != null)
            {
                spriteRenderer = visualRoot.GetComponent<SpriteRenderer>();
            }

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        if (visualRoot == null && spriteRenderer != null)
        {
            visualRoot = spriteRenderer.transform;
        }

        if (visualRoot == null)
        {
            visualRoot = transform;
        }

        rb = GetComponent<Rigidbody2D>();
        mainCollider = GetComponent<Collider2D>();
        playerController = GetComponent<PlayerController>();
    }

    void CaptureBaseValues()
    {
        if (visualRoot != null)
        {
            baseLocalPosition = visualRoot.localPosition;
            baseLocalScale = visualRoot.localScale;
            baseLocalRotation = visualRoot.localRotation;
        }

        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }
    }

    bool HasSeparateVisual()
    {
        return visualRoot != null && visualRoot != transform;
    }

    void UpdateMovementInfo()
    {
        if (rb != null)
        {
            horizontalSpeed = rb.linearVelocity.x;
            verticalSpeed = rb.linearVelocity.y;
            previousWorldPosition = transform.position;
            return;
        }

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 delta = transform.position - previousWorldPosition;

        horizontalSpeed = delta.x / deltaTime;
        verticalSpeed = delta.y / deltaTime;

        previousWorldPosition = transform.position;
    }

    void UpdatePoseMotion()
    {
        if (!HasSeparateVisual())
            return;

        if (attackRoutine != null || pulseRoutine != null || deathRoutine != null)
            return;

        if (enableWalkMotion && IsWalking())
        {
            ApplyWalkMotion();
        }
        else
        {
            ApplyIdleMotion();
        }
    }

    bool IsWalking()
    {
        if (Mathf.Abs(horizontalSpeed) <= movementThreshold)
            return false;

        if (playerController != null && !playerController.IsGrounded())
            return false;

        return true;
    }

    void ApplyIdleMotion()
    {
        if (!enableIdleMotion)
        {
            visualRoot.localPosition = baseLocalPosition;
            visualRoot.localScale = baseLocalScale;
            visualRoot.localRotation = baseLocalRotation;
            return;
        }

        float offsetY = Mathf.Sin(Time.time * idleBobSpeed) * idleBobHeight;

        visualRoot.localPosition = baseLocalPosition + new Vector3(0f, offsetY, 0f);
        visualRoot.localScale = baseLocalScale;
        visualRoot.localRotation = baseLocalRotation;
    }

    void ApplyWalkMotion()
    {
        float direction = horizontalSpeed >= 0f ? 1f : -1f;
        float speedFactor = Mathf.Clamp(Mathf.Abs(horizontalSpeed), 0.6f, 5f);

        walkPhase += Time.deltaTime * walkBobSpeed * Mathf.Lerp(0.7f, 1.25f, Mathf.InverseLerp(0.6f, 5f, speedFactor));

        float sin = Mathf.Sin(walkPhase);
        float absSin = Mathf.Abs(sin);

        float bobY = absSin * walkBobHeight;
        float swayX = sin * walkHorizontalSway * direction;

        float squashX = 1f + (absSin * walkSquashAmount);
        float squashY = 1f - (absSin * walkSquashAmount * 0.45f);

        float tilt = (sin * walkTiltAngle) - (direction * walkForwardTilt);

        visualRoot.localPosition = baseLocalPosition + new Vector3(swayX, bobY, 0f);
        visualRoot.localScale = new Vector3(
            baseLocalScale.x * squashX,
            baseLocalScale.y * squashY,
            baseLocalScale.z
        );
        visualRoot.localRotation = Quaternion.Euler(0f, 0f, tilt);

        TrySpawnWalkDust(direction);
    }

    void TrySpawnWalkDust(float direction)
    {
        if (!enableWalkDust)
            return;

        if (Time.time < nextDustTime)
            return;

        if (Mathf.Abs(verticalSpeed) > walkDustVerticalVelocityLimit)
            return;

        Vector3 dustPosition = transform.position + Vector3.down * 0.45f;

        if (mainCollider != null)
        {
            Bounds bounds = mainCollider.bounds;
            dustPosition = new Vector3(bounds.center.x, bounds.min.y + 0.04f, transform.position.z);
        }

        dustPosition += new Vector3(-direction * 0.20f, 0f, 0f);

        SimpleVFX.SpawnWalkDust(dustPosition, direction, walkDustSizeMultiplier);

        nextDustTime = Time.time + walkDustInterval;
    }

    void UpdateAura()
    {
        if (!auraActive || auraObject == null || auraRenderer == null)
            return;

        auraTimer += Time.deltaTime;

        if (spriteRenderer != null)
        {
            auraRenderer.sprite = spriteRenderer.sprite;
            auraRenderer.flipX = spriteRenderer.flipX;
            auraRenderer.flipY = spriteRenderer.flipY;
            auraRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
            auraRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
        }

        float pulse = 1f + Mathf.Sin(auraTimer * auraPulseSpeed) * auraPulseAmount;
        auraObject.transform.localScale = Vector3.one * auraSilhouetteScale * pulse;

        Color c = auraColor;
        c.a = Mathf.Lerp(auraColor.a * 0.75f, auraColor.a, (Mathf.Sin(auraTimer * auraPulseSpeed) + 1f) * 0.5f);
        auraRenderer.color = c;
    }

    public SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    public void PlayHitFlash()
    {
        if (spriteRenderer == null)
            return;

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitFlashRoutine(hitFlashColor));
    }

    public void PlayInvincibleBlockFeedback()
    {
        if (spriteRenderer == null)
            return;

        if (hitRoutine != null)
        {
            StopCoroutine(hitRoutine);
        }

        hitRoutine = StartCoroutine(HitFlashRoutine(new Color(0.35f, 0.85f, 1f, 1f)));
    }

    IEnumerator HitFlashRoutine(Color flashColor)
    {
        spriteRenderer.color = flashColor;

        yield return new WaitForSeconds(hitFlashDuration);

        spriteRenderer.color = baseColor;
        hitRoutine = null;
    }

    public void PlayLightAttack(Vector2 direction)
    {
        if (!HasSeparateVisual())
        {
            PlaySkillCastPulse(new Color(1f, 0.85f, 0.35f, 1f));
            return;
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(LightAttackRoutine(direction));
    }

    IEnumerator LightAttackRoutine(Vector2 direction)
    {
        direction = NormalizeDirection(direction);

        float halfDuration = Mathf.Max(0.01f, lightAttackDuration / 2f);

        Vector3 startPosition = baseLocalPosition;
        Vector3 targetPosition = baseLocalPosition + new Vector3(direction.x * lightAttackMoveDistance, 0f, 0f);

        Vector3 startScale = baseLocalScale;
        Vector3 targetScale = new Vector3(
            baseLocalScale.x * 1.06f,
            baseLocalScale.y * 0.96f,
            baseLocalScale.z
        );

        Quaternion startRotation = baseLocalRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, -direction.x * 4f);

        float elapsed = 0f;

        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            visualRoot.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
            visualRoot.localScale = Vector3.Lerp(startScale, targetScale, t);
            visualRoot.localRotation = Quaternion.Lerp(startRotation, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < halfDuration)
        {
            float t = elapsed / halfDuration;
            visualRoot.localPosition = Vector3.Lerp(targetPosition, startPosition, t);
            visualRoot.localScale = Vector3.Lerp(targetScale, startScale, t);
            visualRoot.localRotation = Quaternion.Lerp(targetRotation, startRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetVisualToBase();

        attackRoutine = null;
    }

    public void PlayHeavyAttack(Vector2 direction)
    {
        if (!HasSeparateVisual())
        {
            PlaySkillCastPulse(new Color(1f, 0.45f, 0.10f, 1f));
            return;
        }

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(HeavyAttackRoutine(direction));
    }

    IEnumerator HeavyAttackRoutine(Vector2 direction)
    {
        direction = NormalizeDirection(direction);

        float windupTime = heavyAttackDuration * 0.35f;
        float strikeTime = heavyAttackDuration * 0.35f;
        float recoverTime = heavyAttackDuration * 0.30f;

        Vector3 startPosition = baseLocalPosition;
        Vector3 windupPosition = baseLocalPosition - new Vector3(direction.x * heavyWindupDistance, 0f, 0f);
        Vector3 strikePosition = baseLocalPosition + new Vector3(direction.x * heavyStrikeDistance, 0f, 0f);

        Vector3 startScale = baseLocalScale;
        Vector3 windupScale = new Vector3(
            baseLocalScale.x * 0.94f,
            baseLocalScale.y * 1.08f,
            baseLocalScale.z
        );

        Vector3 strikeScale = new Vector3(
            baseLocalScale.x * 1.20f,
            baseLocalScale.y * 0.88f,
            baseLocalScale.z
        );

        Quaternion startRotation = baseLocalRotation;
        Quaternion windupRotation = Quaternion.Euler(0f, 0f, direction.x * 5f);
        Quaternion strikeRotation = Quaternion.Euler(0f, 0f, -direction.x * 8f);

        float elapsed = 0f;

        while (elapsed < windupTime)
        {
            float t = elapsed / windupTime;
            visualRoot.localPosition = Vector3.Lerp(startPosition, windupPosition, t);
            visualRoot.localScale = Vector3.Lerp(startScale, windupScale, t);
            visualRoot.localRotation = Quaternion.Lerp(startRotation, windupRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < strikeTime)
        {
            float t = elapsed / strikeTime;
            visualRoot.localPosition = Vector3.Lerp(windupPosition, strikePosition, t);
            visualRoot.localScale = Vector3.Lerp(windupScale, strikeScale, t);
            visualRoot.localRotation = Quaternion.Lerp(windupRotation, strikeRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < recoverTime)
        {
            float t = elapsed / recoverTime;
            visualRoot.localPosition = Vector3.Lerp(strikePosition, startPosition, t);
            visualRoot.localScale = Vector3.Lerp(strikeScale, startScale, t);
            visualRoot.localRotation = Quaternion.Lerp(strikeRotation, startRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetVisualToBase();

        attackRoutine = null;
    }

    Vector2 NormalizeDirection(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) < 0.01f)
        {
            return Vector2.right;
        }

        return direction.x >= 0f ? Vector2.right : Vector2.left;
    }

    public void PlayAttackBump(float intensity = 1f)
    {
        PlayLightAttack(Vector2.right);
    }

    public void PlaySkillCastPulse(Color pulseColor)
    {
        if (spriteRenderer == null)
            return;

        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
        }

        pulseRoutine = StartCoroutine(SkillCastPulseRoutine(pulseColor));
    }

    IEnumerator SkillCastPulseRoutine(Color pulseColor)
    {
        spriteRenderer.color = pulseColor;

        if (HasSeparateVisual())
        {
            visualRoot.localScale = baseLocalScale * 1.08f;
        }

        yield return new WaitForSeconds(0.12f);

        spriteRenderer.color = baseColor;

        if (HasSeparateVisual())
        {
            ResetVisualToBase();
        }

        pulseRoutine = null;
    }

    public void PlayLevelUpEffect()
    {
        PlaySkillCastPulse(new Color(1f, 0.85f, 0.2f, 1f));
        SimpleVFX.SpawnLevelUp(transform.position + Vector3.up * 1.1f);
    }

    public void SetAuraActive(bool active, Color color)
    {
        auraActive = active;
        auraColor = color;

        if (active)
        {
            EnsureAuraObject();

            if (auraObject != null)
            {
                auraObject.SetActive(true);
            }

            if (auraRenderer != null)
            {
                auraRenderer.color = auraColor;
            }
        }
        else
        {
            if (auraObject != null)
            {
                auraObject.SetActive(false);
            }
        }
    }

    void EnsureAuraObject()
    {
        if (auraObject != null)
            return;

        if (visualRoot == null || spriteRenderer == null)
            return;

        auraObject = new GameObject("Aura_Silhouette");
        auraObject.transform.SetParent(visualRoot);
        auraObject.transform.localPosition = Vector3.zero;
        auraObject.transform.localRotation = Quaternion.identity;
        auraObject.transform.localScale = Vector3.one * auraSilhouetteScale;

        auraRenderer = auraObject.AddComponent<SpriteRenderer>();
        auraRenderer.sprite = spriteRenderer.sprite;
        auraRenderer.flipX = spriteRenderer.flipX;
        auraRenderer.flipY = spriteRenderer.flipY;
        auraRenderer.color = auraColor;
        auraRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        auraRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
    }

    public void PlayDeathEffectAndDestroy(GameObject target, float duration = 0.25f)
    {
        if (deathRoutine != null)
            return;

        deathRoutine = StartCoroutine(DeathRoutine(target, duration));
    }

    IEnumerator DeathRoutine(GameObject target, float duration)
    {
        float elapsed = 0f;

        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        Vector3 startScale = visualRoot != null ? visualRoot.localScale : Vector3.one;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            if (spriteRenderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = c;
            }

            if (HasSeparateVisual())
            {
                visualRoot.localScale = Vector3.Lerp(startScale, startScale * 0.75f, t);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (target != null)
        {
            Destroy(target);
        }
    }

    void ResetVisualToBase()
    {
        if (visualRoot == null)
            return;

        visualRoot.localPosition = baseLocalPosition;
        visualRoot.localScale = baseLocalScale;
        visualRoot.localRotation = baseLocalRotation;
    }
}