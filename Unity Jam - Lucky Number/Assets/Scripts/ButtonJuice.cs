using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// hover grow + press squash on every Button. Added at runtime to each scene's buttons
// Buttons spawned later (ItemTray) add it themselves.
public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const float HoverScale = 1.06f;
    private const float PressScale = 0.92f;
    private const float Speed = 18f;

    private Vector3 baseScale;
    private bool hovered;
    private bool pressed;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Hook() => SceneManager.sceneLoaded += (scene, mode) =>
    {
        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (button.GetComponent<ButtonJuice>() == null) button.gameObject.AddComponent<ButtonJuice>();
    };

    private void Awake() => baseScale = transform.localScale;

    private void Update()
    {
        float target = pressed ? PressScale : hovered ? HoverScale : 1f;
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, Speed * Time.unscaledDeltaTime);
    }

    private void OnDisable()
    {
        hovered = pressed = false;
        transform.localScale = baseScale;
    }

    public void OnPointerEnter(PointerEventData eventData) => hovered = true;
    public void OnPointerExit(PointerEventData eventData) => hovered = pressed = false;
    public void OnPointerDown(PointerEventData eventData) => pressed = true;
    public void OnPointerUp(PointerEventData eventData) => pressed = false;
}
