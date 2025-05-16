using System;
using System.Threading;

namespace Async
{
    public class Cooldown
    {
        public bool IsCooldown { get; private set; }

        public event Action OnCooldownEnded;
        
        public async void StartCooldown(float time)
        {
            IsCooldown = true;

            CancellationToken token = AsyncUtils.Instance.GetCancellationToken();

            await AsyncUtils.Instance.Wait(time);

            if (token.IsCancellationRequested) return;

            IsCooldown = false;
            
            OnCooldownEnded?.Invoke();
        }
    }
}