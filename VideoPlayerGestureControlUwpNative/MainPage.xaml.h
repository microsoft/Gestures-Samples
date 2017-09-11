//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"
#include <memory>
#include "GesturesProxyNative/GesturesServiceEndpoint.hpp"
#include "GesturesNative/Poses/HandPose.hpp"

namespace VideoPlayerGestureControlUwpNative
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
		// Helper for generating hand pinch pose
		std::shared_ptr<Microsoft::Gestures::HandPose> GeneratePinchPose(const std::wstring& name, bool pinchSpread = false);

		// Holding pointer to the gestures service endpoint
		std::shared_ptr<Microsoft::Gestures::Endpoint::GesturesServiceEndpoint> _gesturesServiceEndpoint{ nullptr };
		// Holding pointer to the rewind gesture
		std::shared_ptr<Microsoft::Gestures::Gesture> _rewindGesture{ nullptr };
		// RAII magic for un-subscribing from the pause pose triggered event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _pausePoseTriggeredEventScoper{};
		// RAII magic for un-subscribing from the rewind hand motion triggered triggered event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _rewindMotionTriggeredEventScoper{};
		// RAII magic for un-subscribing from the rewind gesture triggered event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _rewindGestureTriggeredEventScoper{};
		// RAII magic for un-subscribing from the rewind gesture idle triggered event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _rewindGestureIdleTriggeredEventScoper{};
		// RAII magic for un-subscribing from the gesture service status changed event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _gestureServiceStatusChangedEventScoper{};
	};
}

