using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using CrystalDecisions.CrystalReports.Engine;
using System.Windows.Forms;

namespace RPTExporter
{
    public partial class RPTSelect : BridgeOpsClient.CustomWindow
    {
        string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        List<FileItem> files = new List<FileItem>();

        public RPTSelect()
        {
            InitializeComponent();

            directory = Properties.Settings.Default.Directory;
            if (directory == "[blank]")
            {
                directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Properties.Settings.Default.Directory = directory;
                Properties.Settings.Default.Save();
            }

            UpdateDirectory();
        }

        private void UpdateDirectory()
        {
            lblDirectory.Content = directory;
            Refresh();
        }

        public void Refresh()
        {
            lst.ItemsSource = null;
            files.Clear();
            if (Directory.Exists(directory))
            {
                string[] fileNames = Directory.GetFiles(directory, "*.rpt", SearchOption.TopDirectoryOnly);
                foreach (string fn in fileNames)
                {
                    FileInfo fileInfo = new FileInfo(fn);
                    files.Add(new FileItem { FileInfo = fileInfo });
                }

                lst.ItemsSource = files;
            }
        }

        private void Export()
        {
            try
            {
                FileItem fileItem = (FileItem)lst.SelectedItem;
                FileInfo fi = fileItem.FileInfo;

                using (ReportDocument reportDocument = new ReportDocument())
                {
                    reportDocument.Load(fi.FullName);

                    SetParameters setParameters = new SetParameters(reportDocument);
                    setParameters.Owner = this;
                    setParameters.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                App.DisplayWarning(this, ex.Message);
            }
        }

        private void lst_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || lst.SelectedItem == null)
                return;

            Export();
        }

        // Modify the FileItem class to provide a name without extension for use in the ListView.
        public class FileItem
        {
            public FileInfo FileInfo { get; set; }
            public string Name => FileInfo.Name;
            public string NameWithoutExtension => Path.GetFileNameWithoutExtension(Name);
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (lst.SelectedItem == null)
            {
                App.DisplayWarning(this, "You must select a file to export.");
                return;
            }

            Export();
        }

        private void btnDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (FolderBrowserDialog folder = new FolderBrowserDialog())
                {
                    DialogResult result = folder.ShowDialog();
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        if (!Directory.Exists(folder.SelectedPath))
                            throw new Exception();
                        directory = folder.SelectedPath;
                        Properties.Settings.Default.Directory = directory;
                        Properties.Settings.Default.Save();

                        UpdateDirectory();
                    }
                }
            }
            catch
            {
                App.DisplayWarning(this, "Could not select folder.");
            }
        }
    }
}