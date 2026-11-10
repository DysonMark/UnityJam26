using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Plays one pixel VFX strip on an Image. Frames left to right, hidden when done unless it loops.
public class VfxPlayer : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Texture2D strip;
    [SerializeField] private int frameCount = 8;  // frame width = strip width / frameCount (not always square)
    [SerializeField] private float fps = 15f;
    [SerializeField] private bool loop;

    private Sprite[] frames;
    private Coroutine playing;

    private void Awake()
    {
        int w = strip.width / frameCount;
        frames = new Sprite[frameCount];
        for (int i = 0; i < frameCount; i++)
            frames[i] = Sprite.Create(strip, new Rect(i * w, 0, w, strip.height), new Vector2(0.5f, 0.5f));
        image.enabled = false;
    }

    private void OnEnable()
    {
        if (loop) Play();
    }

    public void Play()
    {
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(Run());
    }

    public void Stop()
    {
        if (playing != null) StopCoroutine(playing);
        image.enabled = false;
    }

    private IEnumerator Run()
    {
        image.enabled = true;
        do
        {
            foreach (Sprite frame in frames)
            {
                image.sprite = frame;
                yield return new WaitForSeconds(1f / fps);
            }
        } while (loop);
        image.enabled = false;
    }

    private void OnDestroy()
    {
        foreach (Sprite frame in frames) Destroy(frame);
    }
}
