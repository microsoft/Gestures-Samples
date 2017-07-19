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
        private Random _rand = new Random();
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _cameraPinch;
        private Vector3 _lastPalmLocation = new Vector3(0,0,0);

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowLoaded;
            Closed += (s, args) => _gesturesService.Dispose();
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            var pinchClose = new PinchPose("PinchClosePose", pinchSpread: false);
            pinchClose.Triggered += (s, args) => _gesturesService.RegisterToSkeleton(OnSekeltonReady);
            _cameraPinch = new Gesture("CameraPinch", new PinchPose("PinchOpenPose", pinchSpread: true), 
                                                      pinchClose, 
                                                      new PinchPose("PinchReleasePose", pinchSpread: true));
            _cameraPinch.Triggered += (s, arg) => _gesturesService.UnregisterFromSkeleton();

            _gesturesService = GesturesServiceEndpointFactory.Create();
            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_cameraPinch);
        }

        private void OnSekeltonReady(object sender, HandSkeletonsReadyEventArgs e)
        {
            var currentPalmLocation = e.DefaultHandSkeleton.PalmPosition;
            var diff = _lastPalmLocation - currentPalmLocation;
            _lastPalmLocation = currentPalmLocation;

            // Yoni: do the right logic here for smoothing and moving camera
            if (diff.X < 10 && diff.Y < 10 && diff.Z < 10)
            {
                Dispatcher.InvokeAsync(() => Camera.Position = Camera.Position + new Vector3D(diff.X, diff.Y, diff.Z));
            }
        }
    }
}
