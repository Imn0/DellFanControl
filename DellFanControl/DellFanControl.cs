namespace DellFanControl;

using System;
using System.Management;
using System.Timers;
using System.Windows.Forms;
using DellFanManagement.Interop;
using Microsoft.Win32;

enum AppState
{
    AUTO,
    OFF,
    ONE_SLOW,
    SLOW,
    FAST,
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
            case AppState.ONE_SLOW:
                DellFanControl.SetOneSlow();
                break;
            case AppState.SLOW:
                DellFanControl.SetSlow();
                break;
            case AppState.FAST:
                DellFanControl.SetFast();
                break;
            case AppState.AUTO:
                DellFanControl.Auto();
                break;
        }
    }
}

static class DellFanControl
{

    private static AppState currentAppState = AppState.AUTO;
    private static System.Timers.Timer? aTimer;

    public static void Auto()
    {

        AppState desiredFanSpeed = AppState.OFF;
        
        var Charging_status = SystemInformation.PowerStatus.BatteryChargeStatus & BatteryChargeStatus.Charging;
        if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online && Charging_status == BatteryChargeStatus.Charging)
        {
            // pluged in and charging 
            desiredFanSpeed = AppState.SLOW;
        }
        else if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online)
        {
            // pluged in and NOT charging 
            desiredFanSpeed = AppState.ONE_SLOW;
        }


        // check monitor count
        int monitorCount = 1;

        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (PNPClass = 'Monitor')"))
            {
                var monitors = searcher.Get();
                monitorCount = monitors.Count;
               
            }
        }
        catch (Exception ex)
        {
            File.WriteAllText("error.log", $"An error occurred: {ex.Message}");
        }
        
        if(monitorCount > 1){
            desiredFanSpeed = AppState.SLOW;
        }
        
        
        // check cpu temp 
        // TODO


        desiredFanSpeed.Act();
    }

    public static void SetOff()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level0);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    public static void SetOneSlow()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    public static void SetSlow()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level1);
    }
    public static void SetFast()
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
            DellFanLib.Initialize();
            aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += new ElapsedEventHandler((object? source, ElapsedEventArgs e) =>
            {
                currentAppState.Act();
            });
            aTimer.Enabled = true;

            ApplicationConfiguration.Initialize();

            NotifyIcon notifyIcon = new()
            {
                Icon = new Icon("./Poorly-drawn-fan.ico"),
                Text = "Dell Fan Control",
                Visible = true
            };
            ContextMenuStrip contextMenu = new();
            List<ToolStripMenuItem> menuItems = [];

            AddItem(contextMenu, menuItems, "Auto", Auto, ticked: true);
            contextMenu.Items.Add(new ToolStripSeparator());
            AddItem(contextMenu, menuItems, "Off", () =>
            {
                currentAppState = AppState.ONE_SLOW; SetOff();
            });
            AddItem(contextMenu, menuItems, "One slow", () =>
            {
                currentAppState = AppState.ONE_SLOW; SetOneSlow();
            });
            AddItem(contextMenu, menuItems, "Slow", () =>
            {
                currentAppState = AppState.SLOW; SetSlow();
            });
            AddItem(contextMenu, menuItems, "Full", () =>
            {
                currentAppState = AppState.FAST; SetFast();
            });
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("EXIT", null, (sender, args) =>
            {
                DellFanLib.EnableEcFanControl(); Application.Exit();
            });

            notifyIcon.ContextMenuStrip = contextMenu;

            notifyIcon.MouseClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Left)
                {
                }
                else if (args.Button == MouseButtons.Right)
                {
                }
            };
            notifyIcon.MouseDoubleClick += (sender, args) =>
            {
                if (args.Button == MouseButtons.Right)
                {
                    Application.Exit();
                }
            };
            Application.Run();
        }
        catch (Exception ex)
        {
            File.WriteAllText("error.log", ex.ToString());
        }
    }
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