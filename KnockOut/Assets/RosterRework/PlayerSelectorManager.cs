using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerSelectorManager : MonoBehaviour
{
    [Header("Player Selector Settings")]
    public GameObject selectorPrefab; // Prefab de la bolita (selector)
    public GameObject playerSlotPrefab; // Prefab del cuadro de selección (abajo)
    public Transform selectorsContainer; // Contenedor donde se generarán las bolitas
    public Transform slotsContainer; // Contenedor donde se generarán los cuadros

    [Header("UI Elements")]
    [Tooltip("Texto que muestra un mensaje si hay un solo jugador.")]
    public TextMeshProUGUI errorMessage; // Texto de error para mostrar el mensaje
    public Sprite emptySlotSprite; // Sprite de slot vacío
    [Header("Player Colors")]
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

    [Header("Controls")]
    public int maxPlayers = 8; // Máximo número de jugadores permitido

    private string[] horizontalAxes = { "Horizontal1", "Horizontal2", "Horizontal3", "Horizontal4", "Horizontal5", "Horizontal6", "Horizontal7", "Horizontal8" };
    private string[] verticalAxes = { "Vertical1", "Vertical2", "Vertical3", "Vertical4", "Vertical5", "Vertical6", "Vertical7", "Vertical8" };
    private string[] fire1Buttons = { "Fire1", "Fire1_2", "Fire1_3", "Fire1_4", "Fire1_5", "Fire1_6", "Fire1_7", "Fire1_8" };
    private string[] fire2Buttons = { "Fire2", "Fire2_2", "Fire2_3", "Fire2_4", "Fire2_5", "Fire2_6", "Fire2_7", "Fire2_8" };
    private string[] startButtons = { "Start1", "Start2", "Start3", "Start4", "Start5", "Start6", "Start7", "Start8" };

    private List<GameObject> selectors = new List<GameObject>();
    private List<GameObject> playerSlots = new List<GameObject>();
    private Dictionary<int, bool> isPlayerLocked = new Dictionary<int, bool>();
    private Dictionary<int, GameObject> selectedRosterIcons = new Dictionary<int, GameObject>();

    private Coroutine errorBlinkCoroutine;
    private int lastConnectedPlayers = 0;

    void Start()
    {
        InitializePlayers();
    }

    private void InitializePlayers()
    {
        UpdateConnectedPlayers();
    }

    void Update()
    {
        // Detectar cambios en los mandos conectados/desconectados
        if (Input.GetJoystickNames().Length != lastConnectedPlayers)
        {
            UpdateConnectedPlayers();
        }

        // Mover cada bolita según los controles del mando
        for (int i = 0; i < selectors.Count; i++)
        {
            HandlePlayerInput(i);
        }
        checkStartBattle();
    }

    // Método para comprobar si todos los mandos activos han escogido personaje y mostrar un aviso de que pulsando Start comienza la batalla
    private void checkStartBattle(){
        bool allPlayersReady = true;
        foreach (var player in isPlayerLocked)
        {
            if (!player.Value)
            {
                allPlayersReady = false;
                break;
            }
        }
        if (allPlayersReady && playerSlots.Count > 1)
        {
            errorMessage.text = "¡Pulsa Start para comenzar!";
            errorMessage.gameObject.SetActive(true);
            // Si se pulsa Start, se inicia la batalla
            for (int i = 0; i < playerSlots.Count; i++)
            {
                if (Input.GetButtonDown(startButtons[i]))
                {
                    startBattle();
                }
            }
        }
        else if (playerSlots.Count > 1)
        {
            errorMessage.text = "";
            errorMessage.gameObject.SetActive(false);
        }
    }

    private void startBattle()
    {
        // Crear datos para enviar a la siguiente escena
        List<PlayerData> playersData = new List<PlayerData>();

        foreach (var pair in selectedRosterIcons)
        {
            int playerIndex = pair.Key;
            GameObject rosterIcon = pair.Value;

            GameObject characterPrefab = rosterIcon.GetComponent<RosterIconManager>().characterPrefab;
            Debug.Log($"Player {playerIndex + 1} selected {characterPrefab.name}");
            playersData.Add(new PlayerData
            {
                playerIndex = playerIndex,
                characterPrefab = characterPrefab
            });
        }

        // Guardar los datos en un script estático
        GameData.PlayersData = playersData;
        Debug.Log("Starting battle with " + GameData.PlayersData.Count + " players.");
        // Cambiar a la escena de combate
        SceneManager.LoadScene(2);
    }
    private void UpdateConnectedPlayers()
    {
        int connectedPlayers = Mathf.Min(Input.GetJoystickNames().Length, maxPlayers);

        // Añadir nuevos jugadores si hay más mandos conectados
        for (int i = selectors.Count; i < connectedPlayers; i++)
        {
            AddPlayer(i);
        }

        // Eliminar jugadores si hay menos mandos conectados
        for (int i = selectors.Count - 1; i >= connectedPlayers; i--)
        {
            RemovePlayer(i);
        }

        // Mostrar o esconder el mensaje de error
        if (connectedPlayers <= 1)
        {
            if (connectedPlayers == 1)
            {
                errorMessage.text = "Conecta otro mando para jugar";
            }
            else
            {
                errorMessage.text = "Conecta dos mandos para jugar";
            }

            if (errorBlinkCoroutine == null)
            {
                errorBlinkCoroutine = StartCoroutine(BlinkErrorMessage());
            }
        }
        else
        {
            errorMessage.text = "";
            errorMessage.gameObject.SetActive(false);

            if (errorBlinkCoroutine != null)
            {
                StopCoroutine(errorBlinkCoroutine);
                errorBlinkCoroutine = null;
            }
        }

        ReorderPlayerSlots();
        lastConnectedPlayers = connectedPlayers;
    }
    private void ReorderPlayerSlots()
    {
        // Ordenar de forma invertida los slots en la jerarquía por su PlayerID
        for (int i = 0; i < playerSlots.Count; i++)
        {
            playerSlots[i].transform.SetSiblingIndex(playerSlots.Count - i - 1);
        }
    }
    private IEnumerator BlinkErrorMessage()
    {
        errorMessage.gameObject.SetActive(true);

        while (true)
        {
            errorMessage.alpha = 1f; // Totalmente visible
            yield return new WaitForSeconds(0.5f); // Pausa medio segundo

            errorMessage.alpha = 0f; // Totalmente invisible
            yield return new WaitForSeconds(0.5f); // Pausa medio segundo
        }
    }

    private void AddPlayer(int playerIndex)
    {
        // Crear un cuadro de selección abajo
        GameObject newSlot = Instantiate(playerSlotPrefab, slotsContainer);
        newSlot.GetComponentInChildren<TextMeshProUGUI>().text = $"P{playerIndex + 1}";
        Image bordeImage = newSlot.transform.Find("Borde").GetComponent<Image>();
        bordeImage.color = playerColors[playerIndex];

        playerSlots.Add(newSlot);

        // Crear una nueva bolita (selector)
        GameObject newSelector = Instantiate(selectorPrefab, selectorsContainer);
        TextMeshProUGUI selectorText = newSelector.GetComponentInChildren<TextMeshProUGUI>();
        selectorText.text = $"P{playerIndex + 1}";
        newSelector.GetComponent<Image>().color = playerColors[playerIndex];

        selectors.Add(newSelector);
        isPlayerLocked[playerIndex] = false;
        // Movemos la bolita a la posición donde esta el slot pero 100px mas arriba
        newSelector.GetComponent<RectTransform>().anchoredPosition = new Vector2(newSlot.GetComponent<RectTransform>().anchoredPosition.x, newSlot.GetComponent<RectTransform>().anchoredPosition.y + 100);


    }

    private void RemovePlayer(int playerIndex)
    {
        Destroy(selectors[playerIndex]);
        selectors.RemoveAt(playerIndex);

        Destroy(playerSlots[playerIndex]);
        playerSlots.RemoveAt(playerIndex);
        isPlayerLocked.Remove(playerIndex);
        selectedRosterIcons.Remove(playerIndex);
    }

private void HandlePlayerInput(int playerIndex)
{
    string horizontalAxis = horizontalAxes[playerIndex];
    string verticalAxis = verticalAxes[playerIndex];
    string fire1Button = fire1Buttons[playerIndex];
    string fire2Button = fire2Buttons[playerIndex];

    float horizontal = Input.GetAxis(horizontalAxis);
    float vertical = Input.GetAxis(verticalAxis);

    GameObject selector = selectors[playerIndex];
    RectTransform selectorTransform = selector.GetComponent<RectTransform>();

    // Movimiento del selector
    if (!isPlayerLocked[playerIndex])
    {
        Vector3 movement = new Vector3(horizontal, vertical, 0);
        selectorTransform.anchoredPosition += (Vector2)(movement * Time.deltaTime * 700f);

        // Limitar la posición dentro del contenedor
        RectTransform containerTransform = selectorsContainer.GetComponent<RectTransform>();
        Vector2 clampedPosition = selectorTransform.anchoredPosition;
        Vector2 containerSize = containerTransform.rect.size / 2;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -containerSize.x, containerSize.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -containerSize.y, containerSize.y);

        selectorTransform.anchoredPosition = clampedPosition;

        // Comprobar colisiones con RosterIcons
        Collider2D[] collisions = Physics2D.OverlapCircleAll(selectorTransform.position, 10f);
        bool isOverRosterIcon = false;

        foreach (var collision in collisions)
        {
            if (collision.CompareTag("RosterIcon"))
            {
                isOverRosterIcon = true;
                UpdateSlotPreview(playerIndex, collision.gameObject);
                if (Input.GetButtonDown(fire1Button))
                    {
                        SelectCharacter(playerIndex, collision.gameObject);
                    }
            }
        }

        // Si no está sobre ningún RosterIcon, restaurar el slot
        if (!isOverRosterIcon)
        {
            RestoreSlotPreview(playerIndex);
        }
    }

    // Deseleccionar personaje
    if (isPlayerLocked[playerIndex] && Input.GetButtonDown(fire2Button))
    {
        DeselectCharacter(playerIndex);
    }
}

