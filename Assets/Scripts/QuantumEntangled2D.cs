using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class QuantumEntangled2D : MonoBehaviour
{
    [Header("Quantum Settings")]
    [SerializeField] private float observationRadius = 5f;
    [SerializeField] private float teleportProbability = 0.7f;
    [SerializeField] private float maxTeleportDistance = 10f;
    [SerializeField] private float temporalCloneDuration = 2f;
    [SerializeField] private Color entangledColor = new Color(0.2f, 0.8f, 1f, 0.7f);

    [Header("Physics Anomalies")]
    [SerializeField] private float gravityInversionProbability = 0.3f;
    [SerializeField] private float temporalDistortionForce = 50f;
    [SerializeField] private float quantumSpinTorque = 20f;

    private SpriteRenderer rend;
    private Rigidbody2D rb;
    private Camera mainCamera;
    private bool isBeingObserved;
    private float observationCheckCooldown = 0.1f;
    private float cooldownTimer;
    private Vector2 lastPosition;
    private GameObject temporalClone;
    private float originalGravityScale;

    void Awake()
    {
        // Автоматическая настройка компонентов
        SetupComponents();

        // Инициализация
        rend = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        lastPosition = transform.position;
        originalGravityScale = rb.gravityScale;

        // Настройка внешнего вида
        rend.color = entangledColor;
    }

    private void SetupComponents()
    {
        // Гарантируем наличие SpriteRenderer
        if (!TryGetComponent(out rend))
        {
            rend = gameObject.AddComponent<SpriteRenderer>();
            rend.sprite = CreateDefaultSprite();
        }

        // Гарантируем наличие Rigidbody2D
        if (!TryGetComponent<Rigidbody2D>(out rb))
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        // Гарантируем наличие коллайдера
        if (!GetComponent<Collider2D>())
        {
            gameObject.AddComponent<BoxCollider2D>();
        }

        // Автоматическое создание спрайта если нужно
        if (rend.sprite == null)
        {
            rend.sprite = CreateDefaultSprite();
        }
    }

    private Sprite CreateDefaultSprite()
    {
        // Создаем простой квадратный спрайт
        Texture2D texture = new Texture2D(16, 16);
        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                bool isEdge = x == 0 || x == 15 || y == 0 || y == 15;
                texture.SetPixel(x, y, isEdge ? Color.white : entangledColor);
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), Vector2.one * 0.5f);
    }

    void Start()
    {
        // Случайное вращение при старте
        rb.AddTorque(Random.Range(-quantumSpinTorque, quantumSpinTorque));
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0)
        {
            CheckObservation();
            cooldownTimer = observationCheckCooldown;
        }

        HandleQuantumEffects();
    }

    private void CheckObservation()
    {
        if (mainCamera == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(transform.position);
        bool inView = viewportPos.x > 0 && viewportPos.x < 1 &&
                     viewportPos.y > 0 && viewportPos.y < 1 &&
                     viewportPos.z > 0;

        float distance = Vector2.Distance(transform.position, mainCamera.transform.position);
        isBeingObserved = inView && distance <= observationRadius;

        // Эффект мерцания при наблюдении
        rend.color = Color.Lerp(
            entangledColor,
            Color.white,
            isBeingObserved ? Mathf.PingPong(Time.time * 2f, 0.5f) : 0f
        );
    }

    private void HandleQuantumEffects()
    {
        // Телепортация при отсутствии наблюдения
        if (!isBeingObserved && Random.value < teleportProbability * Time.deltaTime)
        {
            TeleportQuantumObject();
        }

        // Создание временных клонов
        if (Vector2.Distance(lastPosition, transform.position) > 0.1f &&
            temporalClone == null &&
            Random.value < 0.4f)
        {
            CreateTemporalClone();
        }
        lastPosition = transform.position;

        // Инверсия гравитации
        if (Random.value < gravityInversionProbability * Time.deltaTime)
        {
            InvertGravity();
        }
    }

    private void TeleportQuantumObject()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 teleportPosition = (Vector2)transform.position + randomDirection * Random.Range(1f, maxTeleportDistance);

        // Проверка допустимой позиции
        RaycastHit2D hit = Physics2D.Raycast(teleportPosition, Vector2.down, 2f);
        if (hit.collider != null)
        {
            transform.position = hit.point + Vector2.up * 0.2f;

            // Искажение пространства при телепорте
            ApplyTemporalDistortion();

            // Случайный импульс после телепортации
            rb.AddForce(randomDirection * 3f, ForceMode2D.Impulse);
        }
    }

    private void CreateTemporalClone()
    {
        temporalClone = new GameObject("Quantum Clone");
        temporalClone.transform.position = transform.position;
        temporalClone.transform.localScale = transform.localScale * 0.8f;

        SpriteRenderer cloneRenderer = temporalClone.AddComponent<SpriteRenderer>();
        cloneRenderer.sprite = rend.sprite;
        cloneRenderer.color = new Color(entangledColor.r, entangledColor.g, entangledColor.b, 0.4f);
        cloneRenderer.sortingOrder = rend.sortingOrder - 1;

        StartCoroutine(FadeAndDestroyClone(temporalClone, temporalCloneDuration));
    }

    private IEnumerator FadeAndDestroyClone(GameObject clone, float duration)
    {
        SpriteRenderer cloneRenderer = clone.GetComponent<SpriteRenderer>();
        Color startColor = cloneRenderer.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cloneRenderer.color = Color.Lerp(startColor, endColor, elapsed / duration);
            yield return null;
        }

        Destroy(clone);
        if (temporalClone == clone) temporalClone = null;
    }

    private void InvertGravity()
    {
        rb.gravityScale = -rb.gravityScale;
        rend.flipY = rb.gravityScale < 0;

        // Случайный вращательный импульс
        rb.AddTorque(Random.Range(-quantumSpinTorque, quantumSpinTorque));
    }

    private void ApplyTemporalDistortion()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 3f);
        foreach (Collider2D col in colliders)
        {
            Rigidbody2D colRb = col.GetComponent<Rigidbody2D>();
            if (colRb != null && colRb != rb)
            {
                Vector2 forceDirection = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                colRb.AddForce(forceDirection * temporalDistortionForce, ForceMode2D.Impulse);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, observationRadius);
        Gizmos.color = new Color(0.5f, 0f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, 3f);
    }
}