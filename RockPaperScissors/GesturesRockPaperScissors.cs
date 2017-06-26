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

    public delegate void StartRoundHandler(uint roundNum);
    public delegate void UserStrategyChangedHandler(GameStrategy newStrategy);

    public sealed class GesturesRockPaperScissors : IDisposable
    {
        private GesturesServiceEndpoint _gesturesService;
        private Gesture _gameGesture;

        public event StatusChangedHandler GesturesDetectionStatusChanged; 
        public event UserStrategyChangedHandler UserStrategyChanged;

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
            // Step1: Define the Rock-Paper-Scissors gestures
            // Create a pose for 'Rock'...
            var rockPose = new HandPose("RockPose", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            rockPose.Triggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.Rock); 

            // ...another for 'Paper'...
            var paperPose = new HandPose("PaperPose", new PalmPose(new AnyHandContext(), PoseDirection.Left | PoseDirection.Right, PoseDirection.Forward),
                                                      new FingerPose(new AllFingersContext(), FingerFlexion.Open));
            paperPose.Triggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.Paper);

            // ...and last one for 'Scissors'...
            var scissorsPose = new HandPose("ScissorsPose", new FingerPose(new[] { Finger.Index, Finger.Middle }, FingerFlexion.Open),
                                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Middle),
                                                            new FingerPose(new[] { Finger.Ring, Finger.Pinky }, FingerFlexion.Folded));
            scissorsPose.Triggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.Scissors);

            // ...a PassThroughtGestureSegment is a structural gesture segment that provides a way to simplify a gesture state machine construction by 'short-circuiting' 
            // between gesture segments connectd to it and gesture segements it connects to. It helps reduce the number of SubPaths that needs to be defined.
            // Very handy when you need to define a Clique (see https://en.wikipedia.org/wiki/Clique_(graph_theory)#1)
            // as in this case where Rock, Paper and Scissors are all connected to each other...
            var epsilonState = new PassThroughGestureSegment("Epsilon");
            
            // ...this pose is an artificial stop pose. Namely, we want to keep the gesture detector in one of the pose states without ending the gesture so we add this
            // pose as a pose that completes the gesture assuming the user will not perform it frequently. 
            // As long as the user continues to perform the 'Rock', 'Paper' or 'Scissors' poses we will remain within the gesture's state machine.
            var giveUpPose = new HandPose("GiveUpPose", new PalmPose(new AnyHandContext(), PoseDirection.Forward, PoseDirection.Up),
                                                        new FingerPose(new AllFingersContext(), FingerFlexion.Open));

            _gameGesture = new Gesture("RockPaperScissorGesture", epsilonState, giveUpPose);
            // ...add a sub path back and forth from the PassthroughGestureSegment to the various poses
            _gameGesture.AddSubPath(epsilonState, rockPose, epsilonState);
            _gameGesture.AddSubPath(epsilonState, paperPose, epsilonState);
            _gameGesture.AddSubPath(epsilonState, scissorsPose, epsilonState);

            // In case the user performs a pose that is not one of the game poses the gesture resets and this event will trigger
            _gameGesture.IdleTriggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.None);

            // Step2: Connect to Gesture Detection Service, route StatusChanged event to the UI and register the gesture
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (oldStatus, newStatus) => GesturesDetectionStatusChanged?.Invoke(oldStatus, newStatus);
            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_gameGesture);
        }

        public void Dispose() => _gesturesService?.Dispose();
    }
}
