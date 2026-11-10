using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// One die that plays a roll strip on an Image. Used for the player's clean/loaded dice and the AI bet dice.
// Strip = square frames left to right, the last frame shows the rolled face and stays up.
public class DieRoller : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Texture2D[] strips; // 6 per angle, angle-major, see DiceMath.StripIndex
    [SerializeField] private float frameTime = 0.03f;
    [SerializeField] private Sprite idleSprite;   // shown before a roll, optional

    // Frames are cut once per strip and reused
    private readonly Dictionary<Texture2D, Sprite[]> frames = new Dictionary<Texture2D, Sprite[]>();

    public IEnumerator Roll(System.Random rng, int face)
    {
        image.enabled = true;
        foreach (Sprite frame in Frames(strips[DiceMath.StripIndex(rng, strips.Length, face)]))
        {
            image.sprite = frame;
            yield return new WaitForSeconds(frameTime);
        }
    }

    public void Idle()
    {
        image.enabled = idleSprite != null;
        image.sprite = idleSprite;
    }

    public void Clear() => image.enabled = false;

    private Sprite[] Frames(Texture2D strip)
    {
        if (frames.TryGetValue(strip, out Sprite[] cut)) return cut;

        int size = strip.height;
        cut = new Sprite[strip.width / size];
        for (int i = 0; i < cut.Length; i++)
            cut[i] = Sprite.Create(strip, new Rect(i * size, 0, size, size), new Vector2(0.5f, 0.5f));
        frames[strip] = cut;
        return cut;
    }

    private void OnDestroy()
    {
        foreach (Sprite[] cut in frames.Values)
            foreach (Sprite frame in cut) Destroy(frame);
    }
}
