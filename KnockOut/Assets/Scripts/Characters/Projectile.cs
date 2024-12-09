using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private AttackData attack;
    private float lifetime;
    private AttackHandler attackHandler; // Referencia al manejador de ataques
    private GameObject owner; // Quién disparó el proyectil

    // Inicializar el proyectil con los datos necesarios
    public void Initialize(GameObject owner, AttackData attack, AttackHandler handler)
    {
        this.owner = owner;
        this.attack =attack;
        this.lifetime = attack.projectileLifetime;
        this.attackHandler = handler;
        int inverter=1;

        // Convertir ángulo a dirección
        float radians = attack.projectileAngle * Mathf.Deg2Rad;
        if (!handler.characterController.facingRight)
        {
            Vector3 theScale = transform.localScale;
            theScale.x *= -1; // Invierte la escala X
            transform.localScale = theScale;
            inverter = -1;
        }
        Debug.Log("Inverteer: "+inverter);
        Vector2 direction = new Vector2(Mathf.Cos(radians)*inverter, Mathf.Sin(radians));

        // Aplicar velocidad inicial
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * attack.projectileSpeed*5000*Time.deltaTime;
        }

        // Destruir automáticamente tras su tiempo de vida
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar colisiones con el dueño del proyectil
        if (collision.gameObject == owner) return;

        // Verificar si es un objetivo válido
        CharacterBaseController target = collision.GetComponent<CharacterBaseController>();
        if (target != null && target.playerID != owner.GetComponent<CharacterBaseController>().playerID)
        {
            // Aplicar daño y knockback al objetivo
            attackHandler.ApplyDamage(collision.gameObject, attack);

            // Destruir el proyectil al impactar
            Destroy(gameObject);
        }
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}
