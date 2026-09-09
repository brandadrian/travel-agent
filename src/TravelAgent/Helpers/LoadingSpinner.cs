using System;
using System.Threading;
using System.Threading.Tasks;

namespace TravelAgent.Helpers;

public sealed class LoadingSpinner : IDisposable
{
    private readonly string _message;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _spinnerTask;
    private bool _disposed;

    public LoadingSpinner(string message = "Thinking...")
    {
        _message = message;
        _spinnerTask = Task.Run(() => SpinAsync(_cts.Token));
    }

    private async Task SpinAsync(CancellationToken token)
    {
        var animation = new[] { "|", "/", "-", "\\" };
        int counter = 0;
        try
        {
            while (!token.IsCancellationRequested)
            {
                var symbol = animation[counter % animation.Length];
                Console.Write($"\r{_message} {symbol}");
                counter++;
                await Task.Delay(100, token);
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    public void Stop()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        try
        {
            _spinnerTask.Wait();
        }
        catch
        {
        }

        var clearLength = _message.Length + 10;
        Console.Write("\r" + new string(' ', clearLength) + "\r");
    }

    public void Dispose()
    {
        Stop();
        _cts.Dispose();
    }
}
