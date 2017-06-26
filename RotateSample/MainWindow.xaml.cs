using System.Windows;
using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using System.Windows.Media;

namespace Microsoft.Gestures.Samples.RotateSample
{
    public partial class MainWindow : System.Windows.Window
    {
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _rotateGesture;
        private int _rotateTimes = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += WindowLoaded;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Step 1: Connect to Microsoft Gestures Detection service
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, arg) => Dispatcher.Invoke(() => GesturesServiceStatus.Text = $"[{arg.Status}]");
            Closed += (s, arg) => _gesturesService?.Dispose();
            await _gesturesService.ConnectAsync();

            // Step 2: Define your custom gesture 
            // Start with defining the first pose, ...
            var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                            new FingertipPlacementRelation(Finger.Index, RelativePlacement.Above, Finger.Thumb));
            // ... define the second pose, ...
            var rotate = new HandPose("Rotate", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                                new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                                new FingertipPlacementRelation(Finger.Index, RelativePlacement.Right, Finger.Thumb));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
            _rotateGesture = new Gesture("RotateRight", hold, rotate);
            _rotateGesture.Triggered += (s, args) => Dispatcher.Invoke(() => Arrow.RenderTransform = new RotateTransform(++_rotateTimes * 90, Arrow.ActualWidth / 2, Arrow.ActualHeight / 2));  

            // Step 3: Register the gesture (When window focus is lost/gained the service will effectively change the gesture registration automatically)
            //         To manually control the gesture registration, pass 'isGlobal: true' parameter in the function call below
            await _gesturesService.RegisterGesture(_rotateGesture);
        }
    }
}
