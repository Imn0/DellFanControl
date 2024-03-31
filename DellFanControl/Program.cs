namespace DellFanControl;

using System;
using System.Diagnostics;
using System.Windows.Forms;
using DellFanManagement.Interop;
using Microsoft.VisualBasic.ApplicationServices;

static class Program
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
            Icon = SystemIcons.Application,
            Text = "My Application",
            Visible = true
        };

        // Create a context menu and add items
        ContextMenuStrip contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("OFF", null, (sender, args) => { SetOff(); });
        contextMenu.Items.Add("One Slow", null, (sender, args) => { SetOneSlow(); });
        contextMenu.Items.Add("Slow", null, (sender, args) => { SetSlow(); });
        contextMenu.Items.Add("Fast", null, (sender, args) => { SetFull(); });

        // Assign the context menu to the notify icon
        notifyIcon.ContextMenuStrip = contextMenu;

        // Existing mouse click event handler...
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
            if (args.Button == MouseButtons.Left)
            {
                Application.Exit();
            }
        };
        Application.Run();
    }
}