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
        private Gesture _rockGesture;
        private Gesture _paperGesture;
        private Gesture _scissorsGesture;

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
            // Step1: Connect to Gesture Detection Service and route StatusChanged event to the UI
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (oldStatus, newStatus) => GesturesDetectionStatusChanged?.Invoke(oldStatus, newStatus);

            // Step2: Define the Rock-Paper-Scissors gestures
            // One for 'Rock'...
            var rockPose = new HandPose("RockPose", new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            _rockGesture = new Gesture("RockGesture", rockPose);
            _rockGesture.Triggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.Rock); 

            // ...another for 'Paper'...
            var paperPose = new HandPose("PaperPose", new PalmPose(new AnyHandContext(), PoseDirection.Left | PoseDirection.Right, PoseDirection.Forward),
                                                      new FingerPose(new AllFingersContext(), FingerFlexion.Open));
            _paperGesture = new Gesture("PaperGesture", paperPose);
            _paperGesture.Triggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.Paper);

            // ...and last one for 'Scissors'...
            var scissorsPose = new HandPose("ScissorsPose", new FingerPose(new[] { Finger.Index, Finger.Middle }, FingerFlexion.Open),
                                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Middle),
                                                            new FingerPose(new[] { Finger.Ring, Finger.Pinky }, FingerFlexion.Folded));
            _scissorsGesture = new Gesture("ScissorsGesture", scissorsPose);
            _scissorsGesture.Triggered += (s, arg) => UserStrategyChanged?.Invoke(GameStrategy.Scissors);

            await _gesturesService.ConnectAsync();
            await _gesturesService.RegisterGesture(_rockGesture);
            await _gesturesService.RegisterGesture(_paperGesture);
            await _gesturesService.RegisterGesture(_scissorsGesture);
        }

        public void Dispose() => _gesturesService?.Dispose();
    }
}
