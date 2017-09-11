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
using namespace RotateSampleUwpNative;

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

void MainPage::OnPageLoaded(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e)
{
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

	// Step 2: Define your custom gesture (NOTE the modern gratuitous use of shared pointers in order to avoid memory leaks) 
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
	_rotateGesture = Gesture::Make(L"RotateRight", hold, rotate);
				
	_gestureTriggeredEventScoper = _rotateGesture->SubscribeToGestureSegmentTriggeredEvent([dispatcher, this](EventProvider* sender, const GestureSegmentTriggeredEventArgs& args) 
	{ 
		// This event handler simply rotates the arrow control from within the UI thread		
		dispatcher->RunAsync(CoreDispatcherPriority::Normal, ref new DispatchedHandler([this]()			
		{
			auto rotateTransform = ref new RotateTransform();
			rotateTransform->Angle = ++_rotateTimes * 90;
			rotateTransform->CenterX = Arrow->ActualWidth / 2;
			rotateTransform->CenterY = Arrow->ActualHeight / 2;
			Arrow->RenderTransform = rotateTransform;
		}));		
	});
	
	// Step 3: Register the gesture (When window focus is lost/gained the service will effectively change the gesture registration automatically)
	//         To manually control the gesture registration, pass 'isGlobal: true' parameter in the function call below
	future = _gesturesServiceEndpoint->RegisterGestureAsync(_rotateGesture);
	future.wait();
}