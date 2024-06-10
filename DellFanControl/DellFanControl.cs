namespace DellFanControl;

using System;
using System.Timers;
using System.Windows.Forms;
using DellFanManagement.Interop;
using Microsoft.Win32;

enum FanState
{
    OFF,
    ONE_SLOW,
    SLOW,
    FAST,
}

static class DellFanControl
{

    private static FanState currentFanState = FanState.OFF;
    private static System.Timers.Timer? aTimer;

    static void SetOff()
    {
        currentFanState = FanState.OFF;
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level0);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    static void SetOneSlow()
    {
        currentFanState = FanState.ONE_SLOW;

        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    static void SetSlow()
    {
        currentFanState = FanState.SLOW;

        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level1);
    }
    static void SetFast()
    {
        currentFanState = FanState.FAST;

        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level2);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level2);
    }

    static void TimerEvent(object? source, ElapsedEventArgs e){

        switch (currentFanState)
        {
            case FanState.OFF:
                SetOff();
                break;
            case FanState.ONE_SLOW:
                SetOneSlow();
                break;
            case FanState.SLOW:
                SetSlow();
                break;
            case FanState.FAST:
                SetFast();
                break;
        }
    }

    [STAThread]
    static void Main()
    {
        try
        {
            aTimer = new System.Timers.Timer(10000);
            aTimer.Elapsed += new ElapsedEventHandler(TimerEvent);
            aTimer.Enabled = true;

            DellFanLib.Initialize();

            SetOff();
            ApplicationConfiguration.Initialize();

            NotifyIcon notifyIcon = new()
            {
                Icon = new Icon("./Poorly-drawn-fan.ico"),
                Text = "Dell Fan Control",
                Visible = true
            };
            ContextMenuStrip contextMenu = new();
            List<ToolStripMenuItem> menuItems = [];

            AddItem(contextMenu, menuItems, "Off", SetOff, ticked: true);
            AddItem(contextMenu, menuItems, "One slow", SetOneSlow);
            AddItem(contextMenu, menuItems, "Slow", SetSlow);
            AddItem(contextMenu, menuItems, "Full", SetFast);


            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("EXIT", null, (sender, args) => { Application.Exit(); });

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