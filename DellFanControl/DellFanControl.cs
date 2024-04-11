namespace DellFanControl;

using System;
using System.Windows.Forms;
using DellFanManagement.Interop;

static class DellFanControl
{

    static void SetOff()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level0);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    static void SetOneSlow()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level0);
    }
    static void SetSlow()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level1);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level1);
    }
    static void SetFull()
    {
        DellFanLib.DisableEcFanControl();
        DellFanLib.SetFanLevel(FanIndex.Fan2, FanLevel.Level2);
        DellFanLib.SetFanLevel(FanIndex.Fan1, FanLevel.Level2);
    }

    [STAThread]
    static void Main()
    {

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
        AddItem(contextMenu, menuItems, "Full", SetFull);


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
    public delegate void VoidFunction();
    static void AddItem(ContextMenuStrip contextMenu, List<ToolStripMenuItem> menuItems, String name, VoidFunction fanSetting, bool ticked=false)
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