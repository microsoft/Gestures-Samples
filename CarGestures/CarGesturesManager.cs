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

        public event StatusChangedHandler StatusChanged = (s, arg) => { };

        public event EventHandler<GestureSegmentTriggeredEventArgs> DismissNotification = (s, arg) => { };

        public event EventHandler<GestureSegmentTriggeredEventArgs> NextChannel = (s, arg) => { };

        public event EventHandler<GestureSegmentTriggeredEventArgs> AnswerCall = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> HangUpCall = (s, arg) => { };

        public event EventHandler<GestureSegmentTriggeredEventArgs> VolumeUp = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> VolumeDown = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> VolumeToggleMute = (s, arg) => { };

        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourcePhone = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourceRadio = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourceMedia = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> SelectSourceUsb = (s, arg) => { };

        public event EventHandler<GestureSegmentTriggeredEventArgs> TemperatureUp = (s, arg) => { };
        public event EventHandler<GestureSegmentTriggeredEventArgs> TemperatureDown = (s, arg) => { };


        public async Task Initialize()
        {
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, arg) => StatusChanged(s, arg);
            await _gesturesService.ConnectAsync();

            _dismissNotificationGesture = new DismissGesture("DismissCall");
            _dismissNotificationGesture.Triggered += async (s, arg) => {
                                                                         DismissNotification(s, arg);
                                                                         await _gesturesService.UnregisterGesture(_answerCallGesture);
                                                                         await _gesturesService.UnregisterGesture(_dismissNotificationGesture);
                                                                       };
            // Phone Gestures
            _hangUpGesture = new HangUpGesture("HangUpCall");
            _hangUpGesture.Triggered += async (s, arg) => {
                                                            HangUpCall(s, arg);
                                                            await _gesturesService.UnregisterGesture(_hangUpGesture);
                                                          };

            _answerCallGesture = new Gesture("AnswerCall", new OnPhonePose("OnPhoneDown", PoseDirection.Down), new OnPhonePose("OnPhoneLeft", PoseDirection.Left));
            _answerCallGesture.Triggered += async (s, arg) => {
                                                                AnswerCall(s, arg);
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
            selectSourcePhonePose.Triggered += (s, arg) => SelectSourcePhone(s, arg);
            var selectSourceRadioPose = GenerateOpenFingersPose("SelectSourceRadio", new[] { Finger.Index, Finger.Middle });
            selectSourceRadioPose.Triggered += (s, arg) => SelectSourceRadio(s, arg);
            var selectSourceMediaPose = GenerateOpenFingersPose("SelectSourceMedia", new[] { Finger.Index, Finger.Middle, Finger.Ring });
            selectSourceMediaPose.Triggered += (s, arg) => SelectSourceMedia(s, arg);
            var selectSourceUsbPose = GenerateOpenFingersPose("SelectSourceUsb", new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky });
            selectSourceUsbPose.Triggered += (s, arg) => SelectSourceUsb(s, arg);

            _selectSourceGesture = new Gesture("SelectSource", fist, selectSourcePhonePose);
            _selectSourceGesture.AddTriggeringPath(fist, selectSourceRadioPose);
            _selectSourceGesture.AddTriggeringPath(fist, selectSourceMediaPose);
            _selectSourceGesture.AddTriggeringPath(fist, selectSourceUsbPose);
            await _gesturesService.RegisterGesture(_selectSourceGesture);

            // Volume Gestures
            _volumeToggleMuteGesture = new Gesture("VolumeToggleMute", new ClamPose("ClamOpen", clamOpen: true), new ClamPose("ClamClose", clamOpen: false));
            _volumeToggleMuteGesture.Triggered += (s, arg) => VolumeToggleMute(s, arg);
            await _gesturesService.RegisterGesture(_volumeToggleMuteGesture);
            _volumeUpGesture = new Gesture("VolumeUp", new PointingFingerPose("Point"), new HandMotion("ClockwiseCircle", new[] { VerticalMotionSegment.ClockwiseArcRightUpward, VerticalMotionSegment.ClockwiseArcRightDownward, VerticalMotionSegment.ClockwiseArcLeftDownward, VerticalMotionSegment.ClockwiseArcLeftUpward }));
            _volumeUpGesture.Triggered += (s, arg) => VolumeUp(s, arg);
            await _gesturesService.RegisterGesture(_volumeUpGesture);
            _volumeDownGesture = new Gesture("VolumeDown", new PointingFingerPose("Point"), new HandMotion("CounterClockwiseCircle", new[] { VerticalMotionSegment.CounterClockwiseArcRightDownward, VerticalMotionSegment.CounterClockwiseArcRightUpward, VerticalMotionSegment.CounterClockwiseArcLeftUpward, VerticalMotionSegment.CounterClockwiseArcLeftDownward }));
            _volumeDownGesture.Triggered += (s, arg) => VolumeDown(s, arg);
            await _gesturesService.RegisterGesture(_volumeDownGesture);

            // Next Channel
            _nextChannelGesture = GenerateSwipeGesure("SwipeLeft", PoseDirection.Left);
            _nextChannelGesture.Triggered += (s, arg) => NextChannel(s, arg);
            await _gesturesService.RegisterGesture(_nextChannelGesture);

            // Control A/C
            _tempratureUpGesture = GenerateSwipeGesure("SwipeUp", PoseDirection.Up);
            _tempratureUpGesture.Triggered += (s, arg) => TemperatureUp(s, arg);
            await _gesturesService.RegisterGesture(_tempratureUpGesture);
            _tempratureDownGesture = GenerateSwipeGesure("SwipeDown", PoseDirection.Down);
            _tempratureDownGesture.Triggered += (s, arg) => TemperatureDown(s, arg);
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
            var fingerSet = new IndexMiddleSwipePose("FingersSet", othersFolded: false, direction: direction);
            var swipeGesture = new Gesture(name, fingerSet, new IndexMiddleSwipePose("FingersBend", isIndexSwiped: true, isMiddleSwiped: true, direction: direction, othersFolded: false));
            swipeGesture.AddTriggeringPath(fingerSet, new FistPose("Fist", direction));
            return swipeGesture;
        }
    }
}
