using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures.Stock.Gestures;
using Microsoft.Gestures.Stock.HandPoses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Gestures.Samples.CarGestures
{
    public class CarGesturesManager : IDisposable
    {
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _dismissNotificationGesture;

        private Gesture _answerCallGesture;
        private Gesture _hangUpGesture;

        private Gesture _selectSourceGesture;

        private Gesture _volumeUpGesture;
        private Gesture _volumeDownGesture;
        private Gesture _volumeToggleMuteGesture;

        private Gesture _tempratureUpGesture;
        private Gesture _tempratureDownGesture;

        private Gesture _nextChannelGesture;

        public event StatusChangedHandler StatusChanged;

        public event EventHandler<GestureSegmentTriggeredEventArgs> DismissNotification;

        public event EventHandler<GestureSegmentTriggeredEventArgs> NextChannel;

        public event EventHandler<GestureSegmentTriggeredEventArgs> AnswerCall;
        public event EventHandler<GestureSegmentTriggeredEventArgs> HangUpCall;

        public event EventHandler<GestureSegmentTriggeredEventArgs> VolumeUp;
        public event EventHandler<GestureSegmentTriggeredEventArgs> VolumeDown;
        public event EventHandler<GestureSegmentTriggeredEventArgs> VolumeToggleMute;

        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourcePhone;
        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourceRadio;
        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourceMedia;
        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourceUsb;

        public event EventHandler<GestureSegmentTriggeredEventArgs> TemperatureUp;
        public event EventHandler<GestureSegmentTriggeredEventArgs> TemperatureDown;


        public async Task Initialize()
        {
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, args) => StatusChanged?.Invoke(s, args);
            await _gesturesService.ConnectAsync();

            _dismissNotificationGesture = new DismissGesture("DismissCall");
            _dismissNotificationGesture.Triggered += async (s, args) => {
                                                                         DismissNotification?.Invoke(s, args);
                                                                         await _gesturesService.UnregisterGesture(_answerCallGesture);
                                                                         await _gesturesService.UnregisterGesture(_dismissNotificationGesture);
                                                                       };
            // Phone Gestures
            _hangUpGesture = new HangUpGesture("HangUpCall");
            _hangUpGesture.Triggered += async (s, args) => {
                                                            HangUpCall?.Invoke(s, args);
                                                            await _gesturesService.UnregisterGesture(_hangUpGesture);
                                                          };

            _answerCallGesture = new Gesture("AnswerCall", new OnPhonePose("OnPhoneDown", PoseDirection.Down), new OnPhonePose("OnPhoneLeft", PoseDirection.Left));
            _answerCallGesture.Triggered += async (s, args) => {
                                                                AnswerCall?.Invoke(s, args);
                                                                await _gesturesService.UnregisterGesture(_answerCallGesture);
                                                                await _gesturesService.UnregisterGesture(_dismissNotificationGesture);
                                                                await Task.Delay(1000).ContinueWith(async t =>
                                                                {
                                                                    await _gesturesService.RegisterGesture(_hangUpGesture);
                                                                });
                                                              };

            // Source Selection Gestures
            var fist = new FistPose("Fist", PoseDirection.Forward);
            var selectSourcePhonePose = GenerateOpenFingersPose("SelectSourcePhone", new[] { Finger.Index });
            selectSourcePhonePose.Triggered += (s, args) => SelectSourcePhone?.Invoke(s, args);
            var selectSourceRadioPose = GenerateOpenFingersPose("SelectSourceRadio", new[] { Finger.Index, Finger.Middle });
            selectSourceRadioPose.Triggered += (s, args) => SelectSourceRadio?.Invoke(s, args);
            var selectSourceMediaPose = GenerateOpenFingersPose("SelectSourceMedia", new[] { Finger.Index, Finger.Middle, Finger.Ring });
            selectSourceMediaPose.Triggered += (s, args) => SelectSourceMedia?.Invoke(s, args);
            var selectSourceUsbPose = GenerateOpenFingersPose("SelectSourceUsb", new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky });
            selectSourceUsbPose.Triggered += (s, args) => SelectSourceUsb?.Invoke(s, args);

            _selectSourceGesture = new Gesture("SelectSource", fist, selectSourcePhonePose);
            _selectSourceGesture.AddTriggeringPath(fist, selectSourceRadioPose);
            _selectSourceGesture.AddTriggeringPath(fist, selectSourceMediaPose);
            _selectSourceGesture.AddTriggeringPath(fist, selectSourceUsbPose);
            await _gesturesService.RegisterGesture(_selectSourceGesture);

            // Volume Gestures
            _volumeToggleMuteGesture = new Gesture("VolumeToggleMute", new ClamPose("ClamOpen", clamOpen: true), new ClamPose("ClamClose", clamOpen: false));
            _volumeToggleMuteGesture.Triggered += (s, args) => VolumeToggleMute?.Invoke(s, args);
            await _gesturesService.RegisterGesture(_volumeToggleMuteGesture);
            _volumeUpGesture = new Gesture("VolumeUp", new PointingFingerPose("Point"), new HandMotion("ClockwiseCircle", new[] { VerticalMotionSegment.ClockwiseArcRightUpward, VerticalMotionSegment.ClockwiseArcRightDownward, VerticalMotionSegment.ClockwiseArcLeftDownward, VerticalMotionSegment.ClockwiseArcLeftUpward }));
            _volumeUpGesture.Triggered += (s, args) => VolumeUp?.Invoke(s, args);
            await _gesturesService.RegisterGesture(_volumeUpGesture);
            _volumeDownGesture = new Gesture("VolumeDown", new PointingFingerPose("Point"), new HandMotion("CounterClockwiseCircle", new[] { VerticalMotionSegment.CounterClockwiseArcRightDownward, VerticalMotionSegment.CounterClockwiseArcRightUpward, VerticalMotionSegment.CounterClockwiseArcLeftUpward, VerticalMotionSegment.CounterClockwiseArcLeftDownward }));
            _volumeDownGesture.Triggered += (s, args) => VolumeDown?.Invoke(s, args);
            await _gesturesService.RegisterGesture(_volumeDownGesture);

            // Next Channel
            _nextChannelGesture = GenerateSwipeGesure("SwipeLeft", PoseDirection.Left);
            _nextChannelGesture.Triggered += (s, args) => NextChannel?.Invoke(s, args);
            await _gesturesService.RegisterGesture(_nextChannelGesture);

            // Control A/C
            _tempratureUpGesture = GenerateSwipeGesure("SwipeUp", PoseDirection.Up);
            _tempratureUpGesture.Triggered += (s, args) => TemperatureUp?.Invoke(s, args);
            await _gesturesService.RegisterGesture(_tempratureUpGesture);
            _tempratureDownGesture = GenerateSwipeGesure("SwipeDown", PoseDirection.Down);
            _tempratureDownGesture.Triggered += (s, args) => TemperatureDown?.Invoke(s, args);
            await _gesturesService.RegisterGesture(_tempratureDownGesture);
        }

        public async Task IncomingCall()
        {
            await _gesturesService.RegisterGesture(_dismissNotificationGesture);
            await _gesturesService.RegisterGesture(_answerCallGesture);
        }

        public void Dispose()
        {
            _gesturesService?.Dispose();
        }

        private HandPose GenerateOpenFingersPose(string name, Finger[] fingers)
        {
            var nonThumbOtherFingers = (new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }).Except(fingers);
            var fingersOpenPose = new HandPose(name, new PalmPose(new AnyHandContext(), PoseDirection.Forward),
                                                     new FingerPose(fingers, FingerFlexion.Open, PoseDirection.Up),
                                                     new FingertipDistanceRelation(fingers, RelativeDistance.NotTouching, nonThumbOtherFingers.Union(new[] { Finger.Thumb })));
            if (nonThumbOtherFingers.Any()) fingersOpenPose.PoseConstraints.Add(new FingerPose(nonThumbOtherFingers, PoseDirection.Down | PoseDirection.Forward | PoseDirection.Backward));
            return fingersOpenPose;
        }

        private Gesture GenerateSwipeGesure(string name, PoseDirection direction)
        {
            var fingerSet = new HandPose("FingersSet", new PalmPose(new AnyHandContext(), direction),
                                                       new FingertipDistanceRelation(Finger.Index, RelativeDistance.Touching, Finger.Middle),
                                                       new FingerPose(Finger.Index, PoseDirection.Forward),
                                                       new FingerPose(Finger.Middle, PoseDirection.Forward));

            var fingersBent = new HandPose("FingersBent", new PalmPose(new AnyHandContext(), direction),
                                                          new FingertipDistanceRelation(Finger.Index, RelativeDistance.Touching, Finger.Middle),
                                                          new FingerPose(Finger.Index, direction),
                                                          new FingerPose(Finger.Middle, direction));

            var swipeGesture = new Gesture(name, fingerSet, fingersBent);
            swipeGesture.AddTriggeringPath(fingerSet, new FistPose("Fist", direction));
            return swipeGesture;
        }
    }
}
