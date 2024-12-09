using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CharacterBaseController : MonoBehaviour
{
    [HideInInspector]
    public bool started=false;
    [HideInInspector]
    public bool freezing = false; // Indica si el personaje está congelado
    public int playerID = 1; // Identificador del jugador
    public int stocks = 3; // Vidas del personaje
    public float porcentajeVida = 0;
    public bool debugMode = false; // Modo de depuración
    public bool createHitboxes = false; // Crear hitboxes
    public CharacterData characterData; // Datos del personaje
    private Animator animator; // Controlador de animaciones
    private Rigidbody2D rb; // Componente de física 2D
    public AudioSource audioSourceChannel1; // Canal 1 Voces, 
    public AudioSource audioSourceChannel2; // Canal 2 pasos
    public AudioSource audioSourceChannel3; // Canal 3 golpes recibidos

    //Añadimos variable para el spawnpoint
    public Vector3 spawnpoint;
    public bool initialFacingRight = true;
    private bool isPlayingChannel1 = false;
    private bool isPlayingChannel2 = false;
    private bool isPlayingChannel3 = false;
    [HideInInspector]
    public int remainingJumps; // Saltos restantes
    private int lastRemainingJumps; // Saltos restantes del frame anterior
    public float zLayer = 225f;
    [HideInInspector]
    public bool invulnerable =false; // Indica si el personaje es invulnerable
    private bool isAttacking = false; // Indica si el personaje está atacando
    private bool lastIsAttacking = false; // Indica si el personaje estaba atacando en el frame anterior
    [HideInInspector]
    private bool hitstun = false; // Indica si el personaje está en hitstun
    //Controles
    [HideInInspector]
    public string horizontalInput;
    [HideInInspector]
    public string verticalInput;
    private string jumpInput;
    private string crouchInput;
    private string attack1Input;
    private string attack2Input;
    public GameObject playerMarker; 

    // Variables de estado
    [HideInInspector]
    public bool isGrounded; // Indica si el personaje está en el suelo
    [HideInInspector]
    public bool firstFrameRespawn=false; // Indica si el personaje está en el suelo
    [HideInInspector]
    public Coroutine activeKnockbackCoroutine=null; // Indica si el personaje tiene knockback activo
    [HideInInspector]
    public bool jumpedFrame=false; // Indica si el personaje tiene knockback activo

    private bool isPlaying; // Controla la reproducción de audio
    public bool facingRight = true; // Indica si el personaje está mirando a la derecha
    private bool isCrouching = false; // Indica si el personaje está agachado
    private AttackHandler attackHandler;

    public Transform groundCheck; // Objeto para verificar si el personaje está en el suelo
    public LayerMask groundLayer; // Capa del suelo
    private float speedJump; // Velocidad de salto

    private float lastJumpTime = 0f; // Tiempo del último salto
    private float jumpCooldown = 0.1f; // Tiempo mínimo entre saltos en segundos (100 milisegundos)

    public List<Color> playerColors = new List<Color>()
    {
        new Color(1f, 0f, 0f),        // Rojo (P1)
        new Color(0f, 0.4f, 1f),      // Azul (P2)
        new Color(1f, 0.8f, 0f),      // Amarillo (P3)
        new Color(0f, 0.8f, 0.2f),    // Verde (P4)
        new Color(1f, 0.4f, 0f),      // Naranja (P5)
        new Color(0f, 1f, 1f),        // Azul celeste (P6)
        new Color(1f, 0.4f, 0.7f),    // Rosado (P7)
        new Color(0.5f, 0f, 1f)       // Morado oscuro (P8)
    };

    private void Start()
    {
        while (!started)
        {
            // Esperar a que el juego inicie
        }
        AssignControls(playerID); // Asignar controles según el jugador
        playerMarker.GetComponent<SpriteRenderer>().color = playerColors[playerID - 1]; // Asignar color al marcador del jugador

        animator = GetComponent<Animator>();
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length > 1)
        {
            audioSourceChannel1 = audioSources[0];
            audioSourceChannel2 = audioSources[1];
            audioSourceChannel3 = audioSources[2];
        }

        remainingJumps = characterData.maxJumps;
        lastRemainingJumps = characterData.maxJumps;
        
        // Configurar las animaciones
        setGravity();
        attackHandler = gameObject.AddComponent<AttackHandler>();
        attackHandler.Initialize(this, animator, rb);
        
        respawn();

    }
    public void StartCharacter()
    {
        started=true;
    }

    private void Update()
    {
        if (freezing)
        {
            return;
        }
        directionManager(); // Control de la dirección del personaje
        HandleAttacks();
        AssignControls(playerID); // Asignar controles según el jugador
        groundDetector();
        ControladorBasico(); // Control del movimiento básico
        lastIsAttacking= isAttacking;
        firstFrameRespawn=false;
        jumpedFrame=false;
    }

    private void FixedUpdate()
    {
        //Debug.Log("Saltos restantes: " + remainingJumps);
        lastRemainingJumps = remainingJumps;
    }

        private void AssignControls(int playerID)
    {
        // Configurar los nombres de los inputs según el jugador
        if (playerID == 0)
        {
            horizontalInput = "";
            verticalInput = "";
            jumpInput = "";
            crouchInput = "";
            attack1Input = "";
            attack2Input = "";
        }
        else if (playerID == 1)
        {
            horizontalInput = "Horizontal1";
            verticalInput = "Vertical1";
            jumpInput = "Jump1";
            crouchInput = "Crouch1";
            attack1Input = "Fire1";
            attack2Input = "Fire2";
        }
        else
        {
            horizontalInput = "Horizontal"+playerID;
            verticalInput = "Vertical"+playerID;
            jumpInput = "Jump"+playerID;
            crouchInput = "Crouch"+playerID;
            attack1Input = "Fire1_"+playerID;
            attack2Input = "Fire2_"+playerID;
        }
    }


