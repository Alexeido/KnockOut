using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RosterManager : MonoBehaviour
{
    [Header("Roster Settings")]
    [Tooltip("Lista de prefabs de los personajes disponibles.")]
    public List<GameObject> characterPrefabs; // Lista de prefabs de personajes.

    [Tooltip("Prefab del icono del roster.")]
    public GameObject rosterIconPrefab; // Prefab del icono del roster.

    [Tooltip("Contenedor donde se colocarán los iconos del roster.")]
    public Transform rosterContainer; // Contenedor de los iconos del roster.

    void Start()
    {
        // Generar el roster al inicio.
        GenerateRoster();
    }

    private void GenerateRoster()
    {
        // Limpiar cualquier contenido anterior en el contenedor del roster.
        foreach (Transform child in rosterContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear un icono del roster para cada personaje en la lista.
        foreach (GameObject characterPrefab in characterPrefabs)
        {
            // Obtener el CharacterBaseController del prefab del personaje.
            CharacterBaseController characterController = characterPrefab.GetComponent<CharacterBaseController>();
            if (characterController != null && characterController.characterData != null)
            {
                // Instanciar el icono del roster.
                GameObject rosterIcon = Instantiate(rosterIconPrefab, rosterContainer);

                // Configurar el icono con los datos del personaje.
                Image iconImage = rosterIcon.transform.Find("Icon").GetComponent<Image>();
                Image marcoImage = rosterIcon.transform.Find("Marco").GetComponent<Image>();
                rosterIcon.GetComponent<RosterIconManager>().characterPrefab = characterPrefab;

                if (iconImage != null)
                {
                    iconImage.sprite = characterController.characterData.characterIcon;
                }

                // (Opcional) Puedes cambiar colores o propiedades del marco según necesites.
            }
            else
            {
                Debug.LogWarning($"El prefab {characterPrefab.name} no tiene un CharacterBaseController o CharacterData.");
            }
        }
    }
}
