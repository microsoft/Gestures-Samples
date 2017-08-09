# Overview

This repository contains C# code samples demonstrating how to use the [Project Prague](https://aka.ms/gestures) hand gestures SDK. For a quick introduction to programming with our SDK, please visit our [overview page on docs.microsoft.com](https://docs.microsoft.com/en-us/gestures/). In order to build and run the code samples provided in this repository, you must first [set up Project Prague on your machine](https://docs.microsoft.com/en-us/gestures/index#getting-started-with-project-prague). Note that you will need either an [**Intel® RealSense™ SR300 camera**](https://click.intel.com/intelrealsense-developer-kit-featuring-sr300.html) or a [**Kinect for Windows v2 camera**](https://developer.microsoft.com/en-us/windows/kinect/hardware) to enable our gesture detection runtime.

# Code Samples

After cloning this repository, please open the [Microsoft.Gestures.Samples](Microsoft.Gestures.Samples.sln) soulution in Visual Studio. You will find 6 projects in this solution. Each project is a stand-alone code sample that can be built and run independently of the other projects. The table below lists the samples ordered from the least to the most advanced:

Sample|Use case|What will you learn?
------|-----------|--------------------
[RotateSample](RotateSample) | Rotate an image by 90° | Create a simple [gesture](https://docs.microsoft.com/en-us/gestures/index#gesture) made up of two [hand poses](https://docs.microsoft.com/en-us/gestures/index#hand-pose)
[VideoPlayerGestureControl](VideoPlayerGestureControl) | Pause and rewind audio\video playback | Create a simple [gesture](https://docs.microsoft.com/en-us/gestures/index#gesture) made up of multiple [hand poses](https://docs.microsoft.com/en-us/gestures/index#hand-pose) and a [hand motion](https://docs.microsoft.com/en-us/gestures/index#hand-motion)
[RockPaperScissors](RockPaperScissors) | Play Rock-Paper-Scissors against an invincible bot | Create a [gesture](https://docs.microsoft.com/en-us/gestures/index#gesture) made up of multiple [hand poses](https://docs.microsoft.com/en-us/gestures/index#hand-pose) connected in a complex structure
[Camera3D](Camera3D) | Manipulate camera in a 3D scene | Create a simple [gesture](https://docs.microsoft.com/en-us/gestures/index#gesture) and consume hand skeleton stream
[GesturesPowerPointPlugin](GesturesPowerPointPlugin) (requires [Office Tools for Visual Studio](https://www.visualstudio.com/vs/office-tools/))| Create and present a slideshow in PowerPoint using gestures | Create multiple simple [gestures](https://docs.microsoft.com/en-us/gestures/index#gesture) and wire them to activate various PowerPoint features
[CarGestures](CarGestures) | Control a virtual car's infotainment-system with gestures | Create a complete WPF UI with multiple [gestures](https://docs.microsoft.com/en-us/gestures/index#gesture) of varying complexity
[Unity Tutorial Project](Unity) | Follow our [Unity tutorial](https://docs.microsoft.com/en-us/gestures/unity-tutorials-introduction) | Integrate gestures and hand-skeleton input in your Unity games

# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
