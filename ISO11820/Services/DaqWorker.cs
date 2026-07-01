using ISO11820.Models;

namespace ISO11820.Services;

public class DaqWorker : IDisposable
{
    private System.Windows.Forms.Timer? _timer;
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
    }

    /// <summary>必须在 UI 线程调用，因为 WinForms Timer 需要消息泵</summary>
    public void Start()
    {
        if (_timer == null)
        {
            _timer = new System.Windows.Forms.Timer { Interval = 800 };
            _timer.Tick += OnTick;
        }
        _lastSecondTime = DateTime.Now;
        _timer.Start();
    }

    public void Stop()
    {
        _timer?.Stop();
    }

    public void ResetElapsed()
    {
        ElapsedSeconds = 0;
        _lastSecondTime = DateTime.Now;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        // 仿真模式：更新5通道温度
        if (_config.EnableSimulation)
            _temperatures = _simulator.Update(_temperatures, CurrentState);

        // 记录炉温历史
        _furnaceTempHistory.Add(_temperatures["TF1"]);
        if (_furnaceTempHistory.Count > 600) _furnaceTempHistory.RemoveAt(0);

        // 秒检测（用于 Recording 计时器）
        var now = DateTime.Now;
        if ((now - _lastSecondTime).TotalSeconds >= 1.0)
        {
            _lastSecondTime = now;
            if (CurrentState == TestState.Recording)
                ElapsedSeconds++;
            SecondElapsed?.Invoke();
        }

        // 计算温漂
        double drift = DriftCalculator.CalculateDrift(_furnaceTempHistory);

        // 广播数据到 UI（当前已在 UI 线程，无需 Invoke）
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

    public void Dispose()
    {
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }
}
