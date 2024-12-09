using UnityEngine;
using UnityEngine.UI;

public class MenuSelector : MonoBehaviour
{
    private Collider2D objetoDentro; // Referencia al objeto detectado con el tag GameController
    private bool isPlayerOverButton=false; // Indica si el jugador está encima del botón
    private Button button; // Referencia al botón en el objeto

    // Se llama cuando un objeto entra en el trigger del collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            objetoDentro = other; // Guarda la referencia al objeto
            isPlayerOverButton = true; // Activar estado de hover
            Debug.Log("DENTRO");
            Debug.Log($"{objetoDentro.name}");
        }

    }

    // Se llama cuando un objeto sale del trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {   
            isPlayerOverButton = false; // Desactivar estado de hover
            // Restaurar los colores originales del botón
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = Color.white; // Cambia al color por defecto (o ajusta según corresponda)
                button.colors = colors;
            }
            Debug.Log("FUERA");
            objetoDentro = null; // Elimina la referencia al objeto
        }
    }

    // Se llama en cada frame para verificar la pulsación del botón
    private void Update()
    {
        // Si presiona el botón Fire1 (por defecto, clic izquierdo o Ctrl izquierdo)
        if (Input.GetButtonDown("Fire1") && objetoDentro != null)
        {
            Debug.Log($"{objetoDentro.name}");
            if (button != null)
            {
                button.onClick.Invoke();
            }  
            
        }
        // Simular estado de hover visual si el jugador está encima del botón
        if (isPlayerOverButton)
        {
            var colors = button.colors;
            colors.normalColor = colors.highlightedColor; // Cambia al color de "Highlighted"
            button.colors = colors;
        }
    }
    private void Start()
    {
        button = GetComponent<Button>(); // Obtener el componente Button en el objeto
    }
}
