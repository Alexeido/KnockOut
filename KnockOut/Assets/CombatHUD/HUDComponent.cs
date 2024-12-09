using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; // Necesario para usar listas

public class HUDComponent : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI characterNameText;
    public Image characterIconImage;
    public TextMeshProUGUI characterHealthText;
    public Image backgroundBarImage;
    public Image backsquare;

    [Header("Stock (Lives) Settings")]
    [Tooltip("Prefab del corazón que representa una vida.")]
    public GameObject heartPrefab; // Prefab del corazón
    [Tooltip("Contenedor donde se mostrarán los corazones.")]
    public Transform heartsContainer; // Contenedor para los corazones

    private List<GameObject> hearts = new List<GameObject>(); // Lista para almacenar los corazones actuales

    private float lastPercentage = 0f; // Para guardar el último porcentaje de vida
    private Coroutine shakeCoroutine; // Referencia a la corutina activa de agitación
    private Vector3 originalPosition; // Posición original del texto de vida

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

    public void InitializeHUD(CharacterBaseController character)
    {
        // Configurar valores iniciales
        characterNameText.text = character.characterData.characterName;
        characterIconImage.sprite = character.characterData.characterIcon;

        backgroundBarImage.color = playerColors[character.playerID - 1]*0.7f;
        backsquare.color = playerColors[character.playerID - 1]*0.7f;

        // Inicializar los corazones según los stocks iniciales
        UpdateHearts(character.stocks);
        originalPosition = characterHealthText.rectTransform.localPosition;
    }

    public void UpdateHUDValues(CharacterBaseController character)
    {
        // Actualizar el porcentaje de vida
        float porcentaje = Mathf.Round(character.porcentajeVida * 100f) / 100f;

        // Comprobar si ha subido el porcentaje
        if (porcentaje > lastPercentage)
        {
            float incremento = Mathf.Abs(porcentaje - lastPercentage);

            // Iniciar agitación según el incremento
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(ShakeText(incremento));
        }

        // Guardar el nuevo porcentaje
        lastPercentage = porcentaje;

        // Separar la parte entera y decimal
        int parteEntera = Mathf.FloorToInt(porcentaje);
        int parteDecimal = Mathf.FloorToInt((porcentaje - parteEntera) * 100);

        // Formatear el texto con Rich Text para reducir el tamaño de los decimales
        characterHealthText.text = $"{parteEntera}.<size=-20>{parteDecimal}%</size>";

        // Cambiar el color basado en el porcentaje de vida
        if (character.porcentajeVida <= 0)
        {
            characterHealthText.color = Color.white; // 0%
        }
        else if (character.porcentajeVida <= 20)
        {
            characterHealthText.color = Color.Lerp(Color.white, new Color(1f, 0.95f, 0.4f), character.porcentajeVida / 20f);
        }
        else if (character.porcentajeVida <= 60)
        {
            characterHealthText.color = Color.Lerp(new Color(1f, 0.95f, 0.4f), new Color(1f, 0.5f, 0f), (character.porcentajeVida - 20f) / 40f);
        }
        else if (character.porcentajeVida <= 100)
        {
            characterHealthText.color = Color.Lerp(new Color(1f, 0.5f, 0f), new Color(1f, 0.2f, 0.2f), (character.porcentajeVida - 60f) / 40f);
        }
        else if (character.porcentajeVida <= 200)
        {
            characterHealthText.color = Color.Lerp(new Color(1f, 0.2f, 0.2f), new Color(0.6f, 0f, 0f), (character.porcentajeVida - 100f) / 100f);
        }
        else
        {
            characterHealthText.color = new Color(0.6f, 0f, 0f);
        }

        // Actualizar corazones si los stocks cambian
        UpdateHearts(character.stocks);
    }

    private void UpdateHearts(int stocks)
    {
        // Añadir corazones si el número de stocks es mayor que el número actual
        while (hearts.Count < stocks)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartsContainer);
            hearts.Add(newHeart);
        }

        // Eliminar corazones si el número de stocks es menor que el número actual
        while (hearts.Count > stocks)
        {
            GameObject lastHeart = hearts[hearts.Count - 1];
            hearts.RemoveAt(hearts.Count - 1);
            Destroy(lastHeart);
        }
    }

    private IEnumerator ShakeText(float intensity)
    {
        float baseDuration = 0.5f;
        float duration = baseDuration + Mathf.Clamp((intensity - 20f) * 0.02f, 0f, 1f);
        duration = Mathf.Max(duration, baseDuration);

        float elapsed = 0f;

        float shakeMagnitude = Mathf.Clamp(intensity * 2f, 5f, 4f);


        while (elapsed < duration)
        {
            float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
            float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);

            characterHealthText.rectTransform.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        characterHealthText.rectTransform.localPosition = originalPosition;
    }
}
