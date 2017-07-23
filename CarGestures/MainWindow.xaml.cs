using Microsoft.Gestures.Endpoint;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Microsoft.Gestures.Samples.CarGestures
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public enum AudioSource
        {
            Phone = 0,
            Radio,
            Media,
            USB,
        }

        public const double FrameRate = 60;
        public const double AnimationSpeed = 6;
        public const double InitialVolume = 0.7d;
        public const double VolumeIncrement = 0.1f;
        public const double VolumeMinAngle = 135;
        public const double VolumeMaxAngle = 360 + 45;
        public const double VolumeRadius = 130;
        public const double VolumeThickness = 30;

        public const double MaxTemp = 30;
        public const double MinTemp = 10;
        public const double TempIncrement = 2;
        private const double DefaultTemperature = 25.0;
        private DispatcherTimer _timer;

        // Source
        private AudioSource _audioSource = AudioSource.Radio;

        // Incoming Call
        private double _targetCallBackOpacity = 0;
        private double _targetIncomingCallOpacity = 0;
        private double _targetTalkingWithOpacity = 0;

        // Volume Control
        private bool _isMute = false;
        private double _currentVolume = InitialVolume;
        private double _targetVolume = InitialVolume;

        // Radio
        private static readonly string[] _radioStations = new[] { "KBAL", "Radius", "99 FM", "KSAD" };
        private static readonly double[] _radioStationsHz = new[] { 91.8, 100.0, 99, 103 };
        private int _currentStation = 0;
        private double _currentStationHz = _radioStationsHz[0];

        // Temperature
        private double _currentTemperature = DefaultTemperature;
        private double _targetTemperature = DefaultTemperature;

        private CarGesturesManager _gesturesManager = new CarGesturesManager();

        private int CallGenerationInterval = 45 * 1000; //in msec
        private Timer _callGenerator;

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
            _callGenerator = new Timer(new TimerCallback((o) => IncomingCall()), null, Timeout.Infinite, Timeout.Infinite);

            Loaded += async (s, a) => {
                                        Closed += (sender, arg) => _gesturesManager.Dispose();
                                        _gesturesManager.StatusChanged += (sender, arg) => Dispatcher.Invoke(() => GesturesServiceStatus.Fill = new SolidColorBrush(arg.Status == EndpointStatus.Detecting ? Colors.LightGreen : Colors.LightGray));

                                        _gesturesManager.DismissNotification += (s1, a1) => DeclineCall();

                                        _gesturesManager.AnswerCall += (s1, a1) => AcceptCall();
                                        _gesturesManager.HangUpCall += (s1, a1) => DeclineCall();

                                        _gesturesManager.VolumeToggleMute += (s1, a1) => ToggleMute();
                                        _gesturesManager.VolumeUp += (s1, a1) => VolumeUp();
                                        _gesturesManager.VolumeDown += (s1, a1) => VolumeDown();

                                        _gesturesManager.TemperatureUp += (s1, a1) => TempUp();
                                        _gesturesManager.TemperatureDown += (s1, a1) => TempDown();

                                        _gesturesManager.SelectSourcePhone += (s1, a1) => SetAudioSource(AudioSource.Phone);
                                        _gesturesManager.SelectSourceRadio += (s1, a1) => SetAudioSource(AudioSource.Radio);
                                        _gesturesManager.SelectSourceMedia += (s1, a1) => SetAudioSource(AudioSource.Media);
                                        _gesturesManager.SelectSourceUsb += (s1, a1) => SetAudioSource(AudioSource.USB);

                                        _gesturesManager.NextChannel += (s1, a1) => StationDown();

                                        await _gesturesManager.Initialize();
                                        _callGenerator.Change(CallGenerationInterval / 3, Timeout.Infinite);
                                      };
        }

        private void Initialize()
        {
            IncomingCallBack.Opacity = IncomingCallUI.Opacity = 0;
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            _timer = new DispatcherTimer();
            _timer.Tick += OnTimer_Tick;
            _timer.Interval = TimeSpan.FromSeconds(1 / FrameRate);
            _timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            switch (e.Key)
            {
                case Key.NumPad1:
                case Key.D1:
                    SetAudioSource(AudioSource.Phone);
                    break;
                case Key.NumPad2:
                case Key.D2:
                    SetAudioSource(AudioSource.Radio);
                    break;
                case Key.NumPad3:
                case Key.D3:
                    SetAudioSource(AudioSource.Media);
                    break;
                case Key.NumPad4:
                case Key.D4:
                    SetAudioSource(AudioSource.USB);
                    break;
                case Key.C:
                    IncomingCall();
                    break;
                case Key.D:
                    DeclineCall();
                    break;
                case Key.A:
                    AcceptCall();
                    break;
                case Key.M:
                case Key.VolumeMute:
                    ToggleMute();
                    break;
                case Key.Up:
                case Key.VolumeUp:
                    VolumeUp();
                    break;
                case Key.Down:
                case Key.VolumeDown:
                    VolumeDown();
                    break;
                case Key.Right:
                    StationUp();
                    break;
                case Key.Left:
                    StationDown();
                    break;
                case Key.OemPlus:
                case Key.Add:
                    TempUp();
                    break;
                case Key.OemMinus:
                case Key.Subtract:
                    TempDown();
                    break;
                default:
                    break;
            }
        }

        private static double DegToRad(double deg) => deg / 180 * Math.PI;

        private static double SinD(double deg) => Math.Sin(DegToRad(deg));

        private static double CosD(double deg) => Math.Cos(DegToRad(deg));

        private Geometry GetVolumeGeometry(double angle)
        {
            var cx = VolumeDial.ActualWidth / 2;
            var cy = VolumeDial.ActualHeight / 2;
            var r = VolumeRadius;
            var R = VolumeRadius + VolumeThickness;


            // start
            var isLargeArc = (angle - VolumeMinAngle) > 180 ? 1 : 0;
            var cosS = CosD(VolumeMinAngle);
            var sinS = SinD(VolumeMinAngle);

            var sx1 = r * cosS + cx;
            var sy1 = r * sinS + cy;

            var sx2 = R * cosS + cx;
            var sy2 = R * sinS + cy;

            // end
            var cosE = CosD(angle);
            var sinE = SinD(angle);

            var ex1 = r * cosE + cx;
            var ey1 = r * sinE + cy;

            var ex2 = R * cosE + cx;
            var ey2 = R * sinE + cy;

            return Geometry.Parse($"M{sx2},{sy2} A{R},{R} 0 {isLargeArc} 1 {ex2}, {ey2} L{ex1},{ey1} A{r},{r} 0 {isLargeArc} 0 {sx1}, {sy1}");
        }

        private void OnTimer_Tick(object sender, EventArgs e)
        {
            // Incoming Call
            IncomingCallBack.Opacity += (_targetCallBackOpacity - IncomingCallBack.Opacity) / AnimationSpeed;
            IncomingCallUI.Opacity += (_targetIncomingCallOpacity - IncomingCallUI.Opacity) / AnimationSpeed;
            TalkingWith.Opacity += (_targetTalkingWithOpacity - TalkingWith.Opacity) / AnimationSpeed;

            // Volume
            _currentVolume += (_targetVolume - _currentVolume) / AnimationSpeed;
            VolumeText.Content = ((int)Math.Round(10 * _currentVolume)).ToString();
            VolumeDial.Data = GetVolumeGeometry(_currentVolume * (VolumeMaxAngle - VolumeMinAngle) + VolumeMinAngle);
            VolumeDialBorder.Data = GetVolumeGeometry(VolumeMaxAngle);
            MuteIcon.Opacity += ((_isMute || _targetVolume == 0) ? 1 : 0 - MuteIcon.Opacity) / AnimationSpeed;
            VolumeText.Opacity = 0.2 + Math.Max(0, 1 - MuteIcon.Opacity - 0.2);

            // Audio Source
            var audioSourceCount = Enum.GetValues(typeof(AudioSource)).Length;
            var audioSourceOffset = (AudioSourcePanel.ActualWidth - AudioSourceHighlight.ActualWidth * audioSourceCount) / (audioSourceCount - 1) + AudioSourceHighlight.ActualWidth;
            var audioSouceHighlightMargin = AudioSourceHighlight.Margin;
            var audioSouceHighlightTargetLeft = (int)_audioSource * audioSourceOffset;
            audioSouceHighlightMargin.Left += (audioSouceHighlightTargetLeft - audioSouceHighlightMargin.Left) / AnimationSpeed;
            AudioSourceHighlight.Margin = audioSouceHighlightMargin;

            DateText.Text = DateTime.Now.ToString("G") + DateTime.Now.ToString(" dddd, MMMM d, yyyy");

            //Radio station
            _currentStationHz += (_radioStationsHz[_currentStation] - _currentStationHz) / AnimationSpeed;
            RadioText.Text = _currentStationHz.ToString("N1") + " FM";
            RadioNameText.Text = _radioStations[_currentStation];

            // Temperature
            _currentTemperature += (_targetTemperature - _currentTemperature) / AnimationSpeed;
            TempText.Text = string.Format("{0:N1}°", _currentTemperature);
            var tempMargin = TempPointer.Margin;
            var tempNormalized = (_currentTemperature - MinTemp) / (MaxTemp - MinTemp);
            tempMargin.Bottom = TempPanel.Margin.Bottom + tempNormalized * (TempPanel.ActualHeight + TempPointer.ActualHeight / 2) - TempPointer.ActualHeight / 2;
            TempPointer.Margin = tempMargin;
        }

        public async void IncomingCall()
        {
            await _gesturesManager.IncomingCall();
            _targetCallBackOpacity = 1;
            _targetIncomingCallOpacity = 1;
            _targetTalkingWithOpacity = 0;
        }

        public void AcceptCall()
        {
            if (_targetCallBackOpacity != 1) return;
            _targetCallBackOpacity = 1;
            _targetIncomingCallOpacity = 0;
            _targetTalkingWithOpacity = 1;
        }

        public void DeclineCall()
        {
            _targetCallBackOpacity = _targetIncomingCallOpacity = _targetTalkingWithOpacity = 0;
            _callGenerator.Change(CallGenerationInterval, Timeout.Infinite);
        }
        public void EndCall() => DeclineCall();

        public void ToggleMute() => _isMute = !_isMute;

        public void VolumeUp()
        {
            if (_isMute) ToggleMute();
            _targetVolume = Math.Min(_targetVolume + VolumeIncrement, 1);
        }

        public void VolumeDown()
        {
            if (_isMute) ToggleMute();
            _targetVolume = Math.Max(_targetVolume - VolumeIncrement, 0);
        }

        public int StationUp() => _currentStation = (_currentStation + 1) % _radioStations.Length;

        public int StationDown() => _currentStation = (_currentStation + 1) % _radioStations.Length;

        public void TempUp() => _targetTemperature = Math.Min(_targetTemperature + TempIncrement, MaxTemp);

        public void TempDown() => _targetTemperature = Math.Max(_targetTemperature - TempIncrement, MinTemp);

        public void SetAudioSource(AudioSource source) => _audioSource = source;
    }
}
