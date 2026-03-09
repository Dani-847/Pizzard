using UnityEngine;

namespace Pizzard.Core
{
    /// <summary>
    /// Attach to a SpriteRenderer to display a full-screen blurred background.
    /// Scales the sprite to fill the camera view and applies the Pizzard/SpriteGaussianBlur shader.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BlurredBackground : MonoBehaviour
    {
        [SerializeField] private float blurSize = 5f;
        [SerializeField, Range(1, 4)] private int blurIterations = 3;
        [SerializeField] private Color tint = Color.white;
        [SerializeField] private float scaleMultiplier = 1f;

        private SpriteRenderer sr;
        private Material blurMat;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            sr.sortingOrder = -1000; // behind everything

            Shader blurShader = Shader.Find("Pizzard/SpriteGaussianBlur");
            if (blurShader != null)
            {
                blurMat = new Material(blurShader);
                blurMat.SetFloat("_BlurSize", blurSize);
                blurMat.SetFloat("_Iterations", blurIterations);
                blurMat.SetColor("_Color", tint);
                sr.material = blurMat;
            }

            ScaleToFillCamera();
        }

        private void ScaleToFillCamera()
        {
            Camera cam = Camera.main;
            if (cam == null || sr.sprite == null) return;

            float worldHeight = cam.orthographicSize * 2f;
            float worldWidth = worldHeight * cam.aspect;

            float spriteWidth = sr.sprite.bounds.size.x;
            float spriteHeight = sr.sprite.bounds.size.y;

            float scaleX = worldWidth / spriteWidth;
            float scaleY = worldHeight / spriteHeight;
            float scale = Mathf.Max(scaleX, scaleY) * scaleMultiplier;

            transform.localScale = new Vector3(scale, scale, 1f);
            transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 10f);
        }
    }
}
