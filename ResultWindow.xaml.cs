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

        public ResultWindow()
        {
            InitializeComponent();
        }

        public static string Setup(BackgroundWorker worker, ICollection<Row> rows, IEnumerable<string> list, IEnumerable<string> exclude, bool first = true)
        {
            if (first) rows.Clear();
            var max = list.Count();
            var count = 0;
            foreach (var i in list)
            {
                if (worker.CancellationPending) return null;
                if (exclude.Any(j => i.EndsWith(j, StringComparison.CurrentCultureIgnoreCase))) continue;
                if (System.IO.Directory.Exists(i))
                {
                    if(Setup(worker, rows, System.IO.Directory.EnumerateFileSystemEntries(i), exclude, false) == null) return null;
                }
                else if(!rows.Any(j => j.Path == i))
                {
                    rows.Add(new Row(i));
                }
                count++;
                if (first) worker.ReportProgress(count * 100 / max, string.Format("{0:#,0}ファイル {1:#,0}行", rows.Count, rows.Aggregate(0L, (s, r) => s += r.Count)));
            }
            return string.Format("全 {0:#,0}ファイル {1:#,0}行", rows.Count, rows.Aggregate(0L, (s, r) => s += r.Count));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((Button)sender).Content as string)
            {
                case "コピー":
                    Clipboard.SetText(ToText(Rows, "\t"));
                    break;
                case "CSVで保存":
                    var dialog = new SaveFileDialog { FileName = "line_count.tsv", DefaultExt=".csv", Filter = "CSV形式 (.csv)|*.csv" };
                    if (dialog.ShowDialog() ?? false) System.IO.File.WriteAllText(dialog.FileName, ToText(Rows, ","));
                    break;
                default:
                    Close();
                    break;
            }
        }

        public static string ToText(IEnumerable<Row> rows, string separator)
        {
            var text = new StringBuilder();
            foreach (var i in Rows)
            {
                text.Append(i.Path).Append(separator).Append(i.Type).Append(separator).AppendLine(i.Count.ToString());
            }
            return text.ToString();
        }

        private void DataGridRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(((Row)((DataGridRow)sender).Item).Path);
        }
    }

    public class Row
    {
        public Row(string path)
        {
            Path = path;
            Type = System.IO.Path.GetExtension(path);
            Count = System.IO.File.ReadAllLines(path).Length;
        }
        public string Path { get; set; }
        public string Type { get; set; }
        public long Count { get; set; }
    }
}
