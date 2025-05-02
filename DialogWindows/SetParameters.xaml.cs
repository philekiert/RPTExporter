using BridgeOpsClient.DialogWindows;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using BridgeOpsClient.CustomControls;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using SAPBusinessObjects.WPF.Viewer;
using System.Diagnostics;

namespace RPTExporter
{
    public partial class SetParameters : BridgeOpsClient.CustomWindow
    {
        DataTemplate lblTemplate;
        DataTemplate txtTemplate;
        DataTemplate cmbTemplate;
        DataTemplate numTemplate;
        DataTemplate dtmTemplate;
        DataTemplate datTemplate;
        DataTemplate timTemplate;
        DataTemplate chkTemplate;
        DataTemplate lstTemplate;

        ReportDocument report;

        class FieldRow
        {
            public StackPanel stack;
            public Label description;
            public object value;
            public object resolvedValue;
            public string parameterName;

            public FieldRow(Label description, object value, string parameterName)
            {
                this.description = description;
                this.value = value;
                this.parameterName = parameterName;

                stack = new StackPanel();
                stack.Children.Add(description);
                stack.Children.Add((UIElement)value);
            }
        }
        List<FieldRow> rows = new List<FieldRow>();

        public SetParameters(ReportDocument report)
        {
            this.report = report;

            InitializeComponent();

            lblTemplate = (DataTemplate)FindResource("fieldLbl");
            txtTemplate = (DataTemplate)FindResource("fieldTxt");
            cmbTemplate = (DataTemplate)FindResource("fieldCmb");
            numTemplate = (DataTemplate)FindResource("fieldNum");
            dtmTemplate = (DataTemplate)FindResource("fieldDtm");
            datTemplate = (DataTemplate)FindResource("fieldDat");
            timTemplate = (DataTemplate)FindResource("fieldTim");
            chkTemplate = (DataTemplate)FindResource("fieldChk");
            lstTemplate = (DataTemplate)FindResource("fieldLst");

            foreach (ParameterField param in report.ParameterFields)
            {
                Label lbl = lblTemplate.LoadContent() as Label;
                lbl.Content = param.PromptText;
                object field = null;
                if (param.ParameterValueType == ParameterValueKind.StringParameter)
                {
                    if (param.AllowCustomValues)
                        field = txtTemplate.LoadContent() as TextBox;
                    else
                    {
                        List<string> allowed = new List<string>();
                        foreach (ParameterValue value in param.DefaultValues)
                            if (value is ParameterDiscreteValue discreteValue)
                                allowed.Add(discreteValue.Value.ToString());

                        if (param.EnableAllowMultipleValue)
                        {
                            ScrollViewer scrl = (ScrollViewer)lstTemplate.LoadContent();
                            Grid grd = (Grid)scrl.Content;
                            int i = 0;
                            foreach (string s in allowed)
                            {
                                grd.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(24) });
                                CheckBox chk = new CheckBox()
                                {
                                    Margin = new Thickness(5, 4, 5, 0)
                                };
                                Label option = new Label()
                                {
                                    VerticalAlignment = VerticalAlignment.Center,
                                    Padding = new Thickness(5, 0, 5, 0),
                                    Content = s
                                };
                                Grid.SetColumn(chk, 0);
                                Grid.SetColumn(option, 1);
                                Grid.SetRow(chk, i);
                                Grid.SetRow(option, i);
                                grd.Children.Add(chk);
                                grd.Children.Add(option);
                                ++i;
                            }
                            field = scrl;
                        }
                        else
                        {
                            field = (ComboBox)cmbTemplate.LoadContent();
                            ((ComboBox)field).ItemsSource = allowed;
                        }
                    }
                }
                else if (param.ParameterValueType == ParameterValueKind.NumberParameter)
                    field = numTemplate.LoadContent() as NumberEntry;
                else if (param.ParameterValueType == ParameterValueKind.DateTimeParameter)
                    field = dtmTemplate.LoadContent() as DateTimePicker;
                else if (param.ParameterValueType == ParameterValueKind.DateParameter)
                    field = datTemplate.LoadContent() as DatePicker;
                else if (param.ParameterValueType == ParameterValueKind.TimeParameter)
                    field = timTemplate.LoadContent() as TimePicker;
                else if (param.ParameterValueType == ParameterValueKind.BooleanParameter)
                    field = chkTemplate.LoadContent() as CheckBox;

                FieldRow fieldRow = new FieldRow(lbl, field, param.Name);
                rows.Add(fieldRow);
                stkParams.Children.Add(fieldRow.stack);
            }

