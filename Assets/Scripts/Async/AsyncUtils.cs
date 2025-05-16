using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public sealed class AsyncUtils : MonoBehaviour, ICancellationTokenProvider
{
    private CancellationTokenSource _source;
    private static AsyncUtils _instance;
    private static float _timeScale = 1f;

    public static AsyncUtils Instance
    {
        get
        {
            if (!_instance)
            {
                var gameObject = new GameObject(nameof(AsyncUtils));
                DontDestroyOnLoad(gameObject);
                _instance = gameObject.AddComponent<AsyncUtils>();
                _instance._source = new CancellationTokenSource();
            }

            return _instance;
        }
    }

    public static float TimeScale
    {
        get => _timeScale;
        set => _timeScale = Mathf.Clamp(value, 0f, 2f);
    }

    public void CancelAllActions()
    {
        _source.Cancel();
        _source = new CancellationTokenSource();
    }

    public async Task WaitUntilNextFrame()
    {
        await Task.Yield();
    }

    public async Task WaitUntilNextFixedUpdate()
    {
        await Wait(Time.fixedDeltaTime);
    }
    
    public async Task Wait(float time, bool unscaledTime = false)
    {
        float currentTime = Time.time;
        float targetTime = currentTime + time;
        CancellationToken token = GetCancellationToken();
        
        while (currentTime < targetTime)
        {
            currentTime += unscaledTime ? Time.deltaTime : Time.deltaTime * TimeScale;
            await Task.Yield();
            
            if (token.IsCancellationRequested) return;
        }
    }

    public async Task Wait(float time, CancellationToken cancellationToken, bool unscaledTime = false)
    {
        float currentTime = Time.time;
        float targetTime = currentTime + time;
        while (currentTime < targetTime)
        {
            currentTime += unscaledTime ? Time.deltaTime : Time.deltaTime * TimeScale;
            await Task.Yield();

            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    public async Task Wait(
        float time,
        CancellationToken cancellationToken,
        Action<float> onProgress,
        bool unscaledTime = false)
    {
        float currentTime = 0;
        
        while (currentTime < time)
        {
            currentTime += unscaledTime ? Time.deltaTime : Time.deltaTime * TimeScale;
            onProgress?.Invoke(Mathf.Clamp(currentTime / time, 0f, 1f));
            await Task.Yield();

            if (cancellationToken.IsCancellationRequested) return;
        }
    }
    
    public async Task Wait(
        float time,
        Action<float> onProgress,
        CancellationToken token = default,
        bool unscaledTime = false)
    {
        float currentTime = 0;

        if (token == CancellationToken.None)
        {
            token = GetCancellationToken();
        }
        
        while (currentTime < time)
        {
            currentTime += unscaledTime ? Time.deltaTime : Time.deltaTime * TimeScale;
            onProgress?.Invoke(Mathf.Clamp(currentTime / time, 0f, 1f));
            await Task.Yield();
            
            if (token.IsCancellationRequested) return;
        }
    }
    
    public async Task Wait(int timeMilliseconds, bool unscaledTime = false)
    {
        float currentTime = Time.time;
        float targetTime = currentTime + timeMilliseconds / 1000f;
        CancellationToken token = GetCancellationToken();
        
        while (currentTime < targetTime)
        {
            currentTime += unscaledTime ? Time.deltaTime : Time.deltaTime * TimeScale;
            await Task.Yield();
            
            if (token.IsCancellationRequested) return;
        }
    }

    public async Task Wait(int timeMilliseconds, CancellationToken cancellationToken, bool unscaledTime = false)
    {
        float currentTime = Time.time;
        float targetTime = currentTime + timeMilliseconds / 1000f;
        while (currentTime < targetTime)
        {
            currentTime += unscaledTime ? Time.deltaTime : Time.deltaTime * TimeScale;
            await Task.Yield();

            if (cancellationToken.IsCancellationRequested) return;
        }
    }

    public Action<Task> EmptyTask => _ => { };

    private void OnDisable()
    {
        _source.Cancel();
    }

    public CancellationToken GetCancellationToken()
    {
        return _source.Token;
    }
}