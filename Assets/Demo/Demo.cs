using UnityEngine;
using UnityEditor;
using Early.SoundManager;

public sealed class Demo : MonoBehaviour
{
    [SerializeField] private SoundRegistory soundRegistory;
    [SerializeField] private SerializableSoundOptions defaultSoundOptions = new ()
    {
        Volume = 1f,
        Pitch = 1f,
        Spatialize = false,
        Position = default,
        RolloffMode = AudioRolloffMode.Logarithmic,
        MinDistance = 1f,
        MaxDistance = 500f
    };

    private SoundManager soundService;
    public IBgmHandle CurrentBgm { get; private set; }

    private void Start()
    {
        soundService = new SoundManager(soundRegistory);
    }

    private void Update()
    {
        soundService.Tick();
    }

    public void PlaySe1()
    {
        soundService.PlaySe("Se1", defaultSoundOptions);
    }

    public void PlaySe2()
    {
        soundService.PlaySe("Se2", defaultSoundOptions);
    }

    public void SwitchBgm1(float fadeDuration)
    {
        CurrentBgm = soundService.SwitchBgm(
            "Bgm1",
            defaultSoundOptions,
            new SoundFadingOptions(
                fadeDuration: fadeDuration
            )
        );
    }

    public void SwitchBgm2(float fadeDuration)
    {
        CurrentBgm = soundService.SwitchBgm("Bgm2",
            defaultSoundOptions,
            new SoundFadingOptions(
                fadeDuration: fadeDuration
            )
        );
    }

    public void SwitchBgm2()
    {
        CurrentBgm = soundService.SwitchBgm("Bgm2", defaultSoundOptions.WithVolumeAndPitch(0.5f, 1f));
    }

    public void ApplyVolumeSettings(float masterVolume, float seVolume, float bgmVolume)
    {
        soundService.SetMasterVolume(masterVolume);
        soundService.SetSeVolume(seVolume);
        soundService.SetBgmVolume(bgmVolume);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Demo))]
public class DemoEditor : Editor
{
    private Demo demo;
    private bool isPlaying => Application.isPlaying;
    private float fadeDuration1 = 1f;
    private float bgmFadeDuration = 1f;
    private float masterVolume = 1f;
    private float seVolume = 1f;
    private float bgmVolume = 1f;

    private void OnEnable()
    {
        demo = (Demo)target;
        EditorApplication.update += () =>
        {
            if (isPlaying) Repaint();
        };
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (!isPlaying)
        {
            GUILayout.Space(10);
            GUILayout.Label("Enter Play Mode to use the demo controls.", EditorStyles.helpBox);
            return;
        }

        GUILayout.Space(10);
        GUILayout.Label("Demo Controls", EditorStyles.boldLabel);

        GUILayout.Space(20);
        GUILayout.Label("Sound Effects", EditorStyles.boldLabel);
        if (GUILayout.Button("Play Se1"))
        {
            demo.PlaySe1();
        }

        if (GUILayout.Button("Play Se2"))
        {
            demo.PlaySe2();
        }

        GUILayout.Space(20);
        GUILayout.Label("Background Music", EditorStyles.boldLabel);
        GUILayout.Label($"BGM: {(demo.CurrentBgm?.IsPlaying == true ? "Playing" : "Stopped")}");
        GUILayout.Label($"BGM Volume: {demo.CurrentBgm?.Volume:F2}");
        if (GUILayout.Button("Pause BGM"))
        {
            demo.CurrentBgm?.Pause();   
        }
        if (GUILayout.Button("Resume BGM (fade)"))
        {
            demo.CurrentBgm?.Resume(new SoundFadingOptions(1f));   
        }

        bgmFadeDuration = EditorGUILayout.FloatField("Fade Duration", bgmFadeDuration);
        if (GUILayout.Button("Switch Bgm1 (with fade)"))
        {
            demo.SwitchBgm1(fadeDuration1);
        }
        if (GUILayout.Button("Switch Bgm2 (with fade)"))
        {
            demo.SwitchBgm2(bgmFadeDuration);
        }

        if (GUILayout.Button("Switch Bgm2 (no fade)"))
        {
            demo.SwitchBgm2();
        }

        GUILayout.Space(20);
        GUILayout.Label("Volume Settings", EditorStyles.boldLabel);
        EditorGUI.BeginChangeCheck();
        masterVolume = EditorGUILayout.Slider("Master Volume", masterVolume, 0f, 1f);
        seVolume = EditorGUILayout.Slider("SE Volume", seVolume, 0f, 1f);
        bgmVolume = EditorGUILayout.Slider("BGM Volume", bgmVolume, 0f, 1f);
        if (EditorGUI.EndChangeCheck())
        {
            demo.ApplyVolumeSettings(masterVolume, seVolume, bgmVolume);
        }
    }
}
#endif