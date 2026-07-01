using ISO11820.Models;

namespace ISO11820.Services;

public class DaqWorker : IDisposable
{
    private readonly System.Timers.Timer _timer;
    private readonly SensorSimulator _simulator;
    private readonly SimulationConfig _config;
    private Dictionary<string, double> _temperatures;
    private readonly List<double> _furnaceTempHistory = new();
    private readonly List<MasterMessage> _pendingMessages = new();
    private DateTime _lastSecondTime = DateTime.Now;

    public event EventHandler<DataBroadcastEventArgs>? DataBroadcast;
    public event Action? SecondElapsed;

    public Dictionary<string, double> Temperatures => _temperatures;
    public TestState CurrentState { get; set; } = TestState.Idle;
    public int ElapsedSeconds { get; private set; }

    public DaqWorker(SensorSimulator simulator, SimulationConfig config)
    {
        _simulator = simulator;
        _config = config;
        _temperatures = simulator.GetInitialTemperatures();
        _timer = new System.Timers.Timer(200); // 200ms faster tick for smoother UI
        _timer.Elapsed += OnTick;
        _timer.AutoReset = true;
    }

    public void Start()
    {
        _lastSecondTime = DateTime.Now;
        _timer.Start();
    }

    public void Stop() => _timer.Stop();
    public void ResetElapsed()
    {
        ElapsedSeconds = 0;
        _lastSecondTime = DateTime.Now;
    }

    private void OnTick(object? sender, System.Timers.ElapsedEventArgs e)
    {
        try
        {
            if (_config.EnableSimulation)
                _temperatures = _simulator.Update(_temperatures, CurrentState);

            _furnaceTempHistory.Add(_temperatures["TF1"]);
            if (_furnaceTempHistory.Count > 600) _furnaceTempHistory.RemoveAt(0);

            // DateTime-based second detection (more reliable than accumulated ms)
            var now = DateTime.Now;
            if ((now - _lastSecondTime).TotalSeconds >= 1.0)
            {
                _lastSecondTime = now;
                if (CurrentState == TestState.Recording)
                    ElapsedSeconds++;
                SecondElapsed?.Invoke();
            }

            double drift = DriftCalculator.CalculateDrift(_furnaceTempHistory);

            var args = new DataBroadcastEventArgs
            {
                Temperatures = new Dictionary<string, double>(_temperatures),
                CurrentState = CurrentState.ToString(),
                ElapsedSeconds = ElapsedSeconds,
                IsStable = false,
                Drift = drift,
                Messages = new List<MasterMessage>(_pendingMessages)
            };

            DataBroadcast?.Invoke(this, args);
            _pendingMessages.Clear();
        }
        catch (Exception ex)
        {
            // Prevent silent timer death; log the error
            System.Diagnostics.Debug.WriteLine($"DaqWorker.OnTick error: {ex.Message}");
        }
    }

    public void AddMessage(string message)
    {
        _pendingMessages.Add(new MasterMessage
        {
            Time = DateTime.Now.ToString("HH:mm:ss"),
            Message = message
        });
    }

    public List<double> GetFurnaceTempHistory() => new(_furnaceTempHistory);

    public double GetCurrentDrift() => DriftCalculator.CalculateDrift(_furnaceTempHistory);

    public void Dispose() => _timer.Dispose();
}
