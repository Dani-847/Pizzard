using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(SpriteRenderer))]
public class SyncSpriteToImage : MonoBehaviour
{
    private Image uiImage;
    private SpriteRenderer dummyRenderer;

    void Awake()
    {
        uiImage = GetComponent<Image>();
        dummyRenderer = GetComponent<SpriteRenderer>();
        // Disable the SpriteRenderer so it doesn't actually draw in the game world
        // (the UI Image will do the drawing).
        dummyRenderer.enabled = false;
    }

    void Update()
    {
        if (dummyRenderer.sprite != null && uiImage.sprite != dummyRenderer.sprite)
        {
            uiImage.sprite = dummyRenderer.sprite;
        }
    }
}
