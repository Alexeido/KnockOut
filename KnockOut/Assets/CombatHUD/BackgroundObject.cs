using UnityEngine;

public class BackgroundObjectController : MonoBehaviour
{
    public bool rotateX;
    public bool rotateY;
    public bool rotateZ;
    public float rotationSpeedX;
    public float rotationSpeedY;
    public float rotationSpeedZ;

    public bool moveX;
    public bool moveY;
    public bool moveZ;
    public float moveSpeedX;
    public float moveSpeedY;
    public float moveSpeedZ;
    public float moveRangeX;
    public float moveRangeY;
    public float moveRangeZ;

    public bool scale;
    public float scaleSpeed;
    public float scaleRange;

    public bool randomMove;
    public float randomMoveSpeed;
    public float randomMoveRange;

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private Vector3 randomDirection;

    void Start()
    {
        initialPosition = transform.position;
        initialScale = transform.localScale;

        // Inicializar la dirección aleatoria
        randomDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
    }

    void Update()
    {
        // Rotation
        if (rotateX || rotateY || rotateZ)
        {
            float rotationX = rotateX ? rotationSpeedX * Time.deltaTime : 0;
            float rotationY = rotateY ? rotationSpeedY * Time.deltaTime : 0;
            float rotationZ = rotateZ ? rotationSpeedZ * Time.deltaTime : 0;
            transform.Rotate(new Vector3(rotationX, rotationY, rotationZ));
        }

        // Movement
        if (moveX || moveY || moveZ)
        {
            Vector3 newPosition = initialPosition;
            if (moveX)
            {
                newPosition.x += Mathf.PingPong(Time.time * moveSpeedX, moveRangeX) - moveRangeX / 2;
            }
            if (moveY)
            {
                newPosition.y += Mathf.PingPong(Time.time * moveSpeedY, moveRangeY) - moveRangeY / 2;
            }
            if (moveZ)
            {
                newPosition.z += Mathf.PingPong(Time.time * moveSpeedZ, moveRangeZ) - moveRangeZ / 2;
            }
            transform.position = newPosition;
        }

        // Random Smooth Movement (improved)
        if (randomMove)
        {
            // Actualizar la dirección aleatoria gradualmente para un movimiento más orgánico
            randomDirection = Vector3.Lerp(
                randomDirection,
                new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f)
                ).normalized,
                Time.deltaTime
            ).normalized;

            // Mover el objeto en la dirección aleatoria
            transform.position += randomDirection * randomMoveSpeed * Time.deltaTime;

            // Asegurar que el objeto permanece dentro del rango definido
            Vector3 offset = transform.position - initialPosition;
            if (offset.magnitude > randomMoveRange)
            {
                randomDirection = -offset.normalized; // Cambiar la dirección hacia adentro
            }
        }

        // Scaling
        if (scale)
        {
            float newScale = initialScale.x + Mathf.PingPong(Time.time * scaleSpeed, scaleRange) - scaleRange / 2;
            transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }
}
