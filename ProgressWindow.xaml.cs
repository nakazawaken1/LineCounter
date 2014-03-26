using System;
using System.Collections.Generic;
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
    /// ProgressWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ProgressWindow : Window
    {
        BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

        public ProgressWindow()
        {
            InitializeComponent();
            worker.ProgressChanged += (o, e) =>
            {
                progressBar.Value = e.ProgressPercentage;
                message.Text = e.UserState.ToString();
            };
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
        }

        /// <summary>
        /// プログレスダイアログ表示
        /// </summary>
        /// <param name="owner">親ウインドウ</param>
        /// <param name="title">タイトル</param>
        /// <param name="progress">true:進捗率を表示する、false:しない</param>
        /// <param name="task">処理(この中ではUIにはアクセスできない。キャンセルはBackgroundWorker.CancellationPendingでチェック、進捗はBackgroundWorker.ReportProgress(進捗率[progress=falseのときは0], メッセージ)で通知)</param>
        /// <param name="completed">完了時処理(キャンセルされたかどうかはRunWorkerCompletedEventArgs.Cancelled、処理の結果はRunWorkerCompletedEventArgs.Result、例外発生時はRunWorkerCompletedEventArgs.Errorにセットされる)</param>
        public static void Start(Window owner, string title, bool progress, Func<BackgroundWorker, object> task, Action<RunWorkerCompletedEventArgs> completed)
        {
            var window = new ProgressWindow { Owner = owner, Title = title };
            window.progressBar.IsIndeterminate = !progress;
            window.worker.DoWork += (o, e) =>
            {
                var worker = (BackgroundWorker)o;
                e.Result = task(worker);
                e.Cancel = worker.CancellationPending;
            };
            window.worker.RunWorkerCompleted += (o, e) =>
            {
                window.Close();
                owner.IsEnabled = true;
                completed(e);
            };
            owner.IsEnabled = false;
            window.Show();
            window.worker.RunWorkerAsync();
        }
    }
}
