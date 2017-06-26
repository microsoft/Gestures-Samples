using System;
using System.Windows;
using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures.Stock.Gestures;
using Microsoft.Gestures.Stock.HandPoses;

namespace Microsoft.Gestures.Samples.VideoPlayerGestureControl
{
    public partial class MainWindow : System.Windows.Window
    {
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _rewindGesture;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += WindowLoaded;
        }

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // Step 1: Connect to Microsoft Gestures Detection service
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, args) => Dispatcher.Invoke(() => GeturesServiceStatus.Text = $"[{args.Status}]");
            Closed += (s, args) => _gesturesService?.Dispose();
            await _gesturesService.ConnectAsync();

            // Step 2: Define the RewindGesture gesture as follows:
            //  ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐    ┌──────────┐
            //  │          │    │          │    │          │    │          │    │          │    │          │    │          │ 
            //  │   Idle   │ -> │  Spread  │ -> │  Pause   │ -> │  Rewind  │ -> │KeepRewind│ -> │ Release  │ -> │   Idle   │ 
            //  │          │    │(unpinch) │    │ (pinch)  │    │  (left)  │    │ (pinch)  │    │(unpinch) │    │          │ 
            //  └──────────┘    └──────────┘    └────┬─────┘    └──────────┘    └──────────┘    └──────────┘    └──────────┘
            //                                       │                                               ^
            //                                       └───────────────────────────────────────────────┘
            //
            // When ever the gesture returns to Idle state it will always resume playback
            //
            var spreadPose = GeneratePinchPose("Spread", true);
            var pausePose = GeneratePinchPose("Pause");
            pausePose.Triggered += (s, args) => Dispatcher.Invoke(() => VideoStatus.Text = "⏸");

            var rewindMotion = new HandMotion("Back", new PalmMotion(VerticalMotionSegment.Left));
            rewindMotion.Triggered += (s, args) => Dispatcher.Invoke(() => VideoStatus.Text = "⏪");

            var keepRewindingPose = GeneratePinchPose("KeepRewind");
            var releasePose = GeneratePinchPose("Release", true);
            
            // Then define the gesture by concatenating the previous objects to form a simple state machine
            _rewindGesture = new Gesture("RewindGesture", spreadPose, pausePose, rewindMotion, keepRewindingPose, releasePose);
            // Detect if the user releases his pinch-grab and return to playback
            _rewindGesture.AddSubPath(pausePose, releasePose);
            
            // Continue playing the video when the gesture resets (either successful or aborted)
            _rewindGesture.Triggered += (s, args) => Dispatcher.Invoke(() => VideoStatus.Text = "▶");
            _rewindGesture.IdleTriggered += (s, args) => Dispatcher.Invoke(() => VideoStatus.Text = "▶");

            // Step 3: Register the gesture (When window focus is lost/gained the service will effectively change the gesture registration automatically)
            //         To manually control the gesture registration, pass 'isGlobal: true' parameter in the function call below
            await _gesturesService.RegisterGesture(_rewindGesture);
        }

        private HandPose GeneratePinchPose(string name, bool pinchSpread = false)
        {
            var pinchingFingers = new[] { Finger.Thumb, Finger.Index };
            var openFingersContext = pinchSpread ? new AllFingersContext(pinchingFingers) as FingersContext : new AnyFingerContext(pinchingFingers) as FingersContext;
            return new HandPose(name, new FingerPose(openFingersContext, FingerFlexion.Open),
                                      new FingertipDistanceRelation(pinchingFingers, pinchSpread ? RelativeDistance.NotTouching : RelativeDistance.Touching),
                                      new FingertipDistanceRelation(pinchingFingers, RelativeDistance.NotTouching, Finger.Middle));
        }
    }
}
