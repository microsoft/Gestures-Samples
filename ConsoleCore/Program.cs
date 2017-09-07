using System;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net;
using System.Linq;

namespace ConsoleManaged
{
    class Program
    {
        private static GesturesServiceEndpoint _gesturesService;
        private static Gesture _rotateGesture;
        private static Gesture _dropTheMicGesture;
        private static Gesture _likeGesture;

        static void Main(string[] args)
        {       
            Console.Title = "GesturesServiceStatus[Initializing]";
            Console.WriteLine("Welcome to Microsoft Gestures Console ! press 'ctrl+c' to exit");

            string gesturesServiceHostName = !args.Any() ? "localhost" : args[0];
            RegisterGestures(gesturesServiceHostName).Wait();
            Console.ReadKey();
        }

        private static async Task RegisterGestures(string gesturesServiceHostName)
        {
            // Step 1: Connect to Microsoft Gestures Detection service            
            _gesturesService = GesturesServiceEndpointFactory.Create(gesturesServiceHostName);            
            _gesturesService.StatusChanged += (s, arg) => Console.Title = $"GesturesServiceStatus[{arg.Status}]";            
            await _gesturesService.ConnectAsync();

            // Step 2: Define bunch of custom Gestures, each detection of the gesture will emit some message into the console
            await RegisterRotateRightGesture();
            await RegisterDropTheMicGesture();
            await RegisterLikeGesture();
        }

        private static async Task RegisterRotateRightGesture()
        {
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
            _rotateGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);
            
            // Step 3: Register the gesture             
            await _gesturesService.RegisterGesture(_rotateGesture, isGlobal: true);
        }

        private static async Task RegisterDropTheMicGesture()
        {
            // Our starting pose is a full fist pointing down and/or forward
            var fist = new HandPose("Fist", new PalmPose(new AnyHandContext(), PoseDirection.Down | PoseDirection.Forward),
                                                new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring }, FingerFlexion.Folded));
            // Finazling pose is when the fist is being "open" AKA spread
            var spread = new HandPose("Spread", new PalmPose(new AnyHandContext(), PoseDirection.Down | PoseDirection.Forward),
                                                new FingerPose(new[] { Finger.Thumb, Finger.Index, Finger.Middle, Finger.Ring }, FingerFlexion.Open));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: fist -> spread
            _dropTheMicGesture = new Gesture("DropTheMic", fist, spread);            
            _dropTheMicGesture.Triggered += (s,e) => OnGestureDetected(s, e, ConsoleColor.Blue);

            await _gesturesService.RegisterGesture(_dropTheMicGesture, isGlobal: true);
        }

        private static async Task RegisterLikeGesture()
        {
            // Our starting pose is a fist 
            var fist = new HandPose("Fist", new PalmPose(new AnyHandContext(), PoseDirection.Left | PoseDirection.Right),
                                            new FingerPose(new AllFingersContext(), FingerFlexion.Folded));
            // Finazling pose is when the thumb flexion will open in the "up" direction
            var like = new HandPose("Like", new PalmPose(new AnyHandContext(), PoseDirection.Left | PoseDirection.Right),
                                            new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring, Finger.Pinky }, FingerFlexion.Folded),
                                            new FingerPose(Finger.Thumb, FingerFlexion.Open, PoseDirection.Up));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine: fist -> Like
            _likeGesture = new Gesture("LikeGesture", fist, like);
            _likeGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.White);

            await _gesturesService.RegisterGesture(_likeGesture, isGlobal: true);
        }

        private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor color)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Gesture detected! : ");
            Console.ForegroundColor = color;
            Console.WriteLine(args.GestureSegment.Name);
            Console.ResetColor();
        }

        private static string GetFqdn()
        {
            var domainName = IPGlobalProperties.GetIPGlobalProperties().DomainName;
            var hostName = Dns.GetHostName();

            domainName = "." + domainName;
            if (!hostName.EndsWith(domainName))  // if hostname does not already include domain name
            {
                hostName += domainName;   // add the domain name part
            }

            return hostName;                    // return the fully qualified name
        }
    }
}
