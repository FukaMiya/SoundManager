# Early Sound Manager
## Overview
- オブジェクトプールによる最適化
- BGMのクロスフェード
- SpatialSound対応
- マスター音量、SE音量、BGM音量をそれぞれ管理可能
- ISoundHandleによる個別操作
- ScriptableObjectによる文字列キー管理
- VContainerでのDI可能

## Usage
### Initialize
プレイヤーループに組み込むためにUpdate()等で呼び出す必要があります。VContainerを導入している場合、VContainer.Unity.ITickableと互換性があります。
```csharp
public SoundRegistory soundRegistory;
private SoundManager soundService;

void Start()
{
    soundService = new SoundManager(soundRegistory);
}

void Update()
{
    soundService.Tick();
}
```

### Play SE
SEは再生終了後、自動的に回収されます。
```csharp
// 再生
soundService.PlaySe(clip);
soundService.PlaySe("key");
soundService.PlaySe("key", soundOptions);
var handle = soundService.PlaySe("key");

// ハンドルによる操作
handle.Pause();
handle.Resume();
handle.Stop();
```

### Play BGM
BGMは自動的にループします。同時に再生可能な音源は一つですが、クロスフェードによるスムーズな遷移が可能です。
```csharp
// 再生
soundService.PlayBgm(clip);
var handle = soundService.PlayBgm("key");

// 切り替え
// SoundFadingOptionsを渡さない場合、即座に切り替わります。
soundService.SwitchBgm("key");
soundService.SwitchBgm(
    "key",
    new SoundFadingOptions(
        fadeDuration: 1.0f
    )
);

// ハンドルによる操作
// BGMは完全に停止することはできません。一時停止するか、別のBGMに切り替える必要があります。
var handle = soundService.PlayBgm("key");
handle.SetVolume(0.5f);
handle.Pause();
handle.Resume();
```

### Sound Options
再生時に設定可能なパラメータです。
- `(float) Volume`
- `(float) Pitch`
- `(bool) Spatialize`
- `(Vector3) Position`
- `(AudioRolloffMode) RolloffMode`
- `(float) MinDistance`
- `(float) MaxDistance`

`SerializableSoundOptions`によるシリアライズが可能です。

### Sound Fade Options
Bgmのクロスフェード設定です。`FadeDuration`を設定できます。
CancellationTokenを渡すことができますが、現状フェードのキャンセルはできません。
`SerializableSoundFadeOptions`によるシリアライズが可能です。

## License
[MIT License](LICENSE)