using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerButtonActivator : MonoBehaviour
{
    private Button button; // Referencia al botón en el objeto
    private GameObject currentPlayer; // El jugador que está encima del botón

    private string fire1Button = "Fire1";

    private bool isPlayerOverButton = false; // Para controlar el estado visual

    void Start()
    {
        // Obtener la referencia del botón en este objeto
        button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError($"No Button component found on {gameObject.name}!");
        }
    }

    void Update()
    {
        if (currentPlayer != null && button != null)
        {
            // Obtener el índice del jugador basado en el tag "PlayerX"
            int playerIndex = GetPlayerIndex(currentPlayer);

            if (playerIndex >= 0)
            {
                // Nombre del botón Fire1 correspondiente al jugador
                if (playerIndex == 1)
                {
                    fire1Button = "Fire1";
                }
                else
                {
                    fire1Button = $"Fire1_{playerIndex}";
                }

                // Comprobar si se presiona Fire1
                if (Input.GetButtonDown(fire1Button))
                {
                    button.onClick.Invoke(); // Llamar al evento onClick
                }
            }
        }

        // Simular estado de hover visual si el jugador está encima del botón
        if (button != null && isPlayerOverButton)
        {
            var colors = button.colors;
            colors.normalColor = colors.highlightedColor; // Cambia al color de "Highlighted"
            button.colors = colors;
        }
    }

    private int GetPlayerIndex(GameObject player)
    {
        // Extraer el índice del jugador del tag (asumiendo "PlayerX" como formato del tag)
        if (player.CompareTag("Player"))
        {
            string tag = player.transform.Find("Text TMP").GetComponent<TextMeshProUGUI>().text;
            tag = tag.Replace("P", "Player");
            if (tag.Length > 6 && int.TryParse(tag.Substring(6), out int index))
            {
                return index; // Devuelve el índice del jugador
            }
        }

        return -1; // Si no se puede determinar el índice

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectar si el objeto que entra tiene el tag "Player"
        if (collision.CompareTag("Player"))
        {
            currentPlayer = collision.gameObject;
            isPlayerOverButton = true; // Activar estado de hover
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Detectar si el objeto que sale tiene el tag "Player"
        if (collision.gameObject == currentPlayer)
        {
            currentPlayer = null;
            isPlayerOverButton = false; // Desactivar estado de hover

            // Restaurar los colores originales del botón
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = Color.white; // Cambia al color por defecto (o ajusta según corresponda)
                button.colors = colors;
            }
        }
    }
}
