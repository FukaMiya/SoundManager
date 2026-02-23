using UnityEngine;
using UnityEditor;
using Early.SoundManager;

public class Demo : MonoBehaviour
{
    [SerializeField] private SoundRegistory soundRegistory;

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

        soundService.PlaySe("Se1",
            new SoundOptions(
                volume: 1f,
                pitch: 1f
            )
        );
    }

    public void PlaySe2()
    {
        if (!CheckIsPlaying()) return;

        soundService.PlaySe("Se2",
            new SoundOptions(
                volume: 2f,
                pitch: 1f
            )
        );
    }

    public void SwitchBgm1(float fadeDuration)
    {
        if (!CheckIsPlaying()) return;

        soundService.SwitchBgm("Bgm1",
            new SoundOptions(
                volume: 0.5f,
                pitch: 1f
            ),
            new SoundFadingOptions(
                fadeDuration: fadeDuration
            )
        );
    }

    public void SwitchBgm2(float fadeDuration)
    {
        if (!CheckIsPlaying()) return;

        soundService.SwitchBgm("Bgm2",
            new SoundOptions(
                volume: 0.5f,
                pitch: 1f
            ),
            new SoundFadingOptions(
                fadeDuration: fadeDuration
            )
        );
    }

    public void SwitchBgm2()
    {
        if (!CheckIsPlaying()) return;

        soundService.SwitchBgm("Bgm2",
            new SoundOptions(
                volume: 0.5f,
                pitch: 1f
            )
        );
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
        float fadeDuration1 = EditorGUILayout.FloatField("Fade Duration", 1f);
        if (GUILayout.Button("Switch Bgm1 (with fade)"))
        {
            demo.SwitchBgm1(fadeDuration1);
        }

        float fadeDuration2 = EditorGUILayout.FloatField("Fade Duration", 1f);
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