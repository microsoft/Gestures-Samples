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

namespace Microsoft.Gestures.Samples.Camera3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _cameraPinch;

        private PalmSmoother _palmSmoother;

        private SphericalCamera _sphericalCamera;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowLoaded;
            Closed += (s, args) => _gesturesService.Dispose();
        }

        private async void WindowLoaded(object sender, RoutedEventArgs windowLoadedArgs)
        {
            // In XAML, we have specified a 3D scene containing a cube. We would like to use our hand to control the camera in this scene.
            // The following gesture subscribes to the hand skeleton stream whenever our hand forms the pinch pose. The information in 
            // the skeleton stream will be used to move the camera.

            // Data flow outline:
            // [GestureServiceEndpoint]-----Skeleton----->[PalmSmoother]-----SmoothedPositionDelta----->[SphericalCamera]

            // We would like to translate hand motion to camera motion. Because the palm position 
            // values in the skeleton stream contain some jitter, we smooth them (using a moving-average window).
            _palmSmoother = new PalmSmoother();

            // Define the gesture so that when the hand forms a pinch pose register to the skeleton stream and pass it to the palm smoother.
            var pinchClose = new PinchPose("PinchClosePose", pinchSpread: false);
            pinchClose.Triggered += (s, args) => _gesturesService.RegisterToSkeleton((s1, args1) => _palmSmoother.Smooth(args1.DefaultHandSkeleton));
            _cameraPinch = new Gesture("CameraPinch", new PinchPose("PinchOpenPose", pinchSpread: true), 
                                                      pinchClose, 
                                                      new PinchPose("PinchReleasePose", pinchSpread: true));
            _cameraPinch.IdleTriggered += (s, args) => _gesturesService.UnregisterFromSkeleton();

            // The smoothed palm position is used to rotate the camera.
            _sphericalCamera = new SphericalCamera(Camera, Dispatcher);
            _palmSmoother.SmoothedPositionChanged += (s, args) => _sphericalCamera.UpdateCamera(args.SmoothedPositionDelta);

            // Connect to the Gestures Service and register the gesture we've defined
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, arg) => Dispatcher.Invoke(() => GesturesServiceStatus.Text = $"[{arg.Status}]");
            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_cameraPinch);
        }

        private void OnAnimatedHelpEnded(object sender, RoutedEventArgs e)
        {
            animatedHelp.Position = new TimeSpan(0, 0, 1);
            animatedHelp.Play();
        }
    }
}