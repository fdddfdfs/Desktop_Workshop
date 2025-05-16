using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class OnDisableCancellationToken: MonoBehaviour
{
    private static OnDisableCancellationToken _instance;

    private static CancellationTokenSource _cancellationTokenSource = new();
    
    public static CancellationToken Token
    {
        get
        {
            if (_instance == null)
            {
                var onDisableCancellationToken = new GameObject().AddComponent<OnDisableCancellationToken>();
                DontDestroyOnLoad(onDisableCancellationToken.gameObject);
                _instance = onDisableCancellationToken;
            }

            return _cancellationTokenSource.Token;
        }
    }

    public static Action<Task> EmptyTask => _ => { };

    private void OnEnable()
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
        }
    }

    private void OnDisable()
    {
        _cancellationTokenSource.Cancel();
    }
}