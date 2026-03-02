using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// One-shot editor tool that creates AnimatorControllers + AnimationClips
/// for Player, Pblob, and Niggel, then wires them to their prefabs.
/// Run via: Tools/Setup Character Animators
/// </summary>
public class SetupCharacterAnimators : EditorWindow
{
    [MenuItem("Tools/Setup Character Animators")]
    public static void Run()
    {
        AssetDatabase.CreateFolder("Assets/Animations", "Controllers");
        AssetDatabase.CreateFolder("Assets/Animations", "Clips");

        SetupPlayer();
        SetupPblob();
        SetupNiggel();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SetupCharacterAnimators] All done!");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    static Sprite GetSprite(string aseName)
    {
        string path = $"Assets/Sprites/Spritesforfinalv1/{aseName}";
        var all = AssetDatabase.LoadAllAssetsAtPath(path);
        foreach (var a in all) if (a is Sprite s) return s;
        return null;
    }

    static AnimationClip MakeClip(string clipName, Sprite sprite)
    {
        var clip = new AnimationClip { name = clipName, frameRate = 12 };

        var spriteBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keyframes = new ObjectReferenceKeyframe[]
        {
            new ObjectReferenceKeyframe { time = 0f, value = sprite },
            new ObjectReferenceKeyframe { time = 1f / 12f, value = sprite }
        };
        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        string clipPath = $"Assets/Animations/Clips/{clipName}.anim";
        if (!System.IO.File.Exists(Application.dataPath + "/../" + clipPath))
            AssetDatabase.CreateAsset(clip, clipPath);
        else
        {
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            AnimationUtility.SetObjectReferenceCurve(existing, spriteBinding, keyframes);
            EditorUtility.SetDirty(existing);
            clip = existing;
        }
        return clip;
    }

    static AnimatorController MakeController(string name, AnimationClip idle, AnimationClip active, string paramName)
    {
        string path = $"Assets/Animations/Controllers/{name}.controller";
        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);

        ctrl.AddParameter(paramName, AnimatorControllerParameterType.Bool);

        var root = ctrl.layers[0].stateMachine;

        var idleState  = root.AddState("Idle");
        idleState.motion = idle;
        root.defaultState = idleState;

        var activeState = root.AddState(active.name.Contains("attack") || active.name.Contains("Attack") ? "Attack" : "Walk");
        activeState.motion = active;

        // Idle → active
        var t1 = idleState.AddTransition(activeState);
        t1.AddCondition(AnimatorConditionMode.If, 0, paramName);
        t1.hasExitTime = false;
        t1.duration = 0f;

        // active → Idle
        var t2 = activeState.AddTransition(idleState);
        t2.AddCondition(AnimatorConditionMode.IfNot, 0, paramName);
        t2.hasExitTime = false;
        t2.duration = 0f;

        EditorUtility.SetDirty(ctrl);
        return ctrl;
    }

    static void AddAnimatorToPrefab(string prefabPath, AnimatorController ctrl)
    {
        var go = PrefabUtility.LoadPrefabContents(prefabPath);
        var anim = go.GetComponent<Animator>();
        if (anim == null) anim = go.AddComponent<Animator>();
        anim.runtimeAnimatorController = ctrl;
        PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        PrefabUtility.UnloadPrefabContents(go);
        Debug.Log($"[SetupCharacterAnimators] ✓ Animator added to {prefabPath}");
    }

    // ── per-character setup ──────────────────────────────────────────────────

    static void SetupPlayer()
    {
        var idle   = MakeClip("Player_Idle",   GetSprite("player.ase"));
        var walk   = MakeClip("Player_Walk",   GetSprite("playergoingsideawaysanimation.ase"));
        var ctrl   = MakeController("PlayerAnimator", idle, walk, "isMoving");
        AddAnimatorToPrefab("Assets/Prefabs/Base/Player.prefab", ctrl);
    }

    static void SetupPblob()
    {
        var idle   = MakeClip("Pblob_Idle",    GetSprite("pblob.ase"));
        var attack = MakeClip("Pblob_Attack",  GetSprite("pablobwhenattacking.ase"));
        var ctrl   = MakeController("PblobAnimator", idle, attack, "isAttacking");
        AddAnimatorToPrefab("Assets/Prefabs/Base/Pblob.prefab", ctrl);
    }

    static void SetupNiggel()
    {
        var idle   = MakeClip("Niggel_Idle",   GetSprite("niggel.ase"));
        var walk   = MakeClip("Niggel_Walk",   GetSprite("niggleworthingtonmovingsideaways.ase"));
        var ctrl   = MakeController("NiggelAnimator", idle, walk, "isMoving");
        AddAnimatorToPrefab("Assets/Prefabs/Base/Niggel.prefab", ctrl);
    }
}
