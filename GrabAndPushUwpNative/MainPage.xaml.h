//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"
#include "GesturesProxyNative/GesturesServiceEndpoint.hpp"
#include "GesturesNative/Gesture.hpp"
#include "GesturesNative/IGesturesRuntime.hpp"
#include "PalmSmoother.hpp"

namespace GrabAndPushUwpNative
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public ref class MainPage sealed
	{
	public:
		MainPage();

	private:
		// Our PageLoaded event callback
		void OnPageLoaded(Platform::Object^ sender, Windows::UI::Xaml::RoutedEventArgs^ e);
		// UI updating (carries out the caption scaling)
		void UpdateUi(const SmoothedPositionChangeEventArgs& args);
		
		// Holding pointer to the gestures service endpoint
		std::shared_ptr<Microsoft::Gestures::Endpoint::GesturesServiceEndpoint> _gesturesServiceEndpoint{ nullptr };
		// Holding pointer to the grab gesture
		std::shared_ptr<Microsoft::Gestures::Gesture> _grabGesture{ nullptr };
		// RAII magic for un-subscribing from the gesture triggered event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _gestureTriggeredEventScoper{};
		// RAII magic for un-subscribing from the gesture idle event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _gestureIdleTriggeredEventScoper{};
		// RAII magic for un-subscribing from the gesture service status changed event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _gestureServiceStatusChangedEventScoper{};
		// This guy smooths the inherent jitter of the palm position portion of the hand skeleton
		PalmSmoother _palmSmoother;
		// Indication if the hand skeletons streaming is in progress
		bool _isHandSkeletonsStreamingEnabled{ false };		
	};
}
