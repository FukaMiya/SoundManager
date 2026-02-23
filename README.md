# Early Sound Manager
## Overview
- オブジェクトプールによる最適化
- BGMのクロスフェード
- SpatialSound対応
- ISoundHandleによる個別操作
- ScriptableObjectによる文字列キー管理
- VContainerでのDI可能

## Usage
### Initialize
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
//// 単純な再生
// 文字列キーとAudioClipの両方に対応
soundService.PlaySe(clip);
soundService.PlaySe("key");

//// 再生設定
soundService.PlaySe(
    "key",
    new SoundOption(
        volume: 1.0f,
        pitch: 1.0f,
        spatialize: false,
    )
);

//// SpatialBlend
// spatializeを有効にし、再生位置を指定することでSpatialBlendを有効にします
soundService.PlaySe(
    "key",
    new SoundOption(
        volume: 1.0f,
        pitch: 1.0f,
        spatialize: true,
        rolloffMode: AudioRolloffMode.Logarithmic,
        position: new Vector3(0, 0, 0)
        minDistance: 1.0f,
        maxDistance: 500.0f,
    )
);

//// ハンドルによる操作
var handle = soundService.PlaySe("key");
handle.SetVolume(0.5f);
handle.Pause();
handle.Resume();
handle.Stop();
```

### Play BGM
BGMは自動的にループします。同時に再生可能な音源は一つですが、クロスフェードによるスムーズな遷移が可能です。
```csharp
//// 単純な再生
soundService.PlayBgm(clip);
soundService.PlayBgm("key");

//// 再生設定(Spatial Blend)
soundService.PlayBgm(
    "key",
    new SoundOption(
        volume: 1.0f,
        pitch: 1.0f,
        spatialize: true,
        rolloffMode: AudioRolloffMode.Logarithmic,
        position: new Vector3(0, 0, 0)
        minDistance: 1.0f,
        maxDistance: 500.0f,
    )
);

//// 切り替え
// SoundFadingOptionsを渡さない場合、即座に切り替わります。
soundService.SwitchBgm("key");
soundService.SwitchBgm(
    "key",
    new SoundFadingOptions(
        fadeDuration: 1.0f
    )
);

//// ハンドルによる操作
// BGMは完全に停止することはできません。一時停止するか、別のBGMに切り替える必要があります。
var handle = soundService.PlayBgm("key");
handle.SetVolume(0.5f);
handle.Pause();
handle.Resume();
```

## Notice
SoundFadingOptionsにCancellationTokenを渡すことができますが、現状フェードのキャンセルはできません。

## License
[MIT License](LICENSE)