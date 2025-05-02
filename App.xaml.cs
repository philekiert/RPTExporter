using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using BridgeOpsClient.DialogWindows;

namespace RPTExporter
{
    public partial class App : Application
    {
        public static void DisplayWarning(Window w, string message)
        {
            DialogBox dialog = new DialogBox(message);
            dialog.Owner = w;
            dialog.ShowDialog();
        }
    }
}
