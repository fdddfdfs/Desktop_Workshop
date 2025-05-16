using System.Threading;
using TMPro;
using UnityEngine;

public class SubtitlesView : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;

    private CancellationTokenSource _cancellationTokenSource;
    
    public async void SetTextForTime(string text, float time)
    {
        _text.text = text;
        
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = 
            CancellationTokenSource.CreateLinkedTokenSource(AsyncUtils.Instance.GetCancellationToken());
        CancellationToken token = _cancellationTokenSource.Token;
        
        await AsyncUtils.Instance.Wait(time, token);

        if (token.IsCancellationRequested) return;
        
        _text.text = "";
    }
}