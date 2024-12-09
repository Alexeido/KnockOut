using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class AttackHandler : MonoBehaviour
{
    public CharacterBaseController characterController; // Referencia al controlador principal
    private Animator animator; // Referencia al Animator
    private Rigidbody2D rb; // Referencia al Rigidbody2D
    public bool isAttacking = false; // Indica si el personaje está en un ataque activo
    private float lastAttackTime = 0f; // Tiempo del último ataque
    [Header("Attack Sounds")]
    public List<AudioClip> attackSounds = new List<AudioClip>();

    private float[] cooldowns; // Tiempo de ejecucion de ese ultimo ataque para ver si se puede hacer otro


    private Collider2D[] hitResults = new Collider2D[10]; // Buffer para detección de colisiones

    public void Initialize(CharacterBaseController controller, Animator animator, Rigidbody2D rb)
    {
        this.characterController = controller;
        this.animator = animator;
        this.rb = rb;
        LoadAttackSounds("SonidosDefault");
        cooldowns = new float[characterController.characterData.attacks.Length];
    }

    public void HandleAttack(int attackIndex)
    {
        if (isAttacking){
            Debug.Log("No se puede atacar en este momento. 1");
            return;
        }

        if (attackIndex < 0 || attackIndex >= characterController.characterData.attacks.Length){
            Debug.Log("No se puede atacar en este momento. 2");
            return;
        }
        AttackData attack = characterController.characterData.attacks[attackIndex];
        if (attack.requiresStanding && Mathf.Abs(Input.GetAxis(characterController.horizontalInput))!=0){
            Debug.Log("No se puede atacar en este momento. 3");
            return;
        }
        if (!attack.usableInAir && !characterController.isGrounded){
            Debug.Log("No se puede atacar en este momento. 4");
            return;
        } 
        if (Time.time - cooldowns[attackIndex] < attack.cooldown){
            Debug.Log("No se puede atacar en este momento. 5");
            return;
        }
        cooldowns[attackIndex] = Time.time;
        StartCoroutine(PerformAttack(attack, attackIndex));
    }
    private IEnumerator PerformAttack(AttackData attack, int index){
        isAttacking = true;
        // Reproducir la animación del ataque
        characterController.PlayAnimation("Attack"+index);
        if (attack.freezerAttack)
            characterController.freezing = true;

        // Esperar el número de frames especificado por startUpTime
        //Traducimos de frames a segundos
        float currentFPS = 1f / Time.deltaTime;
        float framesToSeconds = attack.startUpTime*3 / currentFPS;
        //Debug.Log(framesToSeconds);
        yield return new WaitForSeconds(attack.startUpTime/10f);

        if (attack.attackType == AttackType.Healing)
        {
            // Curación si aplica
            float healingAmount = Random.Range(attack.minHealing, attack.maxHealing);
            characterController.porcentajeVida -= healingAmount;
            if (characterController.porcentajeVida < 0f)
            {
                characterController.porcentajeVida = 0f;
            }
            characterController.porcentajeVida = Mathf.Round(characterController.porcentajeVida * 100f) / 100f;
            Debug.Log($"Curación de {healingAmount} aplicada.");
        }
        // Activar hitbox si aplica
        if (attack.attackType == AttackType.Projectile && attack.projectilePrefab != null)
        {
            // Proyectil si aplica
            ShootProjectile(attack);
        }
        else
        {
            ActivateHitbox(attack);
        }
        // Sonido de ataque aleatorio
        characterController.StopSound(1);
        characterController.PlaySound(attack.attackSounds,1);

        // Esperar el número de frames especificado por activeTime
        framesToSeconds = attack.activeTime*3 / currentFPS;
        //Debug.Log(framesToSeconds);
        yield return new WaitForSeconds(attack.activeTime/10f);
        // Desactivar hitbox
        DeactivateHitbox();

        // Esperar el número de frames especificado por recoveryTime
        framesToSeconds = attack.recoveryTime*3 / currentFPS;
        //Debug.Log(framesToSeconds);
        yield return new WaitForSeconds(attack.recoveryTime/10f);
        if (attack.freezerAttack)
            characterController.freezing = false;

        isAttacking = false;
        lastAttackTime = Time.time;
        Debug.Log($"Ataque {attack.attackName} finalizado.");
    }


    private void ActivateHitbox(AttackData attack)
    {
        if (attack.hitboxSize==Vector2.zero){
            Debug.LogWarning("Hitbox no configurada en el ataque "+attack.attackName);
            return;
        }
        // Crear hitbox en posición relativa
        int derecha = 1;
        if (!characterController.facingRight)
        {
            derecha = -1;
        }
        Vector2 hitboxPosition = (Vector2)transform.position + new Vector2(attack.hitboxPosition.x * derecha, attack.hitboxPosition.y);

        int hitCount = Physics2D.OverlapBoxNonAlloc(
            hitboxPosition,
            attack.hitboxSize,
            0,
            hitResults
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hit = hitResults[i];

            // Verificar que no es el mismo objeto que realiza el ataque
            if (hit.gameObject == gameObject) continue;

            // Verificar que es otro jugador
            CharacterBaseController otherPlayer = hit.GetComponent<CharacterBaseController>();
            if (otherPlayer != null && otherPlayer.playerID != characterController.playerID)
            {
                ApplyDamage(otherPlayer.gameObject, attack);
            }
        }
    }

    private void DeactivateHitbox()
    {
    }

private Coroutine activeKnockbackCoroutine;

// Método para aplicar daño y knockback al personaje atacado
public void ApplyDamage(GameObject target, AttackData attack)
{
    Debug.Log($"{target.name} recibe {attack.damage} de daño.");

    // Obtener el controlador del personaje atacado
    CharacterBaseController targetController = target.GetComponent<CharacterBaseController>();
    if (targetController == null || targetController.invulnerable)
    {
        return;
    }
    if (targetController != null)
    { // el daño mas un numero aleatorio del 0 al 1
        targetController.porcentajeVida += attack.damage + Random.Range(0f, 1f);
        targetController.porcentajeVida = Mathf.Round(targetController.porcentajeVida * 100f) / 100f;
    }
    targetController.StopSound(3);
    targetController.PlaySound(attack.hitSounds,3);
    targetController.PlaySound(targetController.characterData.damageSounds,1);
    targetController.hitted();

    Rigidbody2D targetRb = target.GetComponent<Rigidbody2D>();
    if (targetRb != null && targetController != null)
    {
        float attackerX = transform.position.x;
        float targetX = target.transform.position.x;
        bool attackerOnLeft = attackerX < targetX;

        // Variables de la fórmula de knockback sacada de smash bros meelee
        float p = attack.damage;  // Daño base del ataque
        float d = targetController.porcentajeVida;  // Daño acumulado por el objetivo
        float w = targetController.characterData.weight;  // Peso del objetivo
        float s = attack.knockbackScaling;  // Escalado del knockback
        float b = attack.baseKnockback;  // Knockback base (como magnitud)
        float r = attack.knockbackMultiplier;  // Multiplicador adicional, ajustable según contexto (terreno, etc.)

        // Fórmula de cálculo del knockback
        float knockbackMagnitude = ((((p / 10f) + (p * d / 20f)) * (200f / (w + 100f)) * 1.4f) + 18f) * s + b;
        knockbackMagnitude *= r*4;

        // Dirección del knockback
        Vector2 knockbackDirection = attack.knockback.normalized;
        knockbackDirection.x *= (attackerOnLeft ? 1 : -1);

        // Aplicar el knockback al Rigidbody
        Vector2 knockbackForce = knockbackDirection * knockbackMagnitude;
        targetRb.AddForce(knockbackForce, ForceMode2D.Impulse);

        // Detener cualquier corutina de knockback activa antes de iniciar una nueva
        if (targetController.activeKnockbackCoroutine != null)
        {
            StopCoroutine(targetController.activeKnockbackCoroutine);
        }

        //Si no tiene hit sound, le ponemos uno por defecto dependiendo del knockbackForce y el daño, tenemos sonidos del 0 al 7, los sonidos los mandaremos por el canal 3 que es el de los golpes
        if (attack.hitSounds.Length == 0)
        {
            Debug.Log("No tiene sonido de golpe, poniendo uno por defecto para el knockbackForce: "+knockbackForce.magnitude);
            if (attackSounds.Count > 0)
            {
                int soundIndex = 6;
                if (knockbackForce.magnitude < 120f)
                {
                    soundIndex = 6;
                }
                else if (knockbackForce.magnitude < 200f)
                {
                    soundIndex = 5;
                }
                else if (knockbackForce.magnitude < 275f)
                {
                    soundIndex = 4;
                }
                else if (knockbackForce.magnitude < 350f)
                {
                    soundIndex = 3;
                }
                else if (knockbackForce.magnitude < 520f)
                {
                    soundIndex = 2;
                }
                else if (knockbackForce.magnitude < 700f)
                {
                    soundIndex = 1;
                }
                else
                {
                    soundIndex = 0;
                }
                targetController.StopSound(3);
                Debug.Log("Poniendo sonido de golpe "+soundIndex);
                targetController.PlaySound(attackSounds[soundIndex],3);
            }
        }
        else
        {
            targetController.StopSound(3);
            targetController.PlaySound(attack.hitSounds,3);
        }


        // Iniciar la nueva corutina de knockback
        targetController.activeKnockbackCoroutine = StartCoroutine(ManageKnockback(targetRb, knockbackForce, targetController));
    }
}

    /// <summary>
    /// Cargar sonidos de una carpeta específica dentro de Resources.
    /// </summary>
    /// <param name="folderPath">Ruta relativa dentro de Resources.</param>
    private void LoadAttackSounds(string folderPath)
    {
        // Cargar todos los sonidos desde la carpeta
        AudioClip[] loadedClips = Resources.LoadAll<AudioClip>(folderPath);
        Debug.Log("Hay "+loadedClips.Length);
        if (loadedClips.Length > 0)
        {
            // Limpiar la lista actual de sonidos
            attackSounds.Clear();

            // Ordenar los clips alfabéticamente por nombre
            System.Array.Sort(loadedClips, (clip1, clip2) => clip1.name.CompareTo(clip2.name));

            // Añadir los clips cargados a la lista
            attackSounds.AddRange(loadedClips);

            Debug.Log($"Se cargaron {attackSounds.Count} sonidos desde la carpeta {folderPath}");
            foreach (var clip in attackSounds)
            {
                Debug.Log(clip.name);
            }
        }
        else
        {
            Debug.LogWarning($"No se encontraron sonidos en la carpeta: {folderPath}");
        }
    }

// Corutina para gestionar el knockback del personaje
private IEnumerator ManageKnockback(Rigidbody2D targetRb, Vector2 initialKnockback, CharacterBaseController targetController)
{
    float horizontalKnockback = initialKnockback.x;
    bool onGround = false;
    int framesOpposingInput = 0; // Frames donde el joystick está en la dirección opuesta
    float opposingInputDecay = 0; // Desaceleración exponencial
    int frames = 0;
    int minFrames = 10;
    if (Mathf.Abs(initialKnockback.x) < 80f && Mathf.Abs(initialKnockback.y) < 100f)
    {
        minFrames = 0;
    }
    bool endKnockback = false;
    Debug.Log("Knockback X: "+horizontalKnockback+" Y: "+initialKnockback.y);
    if (targetController.remainingJumps == 0)
        targetController.remainingJumps = targetController.characterData.maxJumps;
    while (!targetController.invulnerable&&!onGround&&!endKnockback)
    {
        float horizontalInput = Input.GetAxis(targetController.horizontalInput); // Input horizontal del joystick
        float knockbackDirection = Mathf.Sign(horizontalKnockback); // Dirección del knockback inicial

        // Si el jugador está presionando en la dirección opuesta al knockback
        if (horizontalInput != 0 && Mathf.Sign(horizontalInput) != knockbackDirection)
        {
            framesOpposingInput++;
            opposingInputDecay = Mathf.Pow(1.2f, framesOpposingInput); // Aumenta exponencialmente
            float counterForce = opposingInputDecay * 10f * Time.deltaTime; // Ajusta la fuerza con el tiempo
            horizontalKnockback = Mathf.MoveTowards(horizontalKnockback, 0, Mathf.Abs(counterForce));
        }
        else
        {
            // Resetea si el joystick no está en la dirección opuesta
            framesOpposingInput = 0;
            opposingInputDecay = 0;
        }

        // Aplica la velocidad actualizada al Rigidbody
        targetRb.linearVelocity = new Vector2(horizontalKnockback, targetRb.linearVelocity.y);

        // Si el knockback es muy bajo, lo igualamos a 0
        if (Mathf.Abs(horizontalKnockback) < 0.1f)
        {
            horizontalKnockback = 0;
            endKnockback = true;
        }

        // Verificar si el personaje ha tocado el suelo después de un tiempo
        if (frames > minFrames)
        {
            targetController.groundDetector();
            onGround = targetController.isGrounded;
        }
        else
        {
            frames++;
        }

        yield return null;
    }

    // Al tocar el suelo, detener el movimiento horizontal del knockback
    targetRb.linearVelocity = new Vector2(0, targetRb.linearVelocity.y);

    // Limpiar la referencia a la corutina activa al finalizar
    targetController.activeKnockbackCoroutine = null;
}

// Método para limpiar el estado del Rigidbody y detener cualquier knockback
public void ResetCharacterState(Rigidbody2D targetRb)
{
    // Detener la corutina de knockback si está activa
    if (characterController.activeKnockbackCoroutine != null)
    {
        //StopCoroutine(characterController.activeKnockbackCoroutine);
        characterController.activeKnockbackCoroutine = null;
    }

    // Reiniciar las velocidades y otros estados
    if (targetRb != null)
    {
        targetRb.linearVelocity = Vector2.zero;
        targetRb.angularVelocity = 0f;
    }

    Debug.Log("Estado del personaje reseteado.");
}


    private void ShootProjectile(AttackData attack)
    {
        if (attack.projectilePrefab == null)
        {
            Debug.LogWarning("No se ha asignado un prefab de proyectil.");
            return;
        }

        // Verificar el máximo de proyectiles activos
        int currentProjectiles = GameObject.FindGameObjectsWithTag("Projectile").Length;
        if (currentProjectiles >= attack.maxProjectiles)
        {
            Debug.LogWarning("Número máximo de proyectiles activos alcanzado.");
            return;
        }

        // Crear el proyectil
        GameObject projectileInstance = Instantiate(
            attack.projectilePrefab,
            transform.position,
            Quaternion.identity
        );

        // Inicializar el proyectil
        Projectile projectileScript = projectileInstance.GetComponent<Projectile>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(gameObject, attack, this);
        }
        else
        {
            Debug.LogError("El prefab del proyectil no tiene el script Projectile.");
        }
    }

    private void OnDrawGizmos()
    {
        // Dibujar la hitbox en el editor para depuración
        if (characterController == null || characterController.characterData == null || !characterController.createHitboxes) return;

        Gizmos.color = Color.red;
        foreach (var attack in characterController.characterData.attacks)
        {
            if (attack == null) continue;
            if (characterController.facingRight){
                Vector3 hitboxWorldPosition = new Vector3(transform.position.x + attack.hitboxPosition.x, transform.position.y + attack.hitboxPosition.y, 225);
                Gizmos.DrawWireCube(hitboxWorldPosition, (Vector3)attack.hitboxSize);
            }
            else{
                Vector3 hitboxWorldPosition = new Vector3(transform.position.x - attack.hitboxPosition.x, transform.position.y + attack.hitboxPosition.y, 225);
                Gizmos.DrawWireCube(hitboxWorldPosition, (Vector3)attack.hitboxSize);
            }
        }
    }
}
