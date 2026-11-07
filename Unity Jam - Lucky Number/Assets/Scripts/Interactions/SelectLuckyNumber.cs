using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.Interactions
{
    public class SelectLuckyNumber : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI selectedLuckyText;
        [SerializeField] private int number = 1;
        [SerializeField] private float hoverScale = 1.15f;
        [SerializeField] private float pickedScale = 1.25f;
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
        private Vector3 baseScale;

        public int LuckyNumber => number;

        private void Awake()
        {
            image = GetComponent<Image>();
            baseScale = transform.localScale;
        }

        private void Update()
        {
            float target = current == this ? pickedScale : hovered ? hoverScale : 1f;
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale * target, animSpeed * Time.unscaledDeltaTime);
            image.color = current == this ? pickedColor : normalColor;
        }

        public void OnPointerEnter(PointerEventData eventData) => hovered = true;

        public void OnPointerExit(PointerEventData eventData) => hovered = false;

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
            onPicked?.Invoke(number);
            selectedLuckyText.text = number.ToString();
            Debug.Log("Lucky number picked: " + number);
        }

        private void OnDisable()
        {
            if (current == this)
                current = null;
        }
    }
}
