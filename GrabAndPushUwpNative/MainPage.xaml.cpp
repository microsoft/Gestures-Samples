//
// MainPage.xaml.cpp
// Implementation of the MainPage class.
//

#include "pch.h"
#include "MainPage.xaml.h"

using namespace std;
using namespace Microsoft::Gestures::Common;
using namespace Microsoft::Gestures::Endpoint;
using namespace Microsoft::Gestures;
using namespace Microsoft::Gestures::Skeleton;
using namespace GrabAndPushUwpNative;

using namespace Platform;
using namespace Windows::Foundation;
using namespace Windows::Foundation::Collections;
using namespace Windows::UI::Xaml;
using namespace Windows::UI::Xaml::Controls;
using namespace Windows::UI::Xaml::Controls::Primitives;
using namespace Windows::UI::Xaml::Data;
using namespace Windows::UI::Xaml::Input;
using namespace Windows::UI::Xaml::Media;
using namespace Windows::UI::Xaml::Navigation;
using namespace Windows::Media::Core;
using namespace Windows::ApplicationModel::Core;
using namespace Windows::UI::Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

MainPage::MainPage()
{	
	InitializeComponent();
	Loaded += ref new RoutedEventHandler(this, &MainPage::OnPageLoaded);
}

static inline float ComputeScaleFactor(const Vector3& smoothedPalmPositionDelta)
{	
	static auto lastScaleFactor = 1.0f;
	static const auto MOTION_RESTRAINT_FACTOR = 0.01f;
	static const auto MAX_SCALE_FACTOR        = 3.0f;
	
	// Deriving the scale factor according to the delta of the depth dimension
	lastScaleFactor = max(0.0f, min(MAX_SCALE_FACTOR, lastScaleFactor + MOTION_RESTRAINT_FACTOR * smoothedPalmPositionDelta.Z));
	return lastScaleFactor;
}

void MainPage::UpdateUi(const SmoothedPositionChangeEventArgs& args)
{
	// Scaling the border size according to the change in the Z (depth) dimension 
	const auto scaleFactor = ComputeScaleFactor(args.GetSmoothedPositionDelta());
	auto scaleTransform = ref new ScaleTransform();
	scaleTransform->CenterX = Border->Width / 2;
	scaleTransform->CenterY = Border->Height / 2;
	scaleTransform->ScaleX = scaleFactor;
	scaleTransform->ScaleY = scaleFactor;
	
	// Translation (to keep the border within the screen center)
	auto translateTransform = ref new TranslateTransform();
	translateTransform->X = (Border->ActualWidth  - Border->ActualWidth * scaleFactor)  / 2;
	translateTransform->Y = (Border->ActualHeight - Border->ActualHeight * scaleFactor) / 2;
	
	// Composition of the scaling followed by the translation
	auto transformGroup = ref new TransformGroup();
	transformGroup->Children->Append(scaleTransform);
	transformGroup->Children->Append(translateTransform);
	Border->RenderTransform = transformGroup;
}

void MainPage::OnPageLoaded(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
	/*
	* This sample demonstrates how to work with the hands skeletons stream, along with how   
	* to define a "complex" gesture to trigger the hand skeleton stream consumption
	*/
	
	// Step 1: Connect to Microsoft Gestures service
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

	// Step 2: Define your custom gesture (NOTE the modern gracious use of shared pointers in order to avoid memory leaks) 
	// Start with defining the first pose, ...
	auto spreadForwardPose = HandPose::Make(L"Spread",
											PalmPose::Make(AnyHandContext::Make(), PoseDirection::Forward),
											FingerPose::Make(AllFingersContext::Make(), FingerFlexion::Open));
	// ... define the second pose, ...
	auto fistForwardPose = HandPose::Make(L"Fist",
										  PalmPose::Make(AnyHandContext::Make(), PoseDirection::Forward),
										  FingerPose::Make(AllFingersContext::Make(), FingerFlexion::Folded));
	// Define the "release" pose which restarts, so to speak, the gesture FSM
	auto spreadReleasePose = HandPose::Make(L"SpreadRelease",
											PalmPose::Make(AnyHandContext::Make(), PoseDirection::Forward),
											FingerPose::Make(AllFingersContext::Make(), FingerFlexion::Open));
											
	// ... finally define the gesture using the hand pose objects defined above forming a simple state machine: spread -> fist -> release
	_grabGesture = Gesture::Make(L"Grab", spreadForwardPose, fistForwardPose, spreadReleasePose);

	// We'll update the UI upon each change in the smoothed position of the palm
	_palmSmoother.Subscribe([dispatcher, this](const SmoothedPositionChangeEventArgs& args)
	{		
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([args, this]()
		{
			UpdateUi(args);
		}));
	});
	
	// Once the fist segment of the gesture is detected, we "enable"/subscribe for (on the UI thread) the hand skeletons stream
	_gestureTriggeredEventScoper = fistForwardPose->SubscribeToGestureSegmentTriggeredEvent([dispatcher, this](EventProvider*, const GestureSegmentTriggeredEventArgs& args)
	{				
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([dispatcher, this]()
		{			
			if (_isHandSkeletonsStreamingEnabled) return;

			_gesturesServiceEndpoint->RegisterToSkeletonAsync([dispatcher, this](EventProvider*, const HandSkeletonsReadyEventArgs& args)
			{				
				_palmSmoother.Smooth(args.GetDefaultHandSkeleton());				
			}).wait();
			_isHandSkeletonsStreamingEnabled = true;
		}));		
	});

	// The state machine of the grab gesture returned to its initial "Idle" state, it's time to un-subscribe from the hand skeleton stream
	_gestureIdleTriggeredEventScoper = _grabGesture->SubscribeToIdleTriggeredEvent([dispatcher, this](EventProvider*, const GestureSegmentTriggeredEventArgs& args)
	{		
		if (!_isHandSkeletonsStreamingEnabled) return;

		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this]()
		{
			_gesturesServiceEndpoint->UnregisterFromSkeletonAsync().wait();					
		}));
		_isHandSkeletonsStreamingEnabled = false;
	});

	// Step 3: Register the gesture	
	future = _gesturesServiceEndpoint->RegisterGestureAsync(_grabGesture, true);
	future.wait();
}