public bool groundDetectorEx()
{
    bool wasGrounded = isGrounded;
    isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

    // Resetea los saltos si el personaje toca el suelo
    if (isGrounded && !wasGrounded)
    {
        remainingJumps = characterData.maxJumps;
        if (debugMode)
            Debug.Log("Está en el suelo");
    }
    return isGrounded;
}

public bool groundDetector()
{
    bool wasGrounded = isGrounded;

    // Longitud del raycast
    float rayLength = 0.1f;

    // Posiciones desde donde lanzar los raycasts
    Vector2 leftRayOrigin = new Vector2(groundCheck.position.x - 0.1f, groundCheck.position.y);
    Vector2 rightRayOrigin = new Vector2(groundCheck.position.x + 0.1f, groundCheck.position.y);

    // Lanzar raycasts hacia abajo
    RaycastHit2D leftRayHit = Physics2D.Raycast(leftRayOrigin, Vector2.down, rayLength);
    RaycastHit2D rightRayHit = Physics2D.Raycast(rightRayOrigin, Vector2.down, rayLength);

    // Verificar si el objeto detectado tiene el tag "Ground"
    bool leftHitGround = leftRayHit.collider != null && leftRayHit.collider.CompareTag("Ground");
    bool rightHitGround = rightRayHit.collider != null && rightRayHit.collider.CompareTag("Ground");

    // Determinar si alguno de los raycasts toca el suelo con el tag "Ground"
    isGrounded = leftHitGround || rightHitGround;

    // Si toca el suelo y antes no lo estaba, resetea los saltos
    if (isGrounded && !wasGrounded)
    {
        remainingJumps = characterData.maxJumps;

        if (debugMode)
        {
            Debug.Log("Está en el suelo");
        }
    }

    return isGrounded;
}


    // Se ocupa del movimiento basico del personaje, no se incluyen ataques
    private void ControladorBasico()
        {
            float adjustedSpeed = characterData.speed;
            float moveInput = Input.GetAxis(horizontalInput);
            
            jumpedFrame=saltar();
            if (!isCrouching)
            {
                //Debug.Log("Is grounded: "+isGrounded);
                rb.linearVelocity = new Vector2(moveInput * adjustedSpeed, rb.linearVelocity.y);
                if (isGrounded && moveInput != 0 && !isAttacking)
                {
                    //Debug.Log(Mathf.Abs(moveInput));
                    if (Mathf.Abs(moveInput) == 1f)
                    {
                        rb.linearVelocity = new Vector2(moveInput * adjustedSpeed * 1.5f, rb.linearVelocity.y);
                        PlayAnimation("running");
                        PlaySound(characterData.runSounds, characterData.runSoundDelay, 2);
                    }
                    else if (Mathf.Abs(moveInput) > 0.1f)
                    {
                        PlayAnimation("walk");
                        PlaySound(characterData.walkSounds, characterData.walkSoundDelay, 2);
                    }
                }
                else if (isGrounded && !isAttacking)
                {
                    PlayAnimation("idle");
                }
                else if (!isGrounded && !isAttacking && lastIsAttacking)
                {
                    //ponemos el ultimo frame de la animacion de salto
                    animator.Play("jump", -1, 0.7f);                    
                }
            }

            agacharse();
        }



        private void HandleAttacks()
        {
            if (Input.GetButtonDown(attack1Input))
            {
                if (debugMode)
                    Debug.Log("Ataque 1");
                attackHandler.HandleAttack(0); // Ataque 1
            } //side B
            else if (Input.GetButtonDown(attack2Input)&& Mathf.Abs(Input.GetAxis(horizontalInput)) > 0.6)
            {
                attackHandler.HandleAttack(2); // Ataque 2
            }
            else if (Input.GetButtonDown(attack2Input))
            {
                attackHandler.HandleAttack(1); // Ataque 2
            }
            isAttacking = attackHandler.isAttacking;
            
        }

        public void PerformAttack(int attackIndex)
        {
            if (attackIndex < 0 || attackIndex >= characterData.attacks.Length) return;

            var attack = characterData.attacks[attackIndex];
            animator.Play(attack.attackAnimation.name);
            PlaySound(attack.attackSounds, 2);
        }

    private bool saltar()
    {
        if (Input.GetButtonDown(jumpInput) && (remainingJumps > 0) && (Time.time - lastJumpTime >= jumpCooldown))
        {
            StopSound(1);
            PlaySound(characterData.jumpSounds,1);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, speedJump);

            isGrounded = false;
            remainingJumps--;
            lastJumpTime = Time.time;
            if (!isAttacking){
            animator.Play("jump", 0, 0f);}
            return true;
        }
        return false;
    }


    private void agacharse()
    {   
        // Detectar si el joystick se mueve hacia abajo
        float crouchInput = Input.GetAxis($"Vertical{playerID}"); // Usa el ID del jugador para distinguir controles

        if (crouchInput < -0.5f && isGrounded) // Verifica si el eje vertical es mayor que 0.5 (hacia abajo)
        {
            isCrouching = true;
            PlayAnimation("crouch");
            PlaySound(characterData.crouchSounds, 2);
        }
        else
        {
            isCrouching = false;
        }
    }



        private void PlayAnimation(AnimationClip clip)
        {
            animator.StopPlayback();
            if (clip != null)
            {
                animator.Play(clip.name);
            }
        }

        public void PlayAnimation(string clip)
        {
            animator.StopPlayback();
            if (clip != null)
            {
                animator.Play(clip);
            }
        }

        //Corutina que hace que el personaje parpadee de color rojo cuando recibe daño
        private IEnumerator DamageAnimation()
        {
            float elapsedTime = 0f;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("No se encontró un SpriteRenderer en el objeto.");
                yield break;
            }

            while (elapsedTime < 0.4f)
            {
                spriteRenderer.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
                elapsedTime += 0.2f;
            }
        }

        public void hitted()
        {
            StartCoroutine(DamageAnimation());
        }

        private void RestartAnimation(AnimationClip clip)
        {
            // Paramos todas las animaciones
            animator.StopPlayback();
            if (clip != null)
            {
                animator.Play(clip.name,0,0f);
            }
        }

        private void StopAnimation()
        {
            animator.StopPlayback();
        }


        private void PlaySound(AudioClip[] clips, float delay, int channel = 1)
        {
            if (clips != null && clips.Length > 0)
            {
                AudioClip selectedClip = characterData.GetRandomAudioClip(clips);
                if (selectedClip != null)
                {
                    if (channel == 1 && !isPlayingChannel1)
                    {
                        audioSourceChannel1.PlayOneShot(selectedClip);
                        isPlayingChannel1 = true;
                        StartCoroutine(ResetIsPlaying(selectedClip.length + delay, 1));
                    }
                    else if (channel == 2 && !isPlayingChannel2)
                    {
                        audioSourceChannel2.PlayOneShot(selectedClip);
                        isPlayingChannel2 = true;
                        StartCoroutine(ResetIsPlaying(selectedClip.length + delay, 2));
                    }
                }
            }
        }

        public void PlaySound(AudioClip[] clips, int channel = 1)
        {
            if (clips != null && clips.Length > 0)
            {
                AudioClip selectedClip = characterData.GetRandomAudioClip(clips);
                if (selectedClip != null)
                {
                    if (channel == 1 && !isPlayingChannel1)
                    {
                        audioSourceChannel1.PlayOneShot(selectedClip);
                        isPlayingChannel1 = true;
                        StartCoroutine(ResetIsPlaying(selectedClip.length, 1));
                    }
                    else if (channel == 2 && !isPlayingChannel2)
                    {
                        audioSourceChannel2.PlayOneShot(selectedClip);
                        isPlayingChannel2 = true;
                        StartCoroutine(ResetIsPlaying(selectedClip.length, 2));
                    }
                    else if (channel == 3 && !isPlayingChannel3)
                    {
                        audioSourceChannel3.PlayOneShot(selectedClip);
                        isPlayingChannel3 = true;
                        StartCoroutine(ResetIsPlaying(selectedClip.length, 3));
                    }
                }
            }
        }
        
        public void PlaySound(AudioClip clip, int channel = 1)
        {
            if (channel == 1 && !isPlayingChannel1)
            {
                audioSourceChannel1.PlayOneShot(clip);
                isPlayingChannel1 = true;
                StartCoroutine(ResetIsPlaying(clip.length, 1));
            }
            else if (channel == 2 && !isPlayingChannel2)
            {
                audioSourceChannel2.PlayOneShot(clip);
                isPlayingChannel2 = true;
                StartCoroutine(ResetIsPlaying(clip.length, 2));
            }
            else if (channel == 3 && !isPlayingChannel3)
            {
                audioSourceChannel3.PlayOneShot(clip);
                isPlayingChannel3 = true;
                StartCoroutine(ResetIsPlaying(clip.length, 3));
            }
                
        }

        private System.Collections.IEnumerator ResetIsPlaying(float delay, int channel)
        {
            yield return new WaitForSeconds(delay);
            if (channel == 1)
            {
                isPlayingChannel1 = false;
            }
            else if (channel == 2)
            {
                isPlayingChannel2 = false;
            }
            else if (channel == 3)
            {
                isPlayingChannel3 = false;
            }
        }

        private void StopSound(AudioClip clip)
        {
            if (clip != null && isPlaying && audioSourceChannel1.clip == clip)
            {
                audioSourceChannel1.Stop();
                isPlaying = false;
            }
        }

        public void StopSound(int channel = 1)
        {
            if (channel == 1 && isPlayingChannel1)
            {
                audioSourceChannel1.Stop();
                isPlayingChannel1 = false;
            }
            else if (channel == 2 && isPlayingChannel2)
            {
                audioSourceChannel2.Stop();
                isPlayingChannel2 = false;
            }
            else if (channel == 3 && isPlayingChannel3)
            {
                audioSourceChannel3.Stop();
                isPlayingChannel3 = false;
            }
        }


        
        // Configurar la gravedad del personaje dependiendo del peso
        private void setGravity(){        
            rb = GetComponent<Rigidbody2D>();
            // Vamos a configurar la gravedad del personaje en 3 rangos de peso, de 0 a 30, de 30 a 70 y de 70 a 100, Ligero, Medio y Pesado respectivamente
            if (characterData.weight < 30)
            {
                speedJump= 300;
                rb.gravityScale = 40;
            }
            else if (characterData.weight < 70)
            {
                speedJump= 290;
                rb.gravityScale = 50;
            }
            else
            {
                speedJump= 280;
                rb.gravityScale = 60;
            }

        }

        // Control de la dirección del personaje
        private void directionManager()
        {
            // Usa el eje correspondiente al jugador según el ID
            float moveInput = Input.GetAxis($"Horizontal{playerID}");

            if (moveInput > 0)
            {
                FaceToRight();
            }
            else if (moveInput < 0)
            {
                FaceToLeft();
            }
        }
        private void Flip()
        {
            facingRight = !facingRight;
            Vector3 theScale = transform.localScale;
            theScale.x *= -1; // Invierte la escala X
            transform.localScale = theScale;
        }
        private void FaceToLeft()
        {
            facingRight = false;
            Vector3 theScale = transform.localScale;
            if (theScale.x > 0)
            {
                theScale.x *= -1; // Invierte la escala X
            }
            transform.localScale = theScale;
        }
        private void FaceToRight()
        {
            facingRight = true;
            Vector3 theScale = transform.localScale;
            if (theScale.x < 0)
            {
                theScale.x *= -1; // Invierte la escala X
            }
            transform.localScale = theScale;
        }


            private void ShootProjectile(AttackData attack)
        {
            if (attack.projectilePrefab == null) return;

            // Determinar dirección en función de la orientación del personaje
            Vector2 direction = facingRight ? Vector2.right : Vector2.left;

            // Crear el proyectil en la posición del ataque
            GameObject projectile = Instantiate(
                attack.projectilePrefab, 
                transform.position + (Vector3)attack.hitboxPosition, 
                Quaternion.identity
            );
        }

        private void Die()
        {
            // Manejar la muerte del personaje (puedes deshabilitar controles o activar animaciones)
            Debug.Log($"{characterData.characterName} está fuera de combate.");
            gameObject.SetActive(false); // Desactivar al personaje
        }

        public void Eliminate()
        {
            stocks--;
            if (stocks <= 0)
            {
                Die();
            }
            else
            {
                // Respawn del personaje
                if (debugMode)
                    Debug.Log($"{characterData.characterName} ha perdido una vida.");
                respawn();
            }
        }

        private void respawn()
        {   
            attackHandler.ResetCharacterState(rb);
            isAttacking=false;
            attackHandler.isAttacking=false;
            if (debugMode)
                Debug.Log($"{characterData.characterName} ha vuelto al combate.");
            gameObject.SetActive(false); // Reactivar al personaje
            porcentajeVida = 0;
            // Ponemos en posicion del spawn point y mirando donde toca
            transform.position = spawnpoint;
            if (initialFacingRight)
            {
                if (!facingRight)
                {
                    Flip();
                }
            }
            else
            {
                if (facingRight)
                {
                    Flip();
                }
            }
            gameObject.SetActive(true); // Reactivar al personaje
            StartCoroutine(AnimacionRespawn());
            StartCoroutine(temporalInvulnerability(3,"Spawn"));
            // Hacemos que el personaje pase de medir 0 de tamaño a lo que tiene de normal
            //TODO: Hacer que el personaje pase de medir 0 de tamaño a lo que tiene de normal
        }
        
        private System.Collections.IEnumerator temporalInvulnerability(int seconds, string reason)
        {
            // Hacer al personaje invulnerable
            float elapsedTime = 0f;
            invulnerable = true;
            bool canceled = false;

            // Obtiene el SpriteRenderer para controlar la visibilidad
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("No se encontró un SpriteRenderer en el objeto.");
                yield break;
            }

            while (elapsedTime < seconds && !canceled)
            {
                // Alternar visibilidad para hacer que parpadee
                spriteRenderer.enabled = !spriteRenderer.enabled; // Alterna entre visible/invisible

                // Esperar un breve intervalo antes de cambiar la visibilidad nuevamente
                yield return new WaitForSeconds(0.1f);

                elapsedTime += 0.1f;
                if (reason == "Spawn")
                {   // si se mueve o ataca se cancela la invulnerabilidad
                    if (Input.GetAxis(horizontalInput) != 0 || Input.GetAxis(verticalInput) != 0 || Input.GetButtonDown(attack1Input) || Input.GetButtonDown(attack2Input))
                    {
                        canceled = true;
                    }
                    else{
                        if (playerID == 1)
                        {
                            // Flipeamos para que mire a la izquierda
                            transform.position = new Vector3(25, 113.582f, zLayer);
                            if (!facingRight)
                            {
                                Flip();
                            }
                        }
                        else if (playerID == 2)
                        {
                            // Flipeamos para que mire a la izquierda
                            transform.position = new Vector3(300, 113.582f, zLayer);
                            if (facingRight)
                            {
                                Flip();
                            }
                        }
                        else{
                            if (playerID!=0)
                            {
                                transform.position = new Vector3(162, 113.582f, zLayer);
                            }
                        }
                    }
                }
            }

            // Asegurar que el sprite sea visible al terminar
            spriteRenderer.enabled = true;

            // Quitar la invulnerabilidad
            invulnerable = false;
        }



        private System.Collections.IEnumerator AnimacionRespawn()
        {
            freezing = true;
            Vector3 initialScale = transform.localScale;
            float elapsedTime = 0f;

            while (elapsedTime < 0.8f)
            {
                // Interpolación lineal del tamaño del personaje
                transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, elapsedTime);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // Asegurar que la escala final sea exacta
            transform.localScale = initialScale;
            freezing = false;
        }


        private bool pushingPlayer = false; // ¿Estamos empujando a otro jugador?
