using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine.UI;

public static class SetupSpeechBubble
{
    [MenuItem("Tools/Pizzard/Setup Speech Bubble")]
    public static void Execute()
    {
        // Find DialogPanel
        GameObject dialogPanel = GameObject.Find("DialogPanel");
        if (dialogPanel == null)
        {
            Debug.LogError("DialogPanel not found in scene!");
            return;
        }

        // Find speech_bubble clip inside the .ase asset
        string assetPath = "Assets/Sprites/Static/speech_bubble.ase";
        Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        AnimationClip clip = null;
        foreach (Object o in subAssets)
        {
            if (o is AnimationClip && o.name == "speech_bubble") // default name or find first clip
            {
                clip = (AnimationClip)o;
            }
        }
        
        if (clip == null)
        {
            // Just grab the first clip available
            foreach (Object o in subAssets)
            {
                if (o is AnimationClip && !o.name.StartsWith("__preview__")) 
                {
                    clip = (AnimationClip)o;
                    break;
                }
            }
        }

        if (clip == null)
        {
            Debug.LogError("Could not find AnimationClip in " + assetPath);
            return;
        }

        Debug.Log("Found clip: " + clip.name);

        // Create controller
        string controllerPath = "Assets/Animations/SpeechBubble.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddMotion(clip);

        // Add variables to DialogPanel
        Animator animator = dialogPanel.GetComponent<Animator>();
        if (animator == null) animator = dialogPanel.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;

        SpriteRenderer sr = dialogPanel.GetComponent<SpriteRenderer>();
        if (sr == null) sr = dialogPanel.AddComponent<SpriteRenderer>();
        sr.enabled = false; // Hidden, only for animation sampling

        SyncSpriteToImage sync = dialogPanel.GetComponent<SyncSpriteToImage>();
        if (sync == null) sync = dialogPanel.AddComponent<SyncSpriteToImage>();

        // Set layout: remove source image to prevent overriding our Sprite, or set alpha
        Image img = dialogPanel.GetComponent<Image>();
        if (img != null)
        {
            img.color = Color.white;
        }

        // Make the dialog panel bigger and centered
        RectTransform rt = dialogPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(0, -200); // bottom center-ish
            rt.sizeDelta = new Vector2(800, 300);
        }

        // Fix DialogText layout
        GameObject dialogTextObj = GameObject.Find("DialogText");
        if (dialogTextObj != null)
        {
            RectTransform trt = dialogTextObj.GetComponent<RectTransform>();
            if (trt != null)
            {
                // Fill the bubble with some padding
                trt.anchorMin = new Vector2(0, 0);
                trt.anchorMax = new Vector2(1, 1);
                trt.offsetMin = new Vector2(50, 50); // Left, Bottom padding
                trt.offsetMax = new Vector2(-50, -50); // Right, Top padding
                
                var tmp = dialogTextObj.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmp != null)
                {
                    tmp.alignment = TMPro.TextAlignmentOptions.Center;
                    tmp.color = Color.black; // Dark text on light bubble usually
                }
            }
        }

        Debug.Log("Speech Bubble setup complete!");
    }
}
