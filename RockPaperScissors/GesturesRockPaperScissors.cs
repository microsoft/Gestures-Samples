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

    public delegate void StartRaoundHandler(uint roundNum);
    public delegate void UserStrategyChangedHandler(GameStrategy newStrategy);

    public sealed class GesturesRockPaperScissors : IDisposable
    {
        private const int StrategyStabilizationTimeout = 400;
        private const int StartNewRoundAfterRockTimeout = 1000;

        private volatile uint _round = 1;
        private volatile GameStrategy _lastStategy = GameStrategy.None;

        private GesturesServiceEndpoint _gesturesService;
        private Gesture _gameGesture;

        public event StatusChangedHandler GesturesDetectionStatusChanged = (oldStatus, newStatus) => { }; 
        public event UserStrategyChangedHandler UserStrategyChanged = (s) => { };
        public event UserStrategyChangedHandler UserStrategyFinal = (s) => { };
        public event StartRaoundHandler StartRound = (r) => { };

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
            rockPose.Triggered += (s, arg) => InvokeStartRound(); 

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

            // ...construct the game gesture...
            _gameGesture = new Gesture("RockPaperScissor", rockPose, upDownX3Motion, paperPose);
            _gameGesture.AddSubPath(upDownX3Motion, scissorsPose, _gameGesture.IdleGestureSegment);

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

            // If it is 'Scissors'/'Paper' call it immediately
            if (newUserStrategy != GameStrategy.Rock) InvokeUserStrategyFinal(_round, newUserStrategy);
            else
            {
                var currentRound = _round;
                // Give the user a short grace period to change the default 'Rock' pose to one of the other poses before calling it 'Rock'
                Task.Delay(StrategyStabilizationTimeout)
                    .ContinueWith(t =>
                    {
                        if (currentRound == _round)
                        {
                            InvokeUserStrategyFinal(currentRound, newUserStrategy);
                            // Force a gesture reset after 'Rock' - the user may perform a(n unwanted) transition to 'Paper'/'Scissors' and also we want to start a new round
                            Task.Delay(StartNewRoundAfterRockTimeout).ContinueWith(t2 => { _gesturesService.UnregisterGesture(_gameGesture).ContinueWith(t3 => _gesturesService.RegisterGesture(_gameGesture)); });
                        }
                    });
            }
        }

        private void InvokeUserStrategyFinal(uint round, GameStrategy finalStrategy)
        {
            _round++;
            _lastStategy = finalStrategy;
            UserStrategyFinal(finalStrategy);
        }

        private void InvokeStartRound()
        {
            _lastStategy = GameStrategy.None;
            StartRound(_round);
        }
    }
}
