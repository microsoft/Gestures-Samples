using Microsoft.Gestures;
using Microsoft.Gestures.Endpoint;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Gestures.Samples.RockPaperScissors
{
    public enum GameStrategy : byte
    {
        Rock,
        Paper,
        Scissors,
        None
    };

    public delegate void UserStrategyChangedHandler(GameStrategy newStrategy);

    public sealed class GesturesRockPaperScissors : IDisposable
    {
        private const int StrategyStabilizationTimeout = 400;

        private volatile uint _round = 0;
        private volatile GameStrategy _lastStategy = GameStrategy.None;

        private GesturesServiceEndpoint _gesturesService;
        private Gesture _gameGesture;

        public event StatusChangedHandler GesturesDetectionStatusChanged = (oldStatus, newStatus) => { }; 
        public event UserStrategyChangedHandler UserStrategyChanged = (s) => { };
        public event UserStrategyChangedHandler UserStrategyFinal = (s) => { };

        public static GameStrategy WinningStrategy(GameStrategy userStrategy)
        {
            switch (userStrategy)
            {
                case GameStrategy.Rock:     return GameStrategy.Paper;
                case GameStrategy.Paper:    return GameStrategy.Scissors;
                case GameStrategy.Scissors: return GameStrategy.Rock;
                default:                    return GameStrategy.None;
            }
        }

        public async Task Init()
        {
            // Step1: Connect to Gesture Detection Service and route StatusChanged event to the UI
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (oldStatus, newStatus) => GesturesDetectionStatusChanged(oldStatus, newStatus);

            // Step2: Define the Rock-Paper-Scissors gesture
            // Start with the initial fist pose...
            var rockPose = new HandPose("RockPose", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            rockPose.Triggered += (s, arg) => _lastStategy = GameStrategy.None;

            // ...define the shaking motion of the fist up and down three times...
            var upDownX3Motion = new HandMotion("UpAndDownX3", new PalmMotion(VerticalMotionSegment.Upward, VerticalMotionSegment.Downward,
                                                                              VerticalMotionSegment.Upward, VerticalMotionSegment.Downward,
                                                                              VerticalMotionSegment.Upward, VerticalMotionSegment.Downward));
            upDownX3Motion.Triggered += (s, arg) => InvokeUserStrategyChanged(GameStrategy.Rock);

            // ...define the Paper Pose...
            var paperPose = new HandPose("PaperPose", new PalmPose(new AnyHandContext(), PoseDirection.Left | PoseDirection.Right, PoseDirection.Forward),
                                                      new FingerPose(new AllFingersContext(), FingerFlexion.Open));
            paperPose.Triggered += (s, arg) => InvokeUserStrategyChanged(GameStrategy.Paper);

            // ...define the Scissors Pose...
            var scissorsPose = new HandPose("ScissorsPose", new FingerPose(new[] { Finger.Index, Finger.Middle }, FingerFlexion.Open),
                                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Middle),
                                                            new FingerPose(new[] { Finger.Ring, Finger.Pinky }, FingerFlexion.Folded));
            scissorsPose.Triggered += (s, arg) => InvokeUserStrategyChanged(GameStrategy.Scissors);

            // This is an artificial 'garbage' state - we need it to allow the user a delay to transition after the upDownX3Motion from fist to scissors/paper.
            // Otherwise, the gesture would trigger and will never get the chance to transition after the motion to one of the non fist poses.
            // Effectively, we are not interested in this gesture's trigger event but rather only its intermediate states' trigger events.
            var resetPose = new HandPose("ResetPose", new PalmPose(new AnyHandContext(), direction: PoseDirection.Forward, orientation: PoseDirection.Up),
                                                      new FingerPose(new AllFingersContext(), FingerFlexion.Open));

            // ...construct the game gesture...
            _gameGesture = new Gesture("RockPaperScissor", rockPose, upDownX3Motion, resetPose);
            _gameGesture.AddSubPath(upDownX3Motion, paperPose, resetPose);
            _gameGesture.AddSubPath(upDownX3Motion, scissorsPose, resetPose);

            _gameGesture.IdleTriggered += (s, arg) => { if (_lastStategy == GameStrategy.None) InvokeUserStrategyFinal(_round, GameStrategy.None); };

            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_gameGesture);
        }

        public void Dispose() => _gesturesService?.Dispose();

        private void InvokeUserStrategyChanged(GameStrategy newUserStrategy)
        {
            StabilizeUserStrategy(newUserStrategy);
            UserStrategyChanged(newUserStrategy);
        }

        private void StabilizeUserStrategy(GameStrategy newUserStrategy)
        {
            Debug.Assert(newUserStrategy != GameStrategy.None);
            if (newUserStrategy == GameStrategy.Rock)
            {
                var currentRound = _round;
                // Give the user a short grace period to change the default rock pose to one of the other poses before calling it 'Rock'
                Task.Delay(StrategyStabilizationTimeout).ContinueWith(t => InvokeUserStrategyFinal(currentRound, newUserStrategy) );
            }
            else
            {
                InvokeUserStrategyFinal(_round, newUserStrategy);
            }
        }

        private void InvokeUserStrategyFinal(uint round, GameStrategy finalStrategy)
        {
            // if this round was already finalized ignore any update
            if (round != _round) return;

            _round++;
            _lastStategy = finalStrategy;
            UserStrategyFinal(finalStrategy);
        }
    }
}
