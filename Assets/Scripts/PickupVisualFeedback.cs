using UnityEngine;
using System.Collections;

public class PickupVisualFeedback : MonoBehaviour
{
    [Header("Movimento visual")]
    public float bobHeight = 0.12f;
    public float bobSpeed = 3f;
    public float rotationSpeed = 25f;
    public float pulseAmount = 0.08f;
    public float pulseSpeed = 4f;

    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;
    private SpriteRenderer spriteRenderer;
    private bool collected = false;

    void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        baseLocalPosition = transform.localPosition;
        baseLocalScale = transform.localScale;
    }

    void Update()
    {
        if (collected)
            return;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        transform.localPosition = baseLocalPosition + new Vector3(0f, bob, 0f);
        transform.localScale = baseLocalScale * pulse;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    public void PlayCollectAndDestroy(float duration = 0.18f)
    {
        if (collected)
            return;

        collected = true;
        StartCoroutine(CollectRoutine(duration));
    }

    IEnumerator CollectRoutine(float duration)
    {
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;
        Color startColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            transform.localScale = Vector3.Lerp(startScale, startScale * 1.45f, t);

            if (spriteRenderer != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t);
                spriteRenderer.color = c;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}