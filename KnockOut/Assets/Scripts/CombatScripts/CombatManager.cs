using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CombatManager : MonoBehaviour
{
    // Le pasamo la lista de prefabs a la camara y al HUD Manager
    public List<GameObject> characterPrefabs;
    public Camera mainCamera;
    //Collider del escenario
    public Collider2D stageCollider;
    public float ejeZ= 225;

    // Lista de jugadores
    public List<CharacterBaseController> players = new List<CharacterBaseController>();
    public int numStocks = 3;
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

    [Header("UI Elements")]
    public TextMeshProUGUI gameEndMessage; // Mensaje del ganador
    public TextMeshProUGUI startPromptMessage; // Mensaje para pulsar START



    void Start()
    {   
        var audioManager = FindObjectOfType<PersistentAudioManager>();
        audioManager = FindObjectOfType<PersistentAudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayCombatMusic();
        }
        gameEndMessage.gameObject.SetActive(false);
        startPromptMessage.gameObject.SetActive(false);
        Debug.Log("Starting CombatManager, number of players: " + GameData.PlayersData.Count);
        // Crear instancias de los jugadores seleccionados
        foreach (var playerData in GameData.PlayersData)
        {
            Debug.Log($"Player {playerData.playerIndex+1} selected {playerData.characterPrefab.name}");
            GameObject playerInstance = Instantiate(playerData.characterPrefab);

            // Configurar al jugador (posición, ID, etc.)
            playerInstance.name = $"Player{playerData.playerIndex+1}";
            playerInstance.GetComponent<CharacterBaseController>().playerID = playerData.playerIndex+1;
            // Posicionamos a los jugadores en la escena de forma dinamica, por el numero de jugadores, haciendo un espaciado entre ellos
            playerInstance.GetComponent<CharacterBaseController>().spawnpoint = new Vector3(stageCollider.bounds.min.x + (stageCollider.bounds.size.x / (GameData.PlayersData.Count + 1)) * (playerData.playerIndex + 1), 120, ejeZ);
            
            

            // Recalculamos el eje Y en el punto X que nos ha dado en concreto para que no se queden en el aire
            Vector3 spawnPosition = playerInstance.GetComponent<CharacterBaseController>().spawnpoint;
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(spawnPosition.x, stageCollider.bounds.max.y + 10f), Vector2.down, Mathf.Infinity, LayerMask.GetMask("Ground"));

            if (hit.collider != null)
            {
                // Si el raycast golpea el suelo, calculamos la posición correcta en el eje Y
                float groundY = hit.point.y;
                float characterHeight = playerInstance.GetComponent<SpriteRenderer>().bounds.size.y;
                spawnPosition.y = groundY + characterHeight / 2; // Ajustamos para que el personaje quede justo sobre el suelo
            }
            else
            {
                Debug.LogWarning($"No se encontró un suelo en el spawnpoint X={spawnPosition.x}. Usando altura predeterminada.");
            }

            // Actualizamos la posición del spawnpoint
            playerInstance.GetComponent<CharacterBaseController>().spawnpoint = spawnPosition;

            // Asignamos la posición final al transform
            playerInstance.transform.position = spawnPosition;
            // Si el jugador esta de la mitad a la izquierda del mapa, lo ponemos mirando a la derecha
            playerInstance.GetComponent<CharacterBaseController>().stocks = numStocks;
            if (playerInstance.transform.position.x < stageCollider.bounds.center.x)
            {
                playerInstance.GetComponent<CharacterBaseController>().facingRight = true;
            }
            else
            {
                playerInstance.GetComponent<CharacterBaseController>().facingRight = false;
            }
            mainCamera.GetComponent<SmashCamera>().AddPlayer(playerInstance.GetComponent<CharacterBaseController>());
            mainCamera.GetComponent<SmashCamera>().stageBounds=stageCollider;
            GetComponent<CombatHUDManager>().AddPlayer(playerInstance.GetComponent<CharacterBaseController>());
            players.Add(playerInstance.GetComponent<CharacterBaseController>());
        }

        // Tras esto ya podemos empezar el combate
        startAllPlayers();
    }
    
    void Update()
    {
        if (gameEnded() && (Input.GetButtonDown("Start1") || Input.GetButtonDown("Start2") || Input.GetButtonDown("Start3") || Input.GetButtonDown("Start4"))) // "Submit" es el botón por defecto para START        
        {
            RestartGame();
        }
    }
    private bool gameEnded()
    {
        // Comprobar si solo queda un jugador con stocks
        int playersLeft = 0;
        int actualWinner = 0;
        for (int i = 0; i < players.Count&& playersLeft<2; i++)
        {
            if (players[i].stocks > 0)
            {
                playersLeft++;
                actualWinner = i;
            }
        }
        if (playersLeft < 2)
        {
            EndGame(players[actualWinner]);
            return true;
        }
        return false;

    }
    public void EndGame(CharacterBaseController winner)
    {

        // Obtener el color del ganador basado en su ID
        Color winnerColor = playerColors[winner.playerID - 1]; // Ajustamos el índice (-1 porque los IDs empiezan en 1)
        string winnerColorHex = ColorUtility.ToHtmlStringRGB(winnerColor); // Convertir a formato hexadecimal

        // Construir el mensaje del ganador con colores
        gameEndMessage.text = $"Ganador:\n\n{winner.characterData.characterName} <color=#{winnerColorHex}>P{winner.playerID}</color>";
        gameEndMessage.gameObject.SetActive(true);

        // Construir el mensaje para pulsar START
        string startColorHex = ColorUtility.ToHtmlStringRGB(Color.red); // Rojo para el mensaje START
        startPromptMessage.text = $"¡Pulsa <color=#{startColorHex}>START</color> para volver al menu!";
        startPromptMessage.gameObject.SetActive(true);

        freezeAllPlayers();
    }

    private void RestartGame()
    {
        var audioManager = FindObjectOfType<PersistentAudioManager>();
        audioManager = FindObjectOfType<PersistentAudioManager>();
        if (audioManager != null)
        {
            audioManager.PlayMenuMusic();
        }
        // Limpiar datos de memoria compartida
        GameData.PlayersData.Clear();

        // Cargar escena inicial (escena 1)
        SceneManager.LoadScene(1);
    }

    IEnumerator FreezeUnfreezeRoutine()
    {
        yield return new WaitForSeconds(5);
        // Esperamos 5 segundos para probar el freeze
        freezeAllPlayers();
        yield return new WaitForSeconds(5);

        // Esperamos 5 segundos para probar el unfreeze
        unfreezeAllPlayers();
    }


    private void freezeAllPlayers()
    {
        foreach (var player in mainCamera.GetComponent<SmashCamera>().players)
        {
            player.GetComponent<CharacterBaseController>().freezing = true;
        }
    }

    private void unfreezeAllPlayers()
    {
        foreach (var player in mainCamera.GetComponent<SmashCamera>().players)
        {
            player.GetComponent<CharacterBaseController>().freezing = false;
        }
    }    private void startAllPlayers()
    {
        foreach (var player in mainCamera.GetComponent<SmashCamera>().players)
        {
            player.GetComponent<CharacterBaseController>().enabled = true;
            player.GetComponent<CharacterBaseController>().StartCharacter();
        }
    }
}
