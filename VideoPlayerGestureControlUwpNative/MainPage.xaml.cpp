//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"
#include "GesturesNative/Motions/HandMotion.hpp"

using namespace std;
using namespace Microsoft::Gestures::Common;
using namespace Microsoft::Gestures::Endpoint;
using namespace Microsoft::Gestures;
using namespace VideoPlayerGestureControlUwpNative;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::UI::Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{
	InitializeComponent();
	Loaded += ref new RoutedEventHandler(this, &MainPage::OnPageLoaded);
}

void MainPage::OnPageLoaded(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	/////////////////////////////////////////////////////////////////////////////////
	/// This sample demonstrates how to incorporate hand motions into gestures !   //	
	/////////////////////////////////////////////////////////////////////////////////

	// Step 1: Connect to Microsoft Gestures Detection service
	_gesturesServiceEndpoint = GesturesServiceEndpoint::Make();
	auto dispatcher = CoreApplication::MainView->CoreWindow->Dispatcher;
	_gestureServiceStatusChangedEventScoper = _gesturesServiceEndpoint->SubscribeToStatusChangedEvent([dispatcher, this](EventProvider*, const StatusChangedEventArgs& args)
	{
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([args, this]()
		{
			auto endPointStatusStr = EndpointStatusToString(args.GetEndpointStatus());
			auto statusStr = std::wstring(L"[").append(endPointStatusStr).append(L"]");
			GesturesServiceStatus->Text = ref new String(statusStr.c_str());
		}));
	});
	auto future = _gesturesServiceEndpoint->ConnectAsync();
	future.wait(); // One can of course wait for the connection to be established in non blocking fashion via event loops or other async techniques

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
	auto spreadPose = GeneratePinchPose(L"Spread", true);
	auto pausePose = GeneratePinchPose(L"Pause");
	_pausePoseTriggeredEventScoper = pausePose->SubscribeToGestureSegmentTriggeredEvent([dispatcher, this](EventProvider*, const GestureSegmentTriggeredEventArgs& args)
	{
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([args, this]() 
		{
			VideoStatus->Text = ref new String(L"⏸"); 
		}));
	});
	
	auto rewindMotion = HandMotion::Make(L"Back", PalmMotion::Make(VerticalMotionSegment::GetLeft()));
	_rewindMotionTriggeredEventScoper = rewindMotion->SubscribeToGestureSegmentTriggeredEvent([dispatcher, this](EventProvider*, const GestureSegmentTriggeredEventArgs& args)
	{
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([args, this]()
		{
			VideoStatus->Text = ref new String(L"⏪");
		}));
	});
		
	auto keepRewindingPose = GeneratePinchPose(L"KeepRewind");
	auto releasePose = GeneratePinchPose(L"Release", true);

	// Then define the gesture by concatenating the previous objects to form a simple state machine
	_rewindGesture = Gesture::Make(L"RewindGesture", spreadPose, pausePose, rewindMotion, keepRewindingPose, releasePose);
	// Detect if the user releases his pinch-grab and return to playback
	_rewindGesture->AddSubPath(pausePose, releasePose);

	// Continue playing the video when the gesture resets (either successful or aborted)
	_rewindGestureTriggeredEventScoper = _rewindGesture->SubscribeToGestureSegmentTriggeredEvent([dispatcher, this](EventProvider*, const GestureSegmentTriggeredEventArgs& args)
	{
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([args, this]()
		{
			VideoStatus->Text = ref new String(L"▶");
		}));
	});	
	_rewindGestureIdleTriggeredEventScoper = _rewindGesture->SubscribeToIdleTriggeredEvent([dispatcher, this](EventProvider*, const GestureSegmentTriggeredEventArgs& args)
	{
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([args, this]()
		{
			VideoStatus->Text = ref new String(L"▶");
		}));
	});
		
	// Step 3: Register the gesture (When window focus is lost/gained the service will effectively change the gesture registration automatically)
	//         To manually control the gesture registration, pass 'isGlobal: true' parameter in the function call below
	future = _gesturesServiceEndpoint->RegisterGestureAsync(_rewindGesture);
	future.wait();
}

std::shared_ptr<HandPose> MainPage::GeneratePinchPose(const std::wstring& name, bool pinchSpread)
{
	std::vector<Finger> pinchingFingers{ Finger::Thumb, Finger::Index };
	auto openFingersContext = pinchSpread ? (std::shared_ptr<FingersContext>)AllFingersContext::Make(pinchingFingers) : AnyFingerContext::Make(pinchingFingers);
	
	return HandPose::Make(name, 
						  FingerPose::Make(openFingersContext, FingerFlexion::Open),
						  FingertipDistanceRelation::Make(pinchingFingers, pinchSpread ? RelativeDistance::NotTouching : RelativeDistance::Touching),
						  FingertipDistanceRelation::Make(pinchingFingers, RelativeDistance::NotTouching, Finger::Middle));
}
