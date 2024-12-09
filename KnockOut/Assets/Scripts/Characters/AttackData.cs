using UnityEngine;

public enum AttackType
{
    Physical,   // Ataques cuerpo a cuerpo o con armas
    Projectile, // Ataques que generan proyectiles
    Special,    // Ataques con reglas únicas o especiales
    Healing,    // Ataques que curan al personaje
}

[CreateAssetMenu(fileName = "NewAttackData", menuName = "Attack Data", order = 52)]
public class AttackData : ScriptableObject
{
    [Header("General Settings")]
    [Tooltip("Nombre descriptivo del ataque, como 'Espadazo' o 'Rayo de fuego'.")]
    public string attackName;

    [Tooltip("Animación asociada al ataque que se reproducirá durante su ejecución.")]
    public AnimationClip attackAnimation;

    [Tooltip("Sonidos que se reproducen (aleatoriamente) cuando el ataque es ejecutado.")]
    public AudioClip[] attackSounds;

    [Tooltip("Sonidos que se reproducen al golpear al enemigo.")]
    public AudioClip[] hitSounds;

    [Header("Attack Phases")]
    [Tooltip("Tiempo (en segundos) antes de que el ataque sea activo tras presionar el botón.")]
    public float startUpTime;

    [Tooltip("Duración (en segundos) durante la cual la hitbox puede golpear al enemigo.")]
    public float activeTime;

    [Tooltip("Tiempo para poder hacer este ataque otra vez.")]
    public float cooldown;

    [Tooltip("Tiempo (en segundos) posterior al ataque antes de realizar otra acción.")]
    public float recoveryTime;
    [Tooltip("El personaje no podra moverse hasta el recovery.")]
    public bool freezerAttack;

    [Header("Hitbox Settings")]
    [Tooltip("Posición relativa del área de impacto en coordenadas locales del personaje.")]
    public Vector2 hitboxPosition;

    [Tooltip("Tamaño del área de impacto (ancho y alto en unidades).")]
    public Vector2 hitboxSize;

    [Header("Damage and Knockback")]
    [Tooltip("Daño infligido al enemigo en porcentaje.")]
    public float damage;

    [Tooltip("Dirección del knockback aplicado (ejemplo: (1, 1) para diagonal superior derecha, debe apuntar a derecha, se gira solo).")]
    public Vector2 knockback;

    [Tooltip("Factor multiplicador que aumenta el knockback basado en el daño acumulado del enemigo (No mas de 1,5).")]
    public float knockbackScaling;

    [Tooltip("Knockback base, independiente del daño acumulado del enemigo.")]
    public float baseKnockback;

    [Tooltip("Multiplicador adicional de knockback para condiciones especiales.")]
    public float knockbackMultiplier = 1.0f;

    [Tooltip("Prioridad del ataque: ataques con mayor prioridad vencerán a otros.")]
    public int priority;

    [Header("Healing Settings")]
    [Tooltip("Curación mínima que este ataque puede realizar (si es un ataque de tipo curación).")]
    public float minHealing;

    [Tooltip("Curación máxima que este ataque puede realizar (si es un ataque de tipo curación).")]
    public float maxHealing;

    [Header("Attack Conditions")]
    [Tooltip("Requiere que el personaje esté parado (no en movimiento) para usar el ataque.")]
    public bool requiresStanding;

    [Tooltip("Permite que el ataque se realice mientras el personaje está en el aire.")]
    public bool usableInAir;

    [Tooltip("Define el tipo de ataque: físico, proyectil, especial o curación.")]
    public AttackType attackType;

    [Header("Projectile Settings")]
    [Tooltip("Prefab del proyectil que se generará al realizar el ataque (si aplica).")]
    public GameObject projectilePrefab;

    [Tooltip("Velocidad del proyectil en unidades por segundo.")]
    public float projectileSpeed;

    [Tooltip("Máximo número de proyectiles que pueden estar activos simultáneamente.")]
    public int maxProjectiles;

    [Tooltip("Duración (en segundos) antes de que el proyectil desaparezca automáticamente.")]
    public float projectileLifetime;
    [Tooltip ("Anguno de disparo del proyectil en grados.")]
    public float projectileAngle;

    /// <summary>
    /// Devuelve un sonido aleatorio de una lista de clips.
    /// </summary>
    /// <param name="clips">Array de clips de audio.</param>
    /// <returns>Un clip de audio aleatorio o null si no hay clips.</returns>
    public AudioClip GetRandomAudioClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("No audio clips available.");
            return null;
        }
        int randomIndex = Random.Range(0, clips.Length);
        return clips[randomIndex];
    }
}