private float pushTime = 0f; // Tiempo que hemos estado empujando
private float pushThreshold = 0.2f; // Tiempo necesario para atravesar al jugador
private List<GameObject> ignoredCollisions = new List<GameObject>();

private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player") && collision.gameObject != this.gameObject)
    {
        pushingPlayer = true;
        pushTime = 0f; // Reinicia el contador de empuje
    }
}

private void OnCollisionStay2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player") && collision.gameObject != this.gameObject)
    {
        Rigidbody2D otherRb = collision.gameObject.GetComponent<Rigidbody2D>();
        Collider2D otherCollider =  collision.gameObject.GetComponent<CapsuleCollider2D>();
        Collider2D myCollider = GetComponent<Collider2D>(); // Mi collider principal

        if (otherRb != null && otherCollider != null && myCollider != null)
        {
            float moveInput = Input.GetAxis(horizontalInput);
            float pushForce = moveInput * characterData.speed;

            if (Mathf.Abs(moveInput) > 0.1f)
            {
                bool onGround = groundDetector();
                // Aplica empuje proporcional al estado del jugador
                //Debug.Log("isGrounded: "+onGround+" moveInput: "+moveInput);
                if (Mathf.Abs(moveInput) > 0.8f&&onGround) // Corriendo
                {
                    if (pushTime > pushThreshold)
                    {
                        //Debug.Log("Atravesando al jugador "+collision.gameObject.name);
                        Physics2D.IgnoreCollision(otherCollider, myCollider, true); // Ignorar colisiones
                        if (!ignoredCollisions.Contains(collision.gameObject))
                        {
                            ignoredCollisions.Add(collision.gameObject); // Añadir a la lista de colisiones ignoradas
                            StartCoroutine(RestoreCollisionAfterTime(otherCollider, 0.4f, collision.gameObject)); // Restaurar colisiones
                        }
                        pushingPlayer = false;
                    }
                    else
                    {
                        otherRb.AddForce(new Vector2(pushForce*0.2f, 0f), ForceMode2D.Force);
                    }
                }
                else // Caminando
                {
                    otherRb.AddForce(new Vector2(pushForce * 0.2f, 0f), ForceMode2D.Force);
                }

                //Debug.Log("pushTime: " + pushTime);
                pushTime += Time.deltaTime; // Incrementar tiempo de empuje
            }
        }
    }
}

private void OnCollisionExit2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Player") && collision.gameObject != this.gameObject)
    {
        Collider2D otherCollider = collision.collider;
        Collider2D myCollider = GetComponent<CapsuleCollider2D>();

        if (ignoredCollisions.Contains(collision.gameObject))
        {
            //Debug.Log("Colisión ignorada, no se restaura.");
        }
        else{
            //Debug.Log("Restaurando colisiones con el jugador "+collision.gameObject.name);
            pushingPlayer = false;
            pushTime = 0f;
        }
    }
}
        
        private IEnumerator RestoreCollisionAfterTime(Collider2D otherCollider, float delay, GameObject otherPlayer)
        {
            yield return new WaitForSeconds(delay);
            Physics2D.IgnoreCollision(otherCollider, GetComponent<Collider2D>(), false);
            //Debug.LogError("Restaurando colisiones con el jugador después de " + delay + " segundos.");
            ignoredCollisions.Remove(otherPlayer);
        }



    }

