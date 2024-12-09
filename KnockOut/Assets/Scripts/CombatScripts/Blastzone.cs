using UnityEngine;

public class Blastzone : MonoBehaviour
{
    [SerializeField] private Camera mainCamera; // Referencia a la cámara principal
    [SerializeField] private float shakeDuration = 0.5f; // Duración del efecto de sacudida
    [SerializeField] private float shakeMagnitude = 0.3f; // Intensidad de la sacudida

    // Método que se ejecuta al salir de la zona
    private void OnTriggerExit2D(Collider2D other)
    {
        // Verifica si el objeto que sale es un personaje
        if (other.CompareTag("Player"))
        {
            // Obtiene la posición del personaje al salir
            Vector2 exitPosition = other.transform.position;

            // Obtiene los límites del BoxCollider2D (Blastzone)
            BoxCollider2D blastZoneCollider = GetComponent<BoxCollider2D>();
            Bounds bounds = blastZoneCollider.bounds;

            // Verifica si el personaje salió por arriba, izquierda o derecha
            if (exitPosition.y > bounds.min.y) // Si no salió por abajo
            {
                Debug.Log($"{other.name} ha salido por los lados o arriba de la blastzone!");
            }
            StartCoroutine(ShakeCamera()); // Inicia el efecto de sacudida

            // Llama a un método de eliminación en el script del personaje
            CharacterBaseController player = other.GetComponent<CharacterBaseController>();
            if (player != null)
            {
                player.Eliminate();
            }
        }
        if (other.CompareTag("Projectile"))
        {
            Destroy(other.gameObject);
        }
    }

    // Corutina para agitar la cámara
    private System.Collections.IEnumerator ShakeCamera()
    {
        Vector3 originalPosition = mainCamera.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
            float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);
            mainCamera.transform.position = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Restaurar la posición original de la cámara
        mainCamera.transform.position = originalPosition;
    }
}
