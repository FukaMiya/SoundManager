using UnityEngine;
using UnityEditor;
using Early.SoundManager;

public class Demo : MonoBehaviour
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
        if (!CheckIsPlaying()) return;

        soundService.PlaySe("Se1", defaultSoundOptions);
    }

    public void PlaySe2()
    {
        if (!CheckIsPlaying()) return;

        soundService.PlaySe("Se2", defaultSoundOptions.WithVolume(2f));
    }

    public void SwitchBgm1(float fadeDuration)
    {
        if (!CheckIsPlaying()) return;

        soundService.SwitchBgm(
            "Bgm1",
            defaultSoundOptions.WithVolume(0.5f),
            new SoundFadingOptions(
                fadeDuration: fadeDuration
            )
        );
    }

    public void SwitchBgm2(float fadeDuration)
    {
        if (!CheckIsPlaying()) return;

        soundService.SwitchBgm("Bgm2",
            defaultSoundOptions.WithVolumeAndPitch(0.5f, 1f),
            new SoundFadingOptions(
                fadeDuration: fadeDuration
            )
        );
    }

    public void SwitchBgm2()
    {
        if (!CheckIsPlaying()) return;

        soundService.SwitchBgm("Bgm2", defaultSoundOptions.WithVolumeAndPitch(0.5f, 1f));
    }

    private bool CheckIsPlaying()
    {
        if (Application.isPlaying)
        {
            return true;
        }
        else
        {
            Debug.Log("Run in Play Mode");
            return false;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Demo))]
public class DemoEditor : Editor
{
    private Demo demo;
    private float fadeDuration1 = 1f;
    private float fadeDuration2 = 1f;

    private void OnEnable()
    {
        demo = (Demo)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);
        GUILayout.Label("Demo Controls", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Sound Effects", EditorStyles.boldLabel);
        if (GUILayout.Button("Play Se1"))
        {
            demo.PlaySe1();
        }

        if (GUILayout.Button("Play Se2"))
        {
            demo.PlaySe2();
        }
        GUILayout.Space(10);

        GUILayout.Label("Background Music", EditorStyles.boldLabel);
        fadeDuration1 = EditorGUILayout.FloatField("Fade Duration", fadeDuration1);
        if (GUILayout.Button("Switch Bgm1 (with fade)"))
        {
            demo.SwitchBgm1(fadeDuration1);
        }

        fadeDuration2 = EditorGUILayout.FloatField("Fade Duration", fadeDuration2);
        if (GUILayout.Button("Switch Bgm2 (with fade)"))
        {
            demo.SwitchBgm2(fadeDuration2);
        }

        if (GUILayout.Button("Switch Bgm2 (no fade)"))
        {
            demo.SwitchBgm2();
        }
    }
}
#endif