private void UpdateSlotPreview(int playerIndex, GameObject rosterIcon)
{
    var slot = playerSlots[playerIndex];

    // Actualizar el nombre y la imagen del slot
    var slotImage = slot.transform.Find("Cara").GetComponent<Image>();
    var slotText = slot.transform.Find("Texto").GetComponent<TextMeshProUGUI>();

    var rosterManager = rosterIcon.GetComponent<RosterIconManager>();
    var rosterIconImage = rosterIcon.transform.Find("Icon").GetComponent<Image>();

    slotImage.sprite = rosterIconImage.sprite; // Actualizar imagen
    slotText.text = rosterManager.personaje;   // Actualizar texto
}

private void RestoreSlotPreview(int playerIndex)
{
    var slot = playerSlots[playerIndex];

    // Restaurar el estado predeterminado
    var slotImage = slot.transform.Find("Cara").GetComponent<Image>();
    var slotText = slot.transform.Find("Texto").GetComponent<TextMeshProUGUI>();

    slotImage.sprite = emptySlotSprite;       // Restaurar sprite vacío
    slotText.text = $"P{playerIndex + 1}";    // Restaurar texto predeterminado
}


    private void SelectCharacter(int playerIndex, GameObject rosterIcon)
    {
        // Bloquear el jugador
        isPlayerLocked[playerIndex] = true;

        // Cambiar color del selector y slot
        selectors[playerIndex].GetComponent<Image>().color *= 0.7f;
        playerSlots[playerIndex].transform.Find("Borde").GetComponent<Image>().color *= 0.7f;
        //Añadimos a SelectedRosterIcons el rosterIcon seleccionado
        selectedRosterIcons[playerIndex] = rosterIcon;
    }

    private void DeselectCharacter(int playerIndex)
    {
        // Desbloquear el jugador
        isPlayerLocked[playerIndex] = false;
        selectedRosterIcons[playerIndex] = null;
        // Restaurar color del selector y slot
        selectors[playerIndex].GetComponent<Image>().color = playerColors[playerIndex];
        playerSlots[playerIndex].transform.Find("Borde").GetComponent<Image>().color = playerColors[playerIndex];
    }

    

}

// Clase para almacenar los datos de cada jugador
[System.Serializable]
public class PlayerData
{
    public int playerIndex;          // Índice del jugador
    public GameObject characterPrefab; // Prefab del personaje
}

// Clase estática para compartir datos entre escenas
public static class GameData
{
    public static List<PlayerData> PlayersData = new List<PlayerData>();
}