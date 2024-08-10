namespace DellFanControl;

using System;
using System.Management;
using System.Timers;
using System.Windows.Forms;
using DellFanManagement.Interop;
using LibreHardwareMonitor.Hardware;

public enum FanSpeed : int
{
    OFF = 1,
    VERY_LOW = 2,
    LOW = 3,
    MEDIUM = 4,
    HIGH = 5,
}

enum AppState : int
{
    AUTO = 0,
    OFF = 1,
    VERY_LOW = 2,
    LOW = 3,
    MEDIUM = 4,
    HIGH = 5,
}

static class AppStateActions
{
    public static void Act(this AppState state)
    {
        switch (state)
        {
            case AppState.OFF:
                DellFanControl.SetOff();
                break;
            case AppState.VERY_LOW:
                DellFanControl.SetVeryLow();
                break;
            case AppState.LOW:
                DellFanControl.SetLow();
                break;
            case AppState.MEDIUM:
                DellFanControl.SetMedium();
                break;
            case AppState.HIGH:
                DellFanControl.SetHigh();
                break;
            case AppState.AUTO:
                DellFanControl.Auto();
                break;
        }
    }

    public static void SetFanSpeed(this FanSpeed state)
    {
        switch (state)
        {
            case FanSpeed.OFF:
                DellFanControl.SetOff();
                break;
            case FanSpeed.VERY_LOW:
                DellFanControl.SetVeryLow();
                break;
            case FanSpeed.LOW:
                DellFanControl.SetLow();
                break;
            case FanSpeed.MEDIUM:
                DellFanControl.SetMedium();
                break;
            case FanSpeed.HIGH:
                DellFanControl.SetHigh();
                break;
        }
    }
    public static FanSpeed GetHigherFanSpeed(FanSpeed speed1, FanSpeed speed2)
    {
        return (speed1 > speed2) ? speed1 : speed2;
    }
}


static class DellFanControl
{

    private static AppState currentAppState = AppState.AUTO;
    private static System.Timers.Timer? aTimer;

    private static Config appConfig = new(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_config.json"));

    private static DateTime autoLastFanSpeedChangeTime = new(1970, 1, 1);
    private static FanSpeed autoLastFanSpeed = FanSpeed.OFF;

    public static void Auto()
    {

        FanSpeed desiredFanSpeed = appConfig.DefaultSpeed();

        var Charging_status = System.Windows.Forms.SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.Charging;
        if (System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && Charging_status == BatteryChargeStatus.Charging)
        {
            // pluged in and charging 
#if DEBUG
            Console.WriteLine("Charging");
#endif
            desiredFanSpeed = AppStateActions.GetHigherFanSpeed(desiredFanSpeed, appConfig.ChargingSpeed());
        }
        else if (System.Windows.Forms.SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
        {
#if DEBUG
            Console.WriteLine("Pluged in");
#endif
            // pluged in and NOT charging 
            desiredFanSpeed = AppStateActions.GetHigherFanSpeed(desiredFanSpeed, appConfig.PlugedInSpeed());
        }


        // check monitor count
        int monitorCount = 1;

        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Monitor')");
            var monitors = searcher.Get();
            monitorCount = monitors.Count;
        }
        catch (Exception ex)
        {
            File.WriteAllText("error.log", $"An error occurred: {ex.Message}");
        }

#if DEBUG
        Console.WriteLine($"Monitors {monitorCount}");
#endif
        if (monitorCount > 1)
        {
            desiredFanSpeed = appConfig.DockedSpeed();
        }

        Computer computer = new()
        {
            IsCpuEnabled = true
        };
        computer.Open();

        int i = 0;
        float temp = 0.0f;

        foreach (var hardware in computer.Hardware)
        {
            if (hardware.HardwareType == HardwareType.Cpu)
            {
                hardware.Update();
                foreach (var sensor in hardware.Sensors)
                {
                    if (sensor.SensorType == SensorType.Temperature)
                    {
                        if (sensor.Value != null)
                        {
                            i++;
                            temp += (float)sensor.Value;
                        }
                    }
                }
            }
        }

        temp /= i;

#if DEBUG
        Console.WriteLine($"temperature {temp}, speed {appConfig.GetSpeedForTemp(temp)}");
#endif
        desiredFanSpeed = AppStateActions.GetHigherFanSpeed(desiredFanSpeed, appConfig.GetSpeedForTemp(temp));

        DateTime now = DateTime.Now;
        var timeDiffStamp = now - autoLastFanSpeedChangeTime;
        int timeDiff = timeDiffStamp.Seconds;

        if (timeDiff > appConfig.AutoSpoolDownTime() || desiredFanSpeed > autoLastFanSpeed)
        {
            autoLastFanSpeedChangeTime = now;
            autoLastFanSpeed = desiredFanSpeed;
#if DEBUG
            Console.WriteLine($"setting to {desiredFanSpeed}");
#endif

            desiredFanSpeed.SetFanSpeed();
        }

    }

    public static void SetOff()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level0);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    public static void SetVeryLow()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    public static void SetLow()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level1);
    }
    public static void SetMedium()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level2);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level1);
    }
    public static void SetHigh()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level2);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level2);
    }

    [STAThread]
    static void Main()
    {
        try
        {

#if DEBUG
            AllocConsole();
            Console.WriteLine("Debug mode: Console output enabled");
#endif


            DellFanLib.Initialize();
            currentAppState.Act();
            aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
            {
                currentAppState.Act();
            });
            aTimer.Enabled = true;

            ApplicationConfiguration.Initialize();


            NotifyIcon notifyIcon = new()
            {
                Icon = new Icon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Poorly-drawn-fan.ico")),
                Text = "Dell Fan Control",
                Visible = true
            };
            ContextMenuStrip contextMenu = new();
            List<ToolStripMenuItem> menuItems = [];

            AddItem(contextMenu, menuItems, "Auto", Auto, ticked: true);
            contextMenu.Items.Add(new ToolStripSeparator());
            AddItem(contextMenu, menuItems, "Off", () =>
            {
                currentAppState = AppState.VERY_LOW; SetOff();
            });
            AddItem(contextMenu, menuItems, "One slow", () =>
            {
                currentAppState = AppState.VERY_LOW; SetVeryLow();
            });
            AddItem(contextMenu, menuItems, "Slow", () =>
            {
                currentAppState = AppState.LOW; SetLow();
            });
            AddItem(contextMenu, menuItems, "High", () =>
            {
                currentAppState = AppState.HIGH; SetHigh();
            });
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("EXIT", null, (sender, args) =>
            {
                DellFanLib.EnableEcFanControl(); Application.Exit();
            });

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.MouseClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left) { }
                else if (args.Button == MouseButtons.Right) { }
            };
            Application.Run();
        }
        catch (Exception ex)
        {
            File.WriteAllText("error.log", ex.ToString());
        }
    }

#if DEBUG
    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    private static extern bool AllocConsole();
#endif

    public delegate void VoidFunction();
    static void AddItem(ContextMenuStrip contextMenu, List<ToolStripMenuItem> menuItems, string name, VoidFunction fanSetting, bool ticked = false)
    {

        ToolStripMenuItem fullItem = new(name, null, (sender, args) => { fanSetting(); })
        {
            CheckOnClick = true,
            Checked = ticked
        };
        fullItem.CheckStateChanged += (sender, args) =>
        {
            if (fullItem.Checked)
            {
                foreach (var item in menuItems)
                {
                    if (item != fullItem)
                    {
                        item.Checked = false;
                    }
                }
            }
        };
        menuItems.Add(fullItem);
        contextMenu.Items.Add(fullItem);

    }

}

