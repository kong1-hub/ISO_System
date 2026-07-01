using ISO11820.Models;

namespace ISO11820.Services;

public enum TestState
{
    Idle,
    Preparing,
    Ready,
    Recording,
    Complete
}

public class SensorSimulator
{
    private readonly SimulationConfig _config;
    private readonly Random _rng = new();

    public SensorSimulator(SimulationConfig config)
    {
        _config = config;
    }

    public Dictionary<string, double> Update(Dictionary<string, double> current, TestState state)
    {
        var result = new Dictionary<string, double>(current);
        double tf1 = current.GetValueOrDefault("TF1", _config.InitialFurnaceTemp);
        double tf2 = current.GetValueOrDefault("TF2", _config.InitialFurnaceTemp);
        double ts = current.GetValueOrDefault("TS", _config.InitialFurnaceTemp * 0.25);
        double tc = current.GetValueOrDefault("TC", _config.InitialFurnaceTemp * 0.2);
        double tcal = current.GetValueOrDefault("TCal", _config.InitialFurnaceTemp);

        double noise() => (_rng.NextDouble() * 2 - 1) * _config.TempFluctuation;

        if (state == TestState.Idle)
        {
            // Idle: 保持初始温度不变，不模拟降温（等用户操作）
            return result;
        }

        bool isRecording = (state == TestState.Recording || state == TestState.Complete);
        double target = _config.TargetFurnaceTemp;
        double stableThreshold = _config.StableThreshold;

        if (tf1 < target - stableThreshold)
        {
            result["TF1"] = tf1 + _config.HeatingRatePerSecond * 0.8 + noise();
            result["TF2"] = tf2 + _config.HeatingRatePerSecond * 0.8 + noise();
        }
        else
        {
            result["TF1"] = target + noise();
            result["TF2"] = target + noise();
        }

        if (isRecording)
        {
            double surfaceTarget = Math.Min(result["TF1"] * 0.95, 800);
            result["TS"] = ts + (surfaceTarget - ts) * 0.02 + noise();
            double centerTarget = Math.Min(result["TF1"] * 0.85, 750);
            result["TC"] = tc + (centerTarget - tc) * 0.01 + noise();
        }
        else
        {
            result["TS"] = result["TF1"] * 0.3 + noise();
            result["TC"] = result["TF1"] * 0.25 + noise();
        }

        result["TCal"] = result["TF1"] + noise() * 2;
        return result;
    }

    public Dictionary<string, double> GetInitialTemperatures()
    {
        double t = _config.InitialFurnaceTemp;
        var rng = new Random();
        double noise() => (rng.NextDouble() * 2 - 1) * _config.TempFluctuation;
        return new Dictionary<string, double>
        {
            ["TF1"] = t + noise(),
            ["TF2"] = t + noise(),
            ["TS"] = t * 0.3 + noise(),
            ["TC"] = t * 0.25 + noise(),
            ["TCal"] = t + noise() * 2
        };
    }
}
