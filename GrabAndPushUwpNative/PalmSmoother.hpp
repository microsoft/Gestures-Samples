#pragma once
#include <deque>
#include <functional>
#include "GesturesNative/Skeleton/IHandSkeleton.hpp"

class SmoothedPositionChangeEventArgs
{
public:
	SmoothedPositionChangeEventArgs(const Microsoft::Gestures::Skeleton::Vector3& smoothedPosition, const Microsoft::Gestures::Skeleton::Vector3& smoothedPositionDelta)
		: _smoothedPosition(smoothedPosition), _smoothedPositionDelta(smoothedPositionDelta) {}

	/// <summary> Position of palm after jitter was smoothed. </summary>
	Microsoft::Gestures::Skeleton::Vector3 GetSmoothedPosition() const { return _smoothedPosition; }		
	/// <summary> Difference between current smoothed position and previous one. </summary>
	Microsoft::Gestures::Skeleton::Vector3 GetSmoothedPositionDelta() const { return _smoothedPositionDelta; }

private:
	Microsoft::Gestures::Skeleton::Vector3 _smoothedPosition;
	Microsoft::Gestures::Skeleton::Vector3 _smoothedPositionDelta;
};

using SmoothedPositionChangedDelegate = std::function<void(const SmoothedPositionChangeEventArgs&)>;

/// <summary> This class is a simple implementation moving average which smooths the palm position along time </summary>
class PalmSmoother final
{
public:
	PalmSmoother(size_t windowSize = DEFAULT_WINDOW_SIZE, float jumpThreshold = DEFAULT_JUMP_THRESHOLD)
		: _windowSize(windowSize), _jumpThreshold(jumpThreshold) {}
	
	void Smooth(Microsoft::Gestures::Skeleton::HandSkeletonSharedPtr skeleton);
	
	void Subscribe(const SmoothedPositionChangedDelegate& handler);

private:
	// Constants
	static constexpr size_t DEFAULT_WINDOW_SIZE = 5; // [samples]
	static constexpr auto   DEFAULT_JUMP_THRESHOLD = 50.0f;  //[mm]

	// Methods
	Microsoft::Gestures::Skeleton::Vector3 GetCurrentAverage();

	// Fields
	const size_t _windowSize;
	const float _jumpThreshold;
	std::deque<Microsoft::Gestures::Skeleton::Vector3> _window;
	SmoothedPositionChangedDelegate _smoothedPositionChangedDelegate;
};
