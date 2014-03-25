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

        public static void Start(Window owner, Action<BackgroundWorker> task, Action<bool> completed)
        {
            var window = new ProgressWindow { Owner = owner, Topmost = true };
            window.worker.DoWork += (o, e) =>
            {
                var worker = (BackgroundWorker)o;
                task(worker);
                e.Cancel = worker.CancellationPending;
            };
            window.worker.RunWorkerCompleted += (o, e) =>
            {
                window.Close();
                owner.IsEnabled = true;
                completed(e.Cancelled);
            };
            window.Show();
            owner.IsEnabled = false;
            window.worker.RunWorkerAsync();
        }
    }
}
