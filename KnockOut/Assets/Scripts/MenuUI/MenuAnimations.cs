using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuAnimations : MonoBehaviour
{
    [SerializeField] private GameObject Rayo;
    [SerializeField] private GameObject Rayo2;
    [SerializeField] private GameObject Shield;
    [SerializeField] private GameObject FondoFalso;
    [SerializeField] private GameObject Blanco;
    [SerializeField] private GameObject Titulo;
    [SerializeField] private GameObject OptionButtoms;
    [SerializeField] private GameObject Cursor;

    [SerializeField] private RectTransform logoAnimation;

    private bool options = false;

        // Variables para los AudioSource
    private AudioSource audioSource1;
    private bool isAnimating = false;
    private AudioSource audioSource2;


    public void Start(){
        Cursor.SetActive(false);
        //Movemos al cursor a la posición 0,0,0 del canvas
        Cursor.transform.transform.position = new Vector3(960,540,0);
        LeanTween.moveX(Rayo.GetComponent<RectTransform>(), 350, 0.5f).setDelay(1f).setEase(LeanTweenType.easeOutBounce);
        LeanTween.moveX(Rayo2.GetComponent<RectTransform>(), -344, 0.5f).setDelay(1f).setEase(LeanTweenType.easeOutBounce);
        LeanTween.moveY(Shield.GetComponent<RectTransform>(), 300, 0.5f).setDelay(1.5f).setOnComplete(Flash);
    }

    void Update()
    {
        // Control del cursor 1 sin salirnos del canvas
        float moveX1 = Input.GetAxis("Horizontal1");
        float moveY1 = Input.GetAxis("Vertical1");

        // Mover el cursor con el input
        Cursor.transform.Translate(new Vector3(moveX1, moveY1, 0) * Time.deltaTime * 1111);

        // Limitar el movimiento del cursor al área del canvas
        RectTransform cursorRect = Cursor.GetComponent<RectTransform>();
        RectTransform fondoRect = FondoFalso.GetComponent<RectTransform>();

        // Obtener las dimensiones del canvas
        Vector3[] canvasCorners = new Vector3[4];
        fondoRect.GetWorldCorners(canvasCorners);

        // Limitar la posición del cursor dentro de los bordes del canvas
        Vector3 cursorPosition = Cursor.transform.position;

        cursorPosition.x = Mathf.Clamp(cursorPosition.x, canvasCorners[0].x, canvasCorners[2].x);
        cursorPosition.y = Mathf.Clamp(cursorPosition.y, canvasCorners[0].y, canvasCorners[1].y);

        Cursor.transform.position = cursorPosition;
    }


    public void BajarAlpha(){
        LeanTween.alpha(FondoFalso.GetComponent<RectTransform>(), 0f, 1f).setDelay(0.5f).setOnComplete(LogoBoing);
        FondoFalso.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void Flash(){
        Blanco.SetActive(true);
        Titulo.SetActive(true);

        
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length > 1) {
            audioSource1 = audioSources[0];
            audioSource2 = audioSources[1];
        }

        audioSource2.Play();
        var audioManager = FindObjectOfType<PersistentAudioManager>();
        // Comprobamos que exista y no este ya reproduciendo la cancion
        if (audioManager != null&&!audioManager.GetComponent<AudioSource>().isPlaying)
        {
            var audioSource = audioManager.GetComponent<AudioSource>();
            audioSource.Play();
        }
        Cursor.SetActive(true);

        LeanTween.alpha(Blanco.GetComponent<RectTransform>(), 0f, 0.5f).setDelay(0.1f).setOnComplete(BajarAlpha);
        Blanco.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }


    public void ActivateOptions()
    {
        if (isAnimating) return;

        isAnimating = true;

        if (!options)
        {
                LeanTween.moveY(OptionButtoms.GetComponent<RectTransform>(), 359, 1f)
                .setEase(LeanTweenType.easeOutElastic)
                .setOnComplete(() => { isAnimating = false; options = true; });
        }
        else
        {
                LeanTween.moveY(OptionButtoms.GetComponent<RectTransform>(), -100, 0.1f)
                .setOnComplete(() => { isAnimating = false; options = false; });
        }
    }

    public void LogoBoing(){
        Cursor.SetActive(true);
        LeanTween.moveY(logoAnimation, -20, 1f).setLoopPingPong();
    }

    public void Roster(){
        SceneManager.LoadScene(1);
    }

    public void Exit(){
        #if UNITY_EDITOR
            // Detener el juego en el editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Salir del juego en una compilación
            Application.Quit();
        #endif
    }
}