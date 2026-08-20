using UnityEngine;
using System;

public enum PlayState
{
    None,
    Playing,
    Pause
}

public class GameManager : MonoBehaviour, IRegistryAdder
{
    [Header("Variable")]
    [SerializeField]private PlayState currentPlayState;
    SoundManager soundManager;

    public event Action<PlayState> OnPlayStateChanged;
    public event Action OnTimeScaleChanged;
    public float TimeScale
    {
        get
        {
            return Time.timeScale;
        }

        set
        {
            Time.timeScale = value;

            bool flow = Time.timeScale >= 0.5f;
            Cursor.lockState = flow ? CursorLockMode.Locked : CursorLockMode.Confined;
            Cursor.visible = !flow;
            OnTimeScaleChanged?.Invoke();
        }
    }

    public bool IsPlaying => currentPlayState == PlayState.Playing;

    private void Awake()
    {
        AddRegistry();
        SetCurrentState(PlayState.Pause);
    }

    void Start()
    {
        soundManager = StaticRegistry.Find<SoundManager>();
        soundManager.PlayBGM(soundManager.GetClip("BGM_DejaVu"), transform.position, transform.rotation);
    }

    public void AddRegistry()
    {
        StaticRegistry.Add(this);
    }

    public PlayState GetCurrentState()
    {
        return currentPlayState;
    }

    private void SetCurrentState(PlayState change)
    {
        currentPlayState = change;
        OnPlayStateChanged?.Invoke(currentPlayState);
    }

    public void Pause()
    {
        TimeScale = 0f;
        SetCurrentState(PlayState.Pause);
    }
    
    public void Resume()
    {
        TimeScale = 1f;
        SetCurrentState(PlayState.Playing);
    }
}
