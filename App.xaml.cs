using BridgeOpsClient.DialogWindows;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        private void DateTimeClearValueHandler(object o, EventArgs e)
        {
            try
            {
                if (o is TextBox txt && txt.Text == "")
                {
                    DatePicker dp = GetParentControl<DatePicker>(txt);
                    if (dp != null)
                        dp.SelectedDate = null;
                }
            }
            catch { }
        }
        public static T GetParentControl<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);
            while (parentObject != null && !(parentObject is Window))
            {
                parentObject = VisualTreeHelper.GetParent(parentObject);
                if (parentObject is T t)
                    return t;
            }
            return null;
        }

    }
}
