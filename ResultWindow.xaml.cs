using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LineCounter
{
    /// <summary>
    /// ResultWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ResultWindow : Window
    {
        public static ObservableCollection<Row> Rows = new ObservableCollection<Row>();
        public static ObservableCollection<Row> ExcludeRows = new ObservableCollection<Row>();

        public ResultWindow()
        {
            InitializeComponent();
        }

        public static string Setup(BackgroundWorker worker, ICollection<Row> rows, ICollection<Row> excludeRows, IEnumerable<string> list, IEnumerable<string> exclude)
        {
            foreach (var i in list)
            {
                if (worker.CancellationPending) return null;
                if (exclude.Any(j => i.EndsWith(j, StringComparison.CurrentCultureIgnoreCase)))
                {
                    excludeRows.Add(new Row(i, false));
                }
                else if (System.IO.Directory.Exists(i))
                {
                    worker.ReportProgress(0, string.Format("{0:#,0}ファイル {1:#,0}行 {2:#,0}バイト 除外 {3:#,0}", rows.Count, rows.Aggregate(0L, (s, r) => s += r.Count ?? 0L), rows.Aggregate(0L, (s, r) => s += r.Size ?? 0L), excludeRows.Count));
                    if (Setup(worker, rows, excludeRows, System.IO.Directory.EnumerateFileSystemEntries(i), exclude) == null) return null;
                }
                else if (!rows.Any(j => j.Path == i))
                {
                    rows.Add(new Row(i, true));
                }
            }
            return string.Format("全 {0:#,0}ファイル {1:#,0}行 {2:#,0}バイト 除外 {3:#,0}", rows.Count, rows.Aggregate(0L, (s, r) => s += r.Count ?? 0L), rows.Aggregate(0L, (s, r) => s += r.Size ?? 0L), excludeRows.Count);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Content as string)
            {
                case "コピー":
                    Clipboard.SetText(ToText(Rows, "\t", "対象") + ToText(ExcludeRows, "\t", "除外"));
                    break;
                case "CSVで保存":
                    var dialog = new SaveFileDialog { FileName = "line_count.tsv", DefaultExt = ".csv", Filter = "CSV形式 (.csv)|*.csv" };
                    if (dialog.ShowDialog() ?? false) System.IO.File.WriteAllText(dialog.FileName, ToText(Rows, ",", "対象") + ToText(ExcludeRows, ",", "除外"));
                    break;
                default:
                    Close();
                    break;
            }
        }

        public static string ToText(IEnumerable<Row> rows, string separator, string header)
        {
            var text = new StringBuilder();
            foreach (var i in rows)
            {
                if (header != null) text.Append(header).Append(separator);
                text.Append(i.Path).Append(separator)
                    .Append(i.Type).Append(separator)
                    .Append(i.Count.HasValue ? i.Count.ToString() : string.Empty).Append(separator)
                    .AppendLine(i.Size.HasValue ? i.Size.ToString() : string.Empty);
            }
            return text.ToString();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((Row)((DataGridRow)sender).Item).Path);
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = e.Row.GetIndex() + 1;
        }
    }

    public class Row
    {
        public Row(string path, bool count)
        {
            Path = path;
            if (count) Count = System.IO.File.ReadAllLines(path).Length;
            if (System.IO.File.Exists(path)) Size = new System.IO.FileInfo(path).Length;
            Type = Size.HasValue ? System.IO.Path.GetExtension(path) : "フォルダ";
        }
        public string Path { get; set; }
        public string Type { get; set; }
        public long? Count { get; set; }
        public long? Size { get; set; }
    }
}
