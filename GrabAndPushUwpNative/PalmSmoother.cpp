#include "pch.h"
#include <numeric>
#include "PalmSmoother.hpp"
using namespace Microsoft::Gestures::Skeleton;

void PalmSmoother::Smooth(HandSkeletonSharedPtr skeleton)
{
	auto palmPosition = skeleton->GetPalmPosition();
	auto previousAverage = GetCurrentAverage();

	if ((palmPosition - previousAverage).TwoNorm() > _jumpThreshold)
	{
		// filter jump - flush the queue and start over averaging
		_window.clear();
		_window.push_back(palmPosition);
		return;
	}

	_window.push_back(palmPosition);
	if (_window.size() > _windowSize)
	{
		_window.pop_front();
	}

	auto currentAverage = GetCurrentAverage();
	_smoothedPositionChangedDelegate(SmoothedPositionChangeEventArgs{ currentAverage, currentAverage - previousAverage });
}

void PalmSmoother::Subscribe(const SmoothedPositionChangedDelegate& handler)
{
	_smoothedPositionChangedDelegate = handler;
}
												
Vector3 PalmSmoother::GetCurrentAverage()
{
	if (_window.empty()) return Vector3{};

	return std::accumulate(_window.begin(), _window.end(), Vector3{}) * (1.0f / _window.size());
}
