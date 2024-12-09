using UnityEngine;
using UnityEngine.UI;

public class DetectorDentro : MonoBehaviour
{
    private Collider2D objetoDentro; // Referencia al objeto detectado con el tag GameController
    private int jugador = 0; // Jugador que controla este cursor
    public string personaje; // Nombre del personaje seleccionado

    private int seleccionado = 0; // Indica si el personaje ha sido seleccionado
    private int hover = 0; // Indica si el personaje ha sido seleccionado

    public Text NamePlayer1;
    public Text NamePlayer2;
    public GameObject MarcoP1;
    public GameObject MarcoP2;
    public Image ImageP1;
    public Image ImageP2;

    // Se llama cuando un objeto entra en el trigger del collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("GameController") && hover==0)
        {
            objetoDentro = other; // Guarda la referencia al objeto
            Debug.Log("DENTRO");
            Debug.Log($"{objetoDentro.name}");

            // Verificar y modificar texto para P1
            if(objetoDentro.name == "P1")
            {
                hover = 1;
                MarcoP1.SetActive(true);
                NamePlayer1 = NamePlayer1.GetComponent<Text>();
                if (NamePlayer1 != null)
                {
                    if(personaje != "Alexeido"){
                        NamePlayer1.fontSize = 30;
                    }
                    NamePlayer1.text = personaje; // Corregido: asignación directa
                    Debug.Log($"Texto de Player1 establecido a: {personaje}");
                }

                ImageP1 = ImageP1.GetComponent<Image>();
                if (ImageP1 != null)
                {
                    ImageP1.sprite = GetComponent<Image>().sprite;
                }
            }
            // Verificar y modificar texto para P2
            else if(objetoDentro.name == "P2")
            {
                hover = 2;
                MarcoP2.SetActive(true);
                NamePlayer2 = NamePlayer2.GetComponent<Text>();
                if (NamePlayer2 != null)
                {
                    if(personaje != "Alexeido"){
                        NamePlayer2.fontSize = 30;
                    }
                    NamePlayer2.text = personaje;
                    Debug.Log($"Texto de Player2 establecido a: {personaje}");
                }

                ImageP2 = ImageP2.GetComponent<Image>();
                if (ImageP2 != null)
                {
                    ImageP2.sprite = GetComponent<Image>().sprite;
                }
            }
        }

    }

    // Se llama cuando un objeto sale del trigger
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("GameController"))
        {   
            if(other.name == "P1"&&hover==1)
            {
                NamePlayer1.text = "";
                NamePlayer1.fontSize = 25;
                MarcoP1.SetActive(false);
                ImageP1.sprite = null;
                hover = 0;
            }
            else if(other.name == "P2"&&hover==2)
            {
                NamePlayer2.text = "";
                NamePlayer2.fontSize = 25;
                MarcoP2.SetActive(false);
                ImageP2.sprite = null;
                hover = 0;
            }
            Debug.Log("FUERA");
        }
    }

    // Se llama en cada frame para verificar la pulsación del botón
    private void Update()
    {
        // Si presiona el botón Fire1 (por defecto, clic izquierdo o Ctrl izquierdo)
        if (Input.GetButtonDown("Fire1") && objetoDentro != null)
        {
            Debug.Log($"{objetoDentro.name} ha seleccionado a {personaje}");
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.Invoke();
            }  
            
        }
    }
}
