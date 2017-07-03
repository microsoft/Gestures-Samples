using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using Microsoft.Gestures.Endpoint;

namespace Microsoft.Gestures.Samples.RockPaperScissors
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GesturesRockPaperScissors _game;

        public MainWindow()
        {
            InitializeComponent();
            UpdateConnectionStatus(EndpointStatus.Disconnected);

            _game = new GesturesRockPaperScissors();
            _game.GesturesDetectionStatusChanged += (oldStatus, newStatus) => UpdateConnectionStatus(newStatus.Status);
            _game.UserStrategyChanged += (newStrategy) => Dispatcher.InvokeAsync(() => txtCurrentStrategy.Text = $"Last detected user strategy: [{newStrategy}]");
            _game.UserStrategyFinal += (finalStrategy) => Dispatcher.InvokeAsync(() =>
            {
                var winningStrategy = GesturesRockPaperScissors.WinningStrategy(finalStrategy);
                txtMessage.Text = (finalStrategy == GameStrategy.None ? @"Machine be like ¯\_(ツ)_/¯" + Environment.NewLine + "Try shaking a bit slower" :
                                                                        $"Machine plays: {winningStrategy}");
                imgStartegyRock.Visibility    = winningStrategy == GameStrategy.Rock ? Visibility.Visible : Visibility.Collapsed;
                imgStartegyPaper.Visibility   = winningStrategy == GameStrategy.Paper ? Visibility.Visible : Visibility.Collapsed;
                imgStartegyScissor.Visibility = winningStrategy == GameStrategy.Scissors ? Visibility.Visible : Visibility.Collapsed;
                txtPlayAgain.Visibility = Visibility.Visible;
            });

            Loaded += async (s, arg) => await _game.Init();
            Closed += (s, arg) => _game?.Dispose();
        }

        private void UpdateConnectionStatus(EndpointStatus newStatus) => Dispatcher.InvokeAsync(() => txtDetectionServiceStatus.Text = $"Gestures Detection Service [{newStatus}]");
    }
}
