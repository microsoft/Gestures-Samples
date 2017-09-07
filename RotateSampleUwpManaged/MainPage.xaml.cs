using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RotateSampleUwpManaged
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _rotateGesture;
        private int _rotateTimes = 0;

        public MainPage()
        {
            this.InitializeComponent();
            Loaded += PageLoaded;
        }

        private async void PageLoaded(object sender, RoutedEventArgs e)
        {
            // Step 1: Connect to Microsoft Gestures Detection service  
            _gesturesService = GesturesServiceEndpointFactory.Create();
            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            _gesturesService.StatusChanged += async (s, arg) => await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => GesturesServiceStatus.Text = $"[{arg.Status}]");
            Unloaded += async (s, arg) =>
            {
                await _gesturesService?.Disconnect();
                _gesturesService?.Dispose();
            };
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
            _rotateGesture.Triggered += async (s, args) => await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var rotateTransform = new RotateTransform { CenterX = Arrow.ActualWidth / 2, CenterY = Arrow.ActualHeight / 2 };
                rotateTransform.Angle = ++_rotateTimes * 90;
                Arrow.RenderTransform = rotateTransform;
             });

            // Step 3: Register the gesture (When window focus is lost/gained the service will effectively change the gesture registration automatically)
            //         To manually control the gesture registration, pass 'isGlobal: true' parameter in the function call below
            await _gesturesService.RegisterGesture(_rotateGesture);
        }        
    }
}
