using UnityEngine;
using UnityEngine.UI;

namespace UnityJam.LuckyNumber.Intro
{
    public class UISpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float fps = 12f;

        float timer;
        int index;

        private void Reset()
        {
            image = GetComponent<Image>();
        }

        private void Update()
        {
            if (image == null || frames == null || frames.Length == 0)
            {
                enabled = false;
                return;
            }

            timer += Time.deltaTime;
            float frameTime = 1f / fps;
            while (timer >= frameTime)
            {
                timer -= frameTime;
                index = (index + 1) % frames.Length;
                image.sprite = frames[index];
            }
        }
    }
}
