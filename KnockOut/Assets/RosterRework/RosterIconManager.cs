using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RosterIconManager : MonoBehaviour
{
    [Header("Color Settings")]
    [Tooltip("Color predeterminado del marco.")]
    public Color defaultColor = Color.white; // Color original del marco.
    [Tooltip("Color cuando un jugador está encima.")]
    public Color highlightColor = Color.red; // Color cuando un jugador está encima.

    private Image marcoImage; // Referencia al componente Image del Marco.
    [HideInInspector]
    public string personaje; // Nombre del personaje seleccionado
    // Prefab del personaje seleccionado
    public GameObject characterPrefab;
    private List<GameObject> playersOnTop = new List<GameObject>(); // Lista de jugadores encima del icono.

    private void Start()
    {
        // Buscar automáticamente el componente Image en el objeto hijo "Marco".
        Transform marcoTransform = transform.Find("Marco");
        if (marcoTransform != null)
        {
            marcoImage = marcoTransform.GetComponent<Image>();
        }

        if (marcoImage == null)
        {
            Debug.LogError($"No se encontró el objeto 'Marco' o no tiene un componente Image en el prefab {gameObject.name}.");
        }
        else
        {
            // Asegurarse de que el marco comienza con el color predeterminado.
            marcoImage.color = defaultColor;
        }
        personaje = characterPrefab.GetComponent<CharacterBaseController>().characterData.characterName;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Trigger Enter: " + other.name);
        if (other.CompareTag("Player"))
        {
            // Añadir al jugador si no está ya en la lista.
            if (!playersOnTop.Contains(other.gameObject))
            {
                playersOnTop.Add(other.gameObject);
            }

            // Actualizar el color del marco.
            UpdateMarcoColor();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        //Debug.Log("Trigger Exit: " + other.name);
        if (other.CompareTag("Player"))
        {
            // Eliminar al jugador de la lista.
            playersOnTop.Remove(other.gameObject);

            // Actualizar el color del marco.
            UpdateMarcoColor();
        }
    }

    private void UpdateMarcoColor()
    {
        // Cambiar al color resaltado si hay jugadores encima, o al color predeterminado si no.
        if (playersOnTop.Count > 0)
        {
            marcoImage.color = highlightColor;
        }
        else
        {
            marcoImage.color = defaultColor;
        }
    }
}
