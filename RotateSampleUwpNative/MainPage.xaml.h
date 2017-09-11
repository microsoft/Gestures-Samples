//
// MainPage.xaml.h
// Declaration of the MainPage class.
//

#pragma once

#include "MainPage.g.h"
#include <memory>
#include "GesturesProxyNative/GesturesServiceEndpoint.hpp"
#include "GesturesNative/Gesture.hpp"

namespace RotateSampleUwpNative
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

		// Holding pointer to the gestures service endpoint
		std::shared_ptr<Microsoft::Gestures::Endpoint::GesturesServiceEndpoint> _gesturesServiceEndpoint{ nullptr };
		// Holding pointer to the rotate gesture
		std::shared_ptr<Microsoft::Gestures::Gesture> _rotateGesture{ nullptr };
		// Keeping how many times the rotate gesture was detected/triggered
		int _rotateTimes{ 0 };
		// RAII magic for un-subscribing from the gesture triggered event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _gestureTriggeredEventScoper{};
		// RAII magic for un-subscribing from the gesture service status changed event upon object destruction
		Microsoft::Gestures::Common::EventScopedConnection _gestureServiceStatusChangedEventScoper{};
	};
}
