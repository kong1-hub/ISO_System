using ISO11820.Core;
using ISO11820.Data;
using ISO11820.Models;
using ISO11820.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ISO11820.Global;

public sealed class AppContext
{
    public static AppContext Instance { get; } = new();
    private AppContext() { }

    public IConfiguration Configuration { get; set; } = null!;
    public DbHelper Db { get; private set; } = null!;
    public SimulationConfig SimulationConfig { get; private set; } = null!;
    public SensorSimulator Simulator { get; private set; } = null!;
    public DaqWorker DaqWorker { get; private set; } = null!;
    public TestController TestController { get; private set; } = null!;
    public ExportService ExportService { get; private set; } = null!;

    public string CurrentOperator { get; set; } = string.Empty;
    public string CurrentRole { get; set; } = string.Empty;

    /// <summary>将配置中的相对路径解析为基于应用程序目录的绝对路径</summary>
    private static string ResolvePath(string? configured, string fallback)
    {
        string path = configured ?? fallback;
        return Path.IsPathRooted(path) ? path : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
    }

    public void Initialize()
    {
        SimulationConfig = new SimulationConfig();
        Configuration.GetSection("Simulation").Bind(SimulationConfig);
        // 从 Hardware 段读取恒功率（仿真 PID 输出计算用）
        SimulationConfig.ConstPower = double.TryParse(Configuration["Hardware:ConstPower"], out double cp) ? cp : 2048.0;

        string dbPath = Configuration["Database:SqlitePath"] ?? "Data\\ISO11820.db";
        string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbPath);
        string? dbDir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dbDir)) Directory.CreateDirectory(dbDir);

        string connStr = $"Data Source={fullPath}";
        Db = new DbHelper(connStr);

        Simulator = new SensorSimulator(SimulationConfig);
        DaqWorker = new DaqWorker(Simulator, SimulationConfig);
        TestController = new TestController(Db, DaqWorker, SimulationConfig);
        ExportService = new ExportService(Configuration);

        // 将配置中的相对路径解析为基于应用程序目录的绝对路径
        string baseDir = ResolvePath(Configuration["FileStorage:BaseDirectory"], "Data\\ISO11820");
        string testDataDir = ResolvePath(Configuration["FileStorage:TestDataDirectory"], "Data\\ISO11820\\TestData");
        string reportDir = ResolvePath(Configuration["Report:OutputDirectory"], "Data\\ISO11820\\Reports");
        try { Directory.CreateDirectory(baseDir); } catch (Exception ex) { Log.Warning(ex, "无法创建基础目录: {Dir}", baseDir); }
        try { Directory.CreateDirectory(testDataDir); } catch (Exception ex) { Log.Warning(ex, "无法创建测试数据目录: {Dir}", testDataDir); }
        try { Directory.CreateDirectory(reportDir); } catch (Exception ex) { Log.Warning(ex, "无法创建报告目录: {Dir}", reportDir); }
    }
}
