using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Camera3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _timer;
        private Random _rand = new Random();

        public MainWindow()
        {
            InitializeComponent();
            _timer = new Timer(OnTimer, null, 0, 1000);
        }

        private void OnTimer(object state)
        {
            Dispatcher.InvokeAsync(() => Camera.Position = Camera.Position + new Vector3D(_rand.Next(2), _rand.Next(2), _rand.Next(2)));
        }
    }
}
