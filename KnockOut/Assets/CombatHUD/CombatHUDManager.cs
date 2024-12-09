using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CombatHUDManager : MonoBehaviour
{
    [Header("HUD Elements")]
    [Tooltip("Prefab del elemento HUD de cada jugador.")]
    public GameObject hudElementPrefab;

    [Tooltip("Contenedor donde se instanciarán los HUDs de los jugadores.")]
    public Transform hudContainer;

    [Header("Characters")]
    [Tooltip("Lista de todos los personajes presentes en la escena.")]
    public List<CharacterBaseController> characters = new List<CharacterBaseController>();

    private Dictionary<int, GameObject> hudElements = new Dictionary<int, GameObject>();

    void Start()
    {
        // Crear los HUDs iniciales
        UpdateHUD();
    }

    void Update()
    {
        // Actualizar dinámicamente los valores del HUD
        UpdateHUDValues();
    }

    private void UpdateHUD()
    {
        // Limpiar el HUD actual
        foreach (var hud in hudElements.Values)
        {
            Destroy(hud);
        }
        hudElements.Clear();

        // Ordenar los personajes por playerID
        List<CharacterBaseController> activeCharacters = new List<CharacterBaseController>();
        foreach (var character in characters)
        {
            if (character.playerID != 0 && character!=null)
            {
                activeCharacters.Add(character);
            }
        }
        activeCharacters.Sort((a, b) => a.playerID.CompareTo(b.playerID));

        // Generar un HUD para cada personaje activo
        foreach (var character in activeCharacters)
        {
            GameObject hudElement = Instantiate(hudElementPrefab, hudContainer);
            hudElements[character.playerID] = hudElement;

            // Inicializar los valores estáticos
            hudElement.GetComponent<HUDComponent>().InitializeHUD(character);
        }

        // Si no hay personajes activos, ocultar el HUD
        hudContainer.gameObject.SetActive(activeCharacters.Count > 0);
    }

    private void UpdateHUDValues()
    {
        // Actualizar dinámicamente los valores del HUD
        foreach (var character in characters)
        {
            if (character.playerID != 0 && hudElements.ContainsKey(character.playerID))
            {
                hudElements[character.playerID].GetComponent<HUDComponent>().UpdateHUDValues(character);
            }
        }
    }
    public void AddPlayer(CharacterBaseController character)
    {
        characters.Add(character);
        UpdateHUD();
    }
}
