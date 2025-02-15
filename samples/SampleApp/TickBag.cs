namespace SampleApp;

public class TickBag
{
    private int _currentTick = 0;

    public event EventHandler<int>? OnTick;

    public int CurrentTick => _currentTick;

    public void Increment()
    {
        Interlocked.Increment(ref _currentTick);
        OnTick?.Invoke(this, _currentTick);
    }
}
