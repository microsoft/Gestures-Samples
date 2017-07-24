using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures.Skeleton;
using Microsoft.Gestures.Stock.Gestures;
using Microsoft.Gestures.Stock.HandPoses;
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
        const int MovingAverageWindowSize = 5; // [samples]
        const float PalmPositionJumpThreshold = 50; // [mm]

        private Random _rand = new Random();
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _cameraPinch;

        private MovingAverage _movingAverage;
        private Vector3 _lastAveragePosition = new Vector3(0, 0, 0);

        private SphericalCamera _sphericalCamera;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowLoaded;
            Closed += (s, args) => _gesturesService.Dispose();
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // define the gesture we will be using to rotate the object
            var pinchClose = new PinchPose("PinchClosePose", pinchSpread: false);
            pinchClose.Triggered += (s, args) => _gesturesService.RegisterToSkeleton(OnSekeltonReady);
            _cameraPinch = new Gesture("CameraPinch", new PinchPose("PinchOpenPose", pinchSpread: true), 
                                                      pinchClose, 
                                                      new PinchPose("PinchReleasePose", pinchSpread: true));
            _cameraPinch.Triggered += (s, arg) => _gesturesService.UnregisterFromSkeleton();

            // connect to the Gestures Service and register the gesture we've defined
            _gesturesService = GesturesServiceEndpointFactory.Create();
            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_cameraPinch);

            // auxiliary classes to control the camera motion
            _movingAverage = new MovingAverage(MovingAverageWindowSize);
            _sphericalCamera = new SphericalCamera(Camera, Dispatcher);
        }

        private void OnSekeltonReady(object sender, HandSkeletonsReadyEventArgs e)
        {
            var currentPosition = e.DefaultHandSkeleton.PalmPosition;

            // filter out jumps in palm location
            if ((currentPosition - _movingAverage.CurrentAverage).TwoNorm() > PalmPositionJumpThreshold)
            {
                _movingAverage.Flush();
                _movingAverage.AddNewSample(currentPosition);
                _lastAveragePosition = currentPosition;
                return;
            }

            _movingAverage.AddNewSample(currentPosition);
            var currentAveragePosition = _movingAverage.CurrentAverage;
            var diff = currentAveragePosition - _lastAveragePosition;
            _lastAveragePosition = currentAveragePosition;

            _sphericalCamera.UpdateCamera(diff);
        }
    }
}