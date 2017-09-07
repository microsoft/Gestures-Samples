#include "GesturesProxyNative/GesturesServiceEndpoint.hpp"
#include "GesturesNative/Gesture.hpp"
#include "GesturesCommonNative/Console/Console.hpp"
using namespace Microsoft::Gestures::Endpoint;
using namespace Microsoft::Gestures;
using namespace Microsoft::Gestures::Common;

// This function will be called upon gesture detection - it will emit the gesture name to the console in colors
static void OnGestureDetected(EventProvider* /*sender*/, const GestureSegmentTriggeredEventArgs& args, Console::Color color)
{
	Console::SetForegroundColor(Console::Color::Red);
	Console::Write(L"Gesture detected! : ");
	Console::SetForegroundColor(color);
	Console::WriteLine(args.GetGestureSegment()->GetName());
	Console::ResetColor();
}

int main(int argc, char** argv)
{	
	Console::SetTitle(L"GesturesServiceStatus[Initializing]");
	Console::WriteLine(L"Execute one of the following gestures: Like, Drop-the-Mic, Rotate-Right ! press 'ctrl+c' to exit");

	// Step 1: Connect to Microsoft Gestures service  
	auto gesturesServiceHostName = argc == 1 ? "localhost" : argv[1]; // One can optionally pass the hostname/IP address where the gestures service is hosted
	auto gesturesService = GesturesServiceEndpoint::Make(gesturesServiceHostName);
	auto statusChangedEventScoper = gesturesService->SubscribeToStatusChangedEvent([](EventProvider*, const StatusChangedEventArgs& args)
	{
		auto endPointStatusStr = EndpointStatusToString(args.GetEndpointStatus());
		auto statusStr = std::wstring(L"GesturesServiceStatus [").append(endPointStatusStr).append(L"]");
		Console::SetTitle(statusStr);
	});
	auto future = gesturesService->ConnectAsync();
	future.wait(); // One can of course wait for the connection to be established in non blocking fashion via event loops or other async techniques
	
	// Step 2: Define a bunch of custom gestures, each detection of a gesture will print a message to the console
	/////////////////////////////
	// Rotate Right gesture
	/////////////////////////////
	// Start with defining the first pose, ...
	auto hold = HandPose::Make(L"Hold",
							   FingerPose::Make({ Finger::Thumb, Finger::Index }, FingerFlexion::Open, PoseDirection::Forward),
							   FingertipDistanceRelation::Make(Finger::Index, RelativeDistance::NotTouching, Finger::Thumb),
							   FingertipPlacementRelation::Make(Finger::Index, RelativePlacement::Above, Finger::Thumb));
	// ... define the second pose, ...
	auto rotate = HandPose::Make(L"Rotate",
		                         FingerPose::Make({ Finger::Thumb, Finger::Index }, FingerFlexion::Open, PoseDirection::Forward),
								 FingertipDistanceRelation::Make(Finger::Index, RelativeDistance::NotTouching, Finger::Thumb),
								 FingertipPlacementRelation::Make(Finger::Index, RelativePlacement::Right, Finger::Thumb));
	// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: hold -> rotate
	auto rotateGesture = Gesture::Make(L"RotateRight", hold, rotate);
	// This is the way for subscribing/registering for events notification, kindly note the 'rotateTriggerEventScoper' return value, this is 
	// RAII magic for un-subscribing from the gesture triggered event upon object destruction ! (no need for an explicit un-subscribing)
	auto rotateTriggerEventScoper = rotateGesture->SubscribeToGestureSegmentTriggeredEvent([](EventProvider* sender, const GestureSegmentTriggeredEventArgs& args) 
	{ 
		OnGestureDetected(sender, args, Console::Color::Yellow); 
	});
	// Registering the like gesture _globally_ (i.e. isGlobal parameter is set to true), by global registration we mean this gesture will be 
	// detected even it was initiated not by this application or if the this application isn't in focus
	future = gesturesService->RegisterGestureAsync(rotateGesture, true);
	future.wait();

	///////////////////////////////////
	// Drop The Mic(rophone) gesture
	///////////////////////////////////
	// Our starting pose is a full fist pointing down and/or forward
	auto fist = HandPose::Make(L"Fist", 
							   PalmPose::Make(AnyHandContext::Make(), PoseDirection::Down | PoseDirection::Forward),
							   FingerPose::Make({ Finger::Index, Finger::Middle, Finger::Ring }, FingerFlexion::Folded));	
	// Final pose is when the hand is "open" i.e. spread
	auto spread = HandPose::Make(L"Spread", 
							     PalmPose::Make(AnyHandContext::Make(), PoseDirection::Down | PoseDirection::Forward),
								 FingerPose::Make({ Finger::Thumb, Finger::Index, Finger::Middle, Finger::Ring }, FingerFlexion::Open));
	// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: fist -> spread
	auto dropTheMicGesture = Gesture::Make(L"DropTheMic", fist, spread);
	auto dropTheMicTriggerEventScoper = dropTheMicGesture->SubscribeToGestureSegmentTriggeredEvent([](EventProvider* sender, const GestureSegmentTriggeredEventArgs& args)
	{ 
		OnGestureDetected(sender, args, Console::Color::Blue); 
	});
	future = gesturesService->RegisterGestureAsync(dropTheMicGesture, true);
	future.wait();

	///////////////////////////////////
	// Like (thumb up) gesture
	///////////////////////////////////
	// Our starting pose is a fist 
	auto fist2 = HandPose::Make(L"Fist", 
								PalmPose::Make(AnyHandContext::Make(), PoseDirection::Left | PoseDirection::Right),
								FingerPose::Make(AllFingersContext::Make(), FingerFlexion::Folded));
	// In the final pose the thumb flexion will open in the "up" direction
	auto like = HandPose::Make(L"Like", 
							   PalmPose::Make(AnyHandContext::Make(), PoseDirection::Left | PoseDirection::Right),
							   FingerPose::Make({ Finger::Index, Finger::Middle, Finger::Ring, Finger::Pinky }, FingerFlexion::Folded),
							   FingerPose::Make(Finger::Thumb, FingerFlexion::Open, PoseDirection::Up));
	// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: fist2 -> Like
	auto likeGesture = Gesture::Make(L"LikeGesture", fist2, like);
	auto likeTriggerEventScoper = likeGesture->SubscribeToGestureSegmentTriggeredEvent([](EventProvider* sender, const GestureSegmentTriggeredEventArgs& args)
	{
		OnGestureDetected(sender, args, Console::Color::White);
	});
	future = gesturesService->RegisterGestureAsync(likeGesture, true);
	future.wait();

	// That's all folks !
	Console::ReadKey();	
	return 0;
}
