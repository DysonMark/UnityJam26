using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.Interactions
{
    public class SelectLuckyNumber : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
        IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private TextMeshProUGUI selectedLuckyText;
        [SerializeField] private int number = 1;
        [SerializeField] private float hoverScale = 1.15f;
        [SerializeField] private float pickedScale = 1.25f;
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private Sprite normalSprite;   // dice_N_normal
        [SerializeField] private Sprite hoverSprite;    // dice_N_hover
        [SerializeField] private Sprite pressedSprite;  // dice_N_dragging, also while picked
        [SerializeField] private float animSpeed = 12f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color pickedColor = new Color(1f, 0.8f, 0.3f);
        [Tooltip("Delay use to show the color before the panel is disabled")]
        [SerializeField] private float pickDelay = 0.5f;
        private static bool locked;
        public UnityEvent<int> onPicked;

        private static SelectLuckyNumber current;
        private Image image;
        private bool hovered;
        private bool pressed;
        private Vector3 baseScale;

        public int LuckyNumber => number;

        private void Awake()
        {
            image = GetComponent<Image>();
            baseScale = transform.localScale;
            if (normalSprite == null) normalSprite = hoverSprite = pressedSprite = image.sprite; // TEMP: null guard until Phase 2 wires the scene
        }

        private void Update()
        {
            float target = current == this ? pickedScale : pressed ? pressedScale : hovered ? hoverScale : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, animSpeed * Time.unscaledDeltaTime);
            image.color = current == this ? pickedColor : normalColor;
            image.sprite = pressed || current == this ? pressedSprite : hovered ? hoverSprite : normalSprite;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;

        public void OnPointerExit(PointerEventData eventData) => hovered = false;

        public void OnPointerDown(PointerEventData eventData) => pressed = true;

        public void OnPointerUp(PointerEventData eventData) => pressed = false;

        public void OnPointerClick(PointerEventData eventData)
        {
            if (locked)
                return;
            StartCoroutine(PickRoutine());
        }

        private IEnumerator PickRoutine()
        {
            locked = true;
            current = this;
            yield return new WaitForSecondsRealtime(pickDelay);
            locked = false;
            selectedLuckyText.text = number.ToString(); // before onPicked so TurnManager can add the second number
            onPicked?.Invoke(number);
            Debug.Log("Lucky number picked: " + number);
        }

        private void OnDisable()
        {
            if (current == this)
                current = null;
            locked = false;
            pressed = false;
            hovered = false;
        }
    }
}