            // Set database connection information
            foreach (Table table in report.Database.Tables)
            {
                TableLogOnInfo logOnInfo = table.LogOnInfo;
                logOnInfo.ConnectionInfo.Password = "reader";
                table.ApplyLogOnInfo(logOnInfo);
            }
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (!AssembleUpdate())
                return;

            foreach (FieldRow field in rows)
                report.SetParameterValue(field.parameterName, field.resolvedValue);

            Microsoft.Win32.SaveFileDialog saveDialog = new Microsoft.Win32.SaveFileDialog();
            DateTime now = DateTime.Now;
            string name = System.IO.Path.GetFileNameWithoutExtension(report.FileName);
            saveDialog.FileName = $"{name} {now.ToString("yyyy-MM-dd HHmmss")}.pdf";
            saveDialog.DefaultExt = ".pdf";
            //saveDialog.Filter = "Excel Workbook|*.xlsx|Excel Macro-Enabled Workbook|*.xlsm";
            saveDialog.Filter = "PDF|*.pdf|Excel Workbook|*.xlsx";

            bool? result = saveDialog.ShowDialog();
            if (result != true)
            {
                App.DisplayWarning(this, "You must select a file name.");
                return;
            }
            //if (!saveDialog.FileName.EndsWith(".xlsx") && !saveDialog.FileName.EndsWith(".xlsm"))
            if (!saveDialog.FileName.ToLower().EndsWith(".pdf") && !saveDialog.FileName.ToLower().EndsWith(".xlsx"))
            {
                App.DisplayWarning(this, "Invalid file extension.");
                return;
            }

            Close();

            string saveName = saveDialog.FileName;
            bool pdf = saveName.EndsWith(".pdf");

            ExportOptions exportOptions = report.ExportOptions; 
            exportOptions.ExportFormatType = pdf ? ExportFormatType.PortableDocFormat : ExportFormatType.ExcelWorkbook;
            exportOptions.ExportDestinationType = ExportDestinationType.DiskFile;
            exportOptions.DestinationOptions = new DiskFileDestinationOptions
            {
                DiskFileName = saveName
            };

            // Perform the export
            report.Export();

            Process.Start(saveName);
        }

        private bool AssembleUpdate()
        {
            bool Abort(string message)
            {
                App.DisplayWarning(this, message);
                return false;
            }

            foreach (FieldRow row in rows)
            {
                if (row.value == null)
                    return Abort($"You must select a value for all parameters.");

                if (row.value is TextBox txt)
                {
                    if (txt.Text == "")
                        return Abort($"You must select a value for all parameters.");
                    row.resolvedValue = txt.Text.Replace("'", "''");
                }
                else if (row.value is ComboBox cmb)
                {
                    if (cmb.SelectedIndex < 0)
                        return Abort($"You must select a value for all parameters.");
                    row.resolvedValue = cmb.Text.Replace("'", "''");
                }
                else if (row.value is ScrollViewer scrl)
                {
                    Grid grd = (Grid)scrl.Content;
                    List<string> values = new List<string>();
                    for (int r = 0; r < grd.Children.Count; r += 2)
                    {
                        if (((CheckBox)grd.Children[r]).IsChecked == true)
                            values.Add(((string)((Label)grd.Children[r + 1]).Content).Replace("'", "''"));
                    }
                    if (values.Count == 0)
                        return Abort($"Checkbox lists must have at least one item selected.");
                    row.resolvedValue = $"('{string.Join("', '", values)}')";
                }
                else if (row.value is DateTimePicker dtm)
                {
                    DateTime? dt = dtm.GetDateTime();
                    if (dt == null)
                        return Abort($"You must select a value for all parameters.");
                    row.resolvedValue = dt.Value;
                }
                else if (row.value is DatePicker dat)
                {
                    if (dat.SelectedDate == null)
                        return Abort($"You must select a value for all parameters.");
                    row.resolvedValue = dat.SelectedDate.Value;
                }
                else if (row.value is TimePicker tim)
                {
                    TimeSpan? ts = tim.GetTime();
                    if (ts == null)
                        return Abort($"You must select a value for all parameters.");
                    row.resolvedValue = ts.Value;
                }
                else if (row.value is CheckBox chk)
                {
                    if (chk.IsChecked == null)
                        return Abort($"You must select a value for all parameters.");
                    row.resolvedValue = chk.IsChecked == true;
                }
                else
                    return Abort($"You must select a value for all parameters.");
            }

            return true;
        }
    }
}
