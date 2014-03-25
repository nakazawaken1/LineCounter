using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LineCounter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public static ObservableCollection<string> Paths = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListBoxPaths_DoubleClick(object o, EventArgs e)
        {
            Paths.Remove(((ListBoxItem)o).Content as string).ToString();
        }

        private void ListBoxPaths_Drop(object sender, DragEventArgs e)
        {
            foreach (var i in e.Data.GetData(DataFormats.FileDrop) as string[]) if (!Paths.Contains(i)) Paths.Add(i);
        }

        private void ListBoxPaths_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetData(DataFormats.FileDrop) != null ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch(((Button)sender).Content as string)
            {
                case "行カウント開始":
                    if (Paths.Count <= 0)
                    {
                        MessageBox.Show("行数を数えるファイルまたはフォルダを中央にドラッグしてください。");
                        return;
                    }
                    var result = App.Current.Windows.OfType<ResultWindow>().FirstOrDefault();
                    if (result == null) result = new ResultWindow();
                    var rows = new List<Row>();
                    var title = string.Empty;
                    ProgressWindow.Start(this, worker => title = ResultWindow.Setup(worker, rows, Paths, Properties.Settings.Default.Exclude.Split(new char[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries))
                    , cancel => {
                        if (!cancel)
                        {
                            result.Title = title;
                            ResultWindow.Rows.Clear();
                            foreach (var i in rows) ResultWindow.Rows.Add(i);
                            result.Owner = this;
                            result.ShowDialog();
                        }
                    });
                    break;
                case "一覧クリア":
                    Paths.Clear();
                    break;
                default:
                    Close();
                    break;
            }
        }

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(MessageBox.Show("除外する名前を初期設定に戻します。よろしいですか？", "確認", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Properties.Settings.Default.Reset();
            }
        }
    }
}
