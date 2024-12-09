using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SmashCamera : MonoBehaviour
{
    public List<CharacterBaseController> players; // Lista de todos los jugadores
    public Collider2D blastZone; // Collider de la blastzone
    public Collider2D stageBounds; // Collider del escenario
    public float minZoom = 300f; // Zoom mínimo
    public float maxZoom = 0f; // Zoom máximo
    public float zoomSpeed = 10f; // Velocidad de transición del zoom
    public float cameraMoveSpeed = 5f; // Velocidad de movimiento de la cámara
    public float padding = 2f; // Espacio adicional alrededor de los jugadores
    public float yOffset = 10f; // Desplazamiento adicional en el eje Y
    public float minYLimit = -50f; // Límite mínimo en el eje Y
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        UpdateCamera();
    }

    
    private void UpdateCamera()
    {
        if (players == null || players.Count == 0) return;
        // Eliminamos los jugadores que valen null
        players.RemoveAll(p => p == null);
        // Filtrar jugadores activos (playerID > 0 y stocks > 0)
        List<CharacterBaseController> activePlayers = players.FindAll(p => p.playerID > 0 && p.stocks > 0);

        if (activePlayers.Count == 0) return;

        // Calcular el centro promedio de los jugadores
        Vector3 averagePosition = Vector3.zero;
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (var player in activePlayers)
        {
            Vector3 position = player.transform.position;

            averagePosition += position;

            // Encontrar los límites de los jugadores
            if (position.x < minX) minX = position.x;
            if (position.x > maxX) maxX = position.x;
            if (position.y < minY) minY = position.y;
            if (position.y > maxY) maxY = position.y;
        }

        averagePosition /= activePlayers.Count;

        // Asegurarse de que el centro promedio esté dentro de los límites del escenario
        float stageMinX = stageBounds != null ? stageBounds.bounds.min.x : -146f; // Límite izquierdo del escenario
        float stageMaxX = stageBounds != null ? stageBounds.bounds.max.x : 468f;  // Límite derecho del escenario

        float clampedX = Mathf.Clamp(averagePosition.x, stageMinX, stageMaxX);
        float clampedY = Mathf.Clamp(averagePosition.y, minYLimit, maxY)+yOffset; // Sumar el yOffset y limitar con minYLimit

        Vector3 targetPosition = new Vector3(clampedX, clampedY, transform.position.z);

        // Mover la cámara suavemente hacia la posición objetivo
        transform.position = Vector3.Lerp(transform.position, targetPosition, cameraMoveSpeed * Time.deltaTime);

        // Calcular el zoom basado en la distancia máxima entre jugadores
        float maxDistance = 0f;
        foreach (var player in activePlayers)
        {
            foreach (var otherPlayer in activePlayers)
            {
                float distance = Vector3.Distance(player.transform.position, otherPlayer.transform.position);
                if (distance > maxDistance) maxDistance = distance;
            }
        }

        // Ajustar el zoom moviendo la cámara en el eje Z
        float desiredZ = Mathf.Lerp(-maxZoom, -minZoom, maxDistance / (stageMaxX - stageMinX));
        transform.position = new Vector3(transform.position.x, transform.position.y, Mathf.Lerp(transform.position.z, desiredZ, zoomSpeed * Time.deltaTime));

        // Limitar la cámara dentro de la blastzone
        if (blastZone != null)
        {
            Bounds bounds = blastZone.bounds;
            float halfHeight = Mathf.Abs(transform.position.z); // Esto depende del tamaño Z como zoom
            float halfWidth = cam.aspect * halfHeight;

            float boundedX = Mathf.Clamp(transform.position.x, bounds.min.x + halfWidth, bounds.max.x - halfWidth);
            float boundedY = Mathf.Clamp(transform.position.y, bounds.min.y + halfHeight, bounds.max.y - halfHeight);

            transform.position = new Vector3(boundedX, boundedY, transform.position.z);
        }
    }
    public void AddPlayer(CharacterBaseController player)
    {
        if (players == null) players = new List<CharacterBaseController>();
        if (!players.Contains(player)) players.Add(player);
    }
}