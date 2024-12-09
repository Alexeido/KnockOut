using UnityEngine;
using UnityEngine.SceneManagement;

public class Animations : MonoBehaviour
{
    [SerializeField] private GameObject OptionButtoms;
    [SerializeField] private GameObject Blanco;
    bool options = false;
    bool isAnimating = false;

    public void Start()
    {
        LeanTween.alpha(Blanco.GetComponent<RectTransform>(), 0f, 1f);
        //Blanco.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    void Update()
    {
    }

    void Click(GameObject cursor)
    {
        Vector2 cursorWorldPosition = cursor.transform.position;
        ContactFilter2D filter = new ContactFilter2D();
        filter.layerMask = LayerMask.GetMask("Default"); // Ignora el Layer "Cursor"
        filter.useLayerMask = true;
        Collider2D[] results = new Collider2D[10];

        // Detectar colisiones usando un filtro
        int hitCount = Physics2D.OverlapPoint(cursorWorldPosition, filter, results);

        if (hitCount > 0)
        {
            foreach (Collider2D hitCollider in results)
            {
                if (hitCollider != null && !hitCollider.gameObject.Equals(cursor))
                {
                    if (hitCollider.CompareTag("Player"))
                    {
                        Debug.Log("Has hecho clic en un personaje con el tag 'Player': " + hitCollider.gameObject.name);
                    }
                    else
                    {
                        Debug.Log("Clic en otro objeto: " + hitCollider.gameObject.name);
                    }
                    break;
                }
            }
        }
        else
        {
            Debug.Log("No se detectó ningún objeto bajo el cursor.");
        }
    }

    public void ActivateOptions()
    {
        if (isAnimating) return;

        isAnimating = true;

        if (!options)
        {
            LeanTween.moveY(OptionButtoms.GetComponent<RectTransform>(), -322, 1f)
                .setEase(LeanTweenType.easeOutElastic)
                .setOnComplete(() => { isAnimating = false; options = true; });
        }
        else
        {
            LeanTween.moveY(OptionButtoms.GetComponent<RectTransform>(), 137, 0.1f)
                .setOnComplete(() => { isAnimating = false; options = false; });
        }
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }
}