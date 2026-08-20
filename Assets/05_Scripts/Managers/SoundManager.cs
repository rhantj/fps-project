using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour, IRegistryAdder
{
    [SerializeField] private List<AudioClip> preloadClips = new();
    Dictionary<string, AudioClip> clips = new();
    AudioSource currentAudio;

    public float MasterVolume
    {
        get { return AudioListener.volume; }
        set { AudioListener.volume = value; }
    }
    public float SFXVolume { get; set; } = 1.0f;
    public float BGMVolume { get; set; } = 1.0f;

    public AudioSource CurrentAudio => currentAudio;

    private void Awake()
    {
        AddRegistry();
        foreach(var c in preloadClips)
        {
            clips[c.name] = c;
        }
    }

    public AudioClip GetClip(string name)
    {
        return clips.TryGetValue(name, out var clip) ? clip : null;
    }


    public void PlaySound(AudioClip clip, Vector3 pos, Quaternion rot)
    {
        var speaker = ObjectPoolManager.Instance.Spawn(PoolId.SoundPlayer, pos, rot);
        if (speaker == null) return;

        var audioSource = speaker.GetComponent<AudioSource>();

        audioSource.clip = clip;
        audioSource.volume = SFXVolume;
        audioSource.spatialBlend = 1.0f; // 3D sound
        audioSource.loop = false;        // BGM에 쓰였던 인스턴스가 재사용될 수 있다
        audioSource.Play();

        StartCoroutine(Co_DespawnSoundPlayer(audioSource));
    }

    public void PlayBGM(AudioClip clip, Vector3 pos, Quaternion rot)
    {
        if (currentAudio != null)
        {
            // Stop만 하면 스피커가 풀로 돌아가지 않아 호출할 때마다 인스턴스가 샌다
            currentAudio.Stop();
            ObjectPoolManager.Instance.Despawn(currentAudio.gameObject);
            currentAudio = null;
        }

        var bgmPlayer = ObjectPoolManager.Instance.Spawn(PoolId.SoundPlayer, pos, rot);
        if (bgmPlayer == null) return;

        var audioSource = bgmPlayer.GetComponent<AudioSource>();
        currentAudio = audioSource;

        audioSource.volume = BGMVolume;
        audioSource.clip = clip;
        audioSource.spatialBlend = 0.0f; // 2D sound
        audioSource.loop = true;
        audioSource.Play();
    }

    IEnumerator Co_DespawnSoundPlayer(AudioSource aus)
    {
        while(aus.isPlaying)
            yield return null;

        ObjectPoolManager.Instance.Despawn(aus.gameObject);
    }

    public void AddRegistry()
    {
        StaticRegistry.Add<SoundManager>(this);
    }
}