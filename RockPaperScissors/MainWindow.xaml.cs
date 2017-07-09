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

            _game = new GesturesRockPaperScissors();

            _game.GesturesDetectionStatusChanged += (oldStatus, newStatus) => Dispatcher.InvokeAsync(() => txtDetectionServiceStatus.Text = $"Gestures Detection Service [{newStatus.Status}]");

            _game.UserStrategyChanged += (newStrategy) => Dispatcher.InvokeAsync(() =>
            {
                txtCurrentStrategy.Text = $"Last detected user strategy: [{newStrategy}]";

                var winningStrategy = GesturesRockPaperScissors.WinningStrategy(newStrategy);
                txtMessage.Text = (newStrategy == GameStrategy.None ? $@"Machine be like ¯\_(ツ)_/¯ {Environment.NewLine} Try again" :
                                                                      $"Machine plays: {winningStrategy}");
                txtMessage.Text += $"{Environment.NewLine} Let's play again";
                imgStartegyRock.Visibility = winningStrategy == GameStrategy.Rock ? Visibility.Visible : Visibility.Collapsed;
                imgStartegyPaper.Visibility = winningStrategy == GameStrategy.Paper ? Visibility.Visible : Visibility.Collapsed;
                imgStartegyScissor.Visibility = winningStrategy == GameStrategy.Scissors ? Visibility.Visible : Visibility.Collapsed;
            });

            Loaded += async (s, arg) => await _game.Init();
            Closed += (s, arg) => _game?.Dispose();
        }
    }
}
