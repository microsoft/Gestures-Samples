The contents of this directory is:

1. [**GesturesTutorial**](GesturesTutorial) - a Unity project demonstrating how you can use [Project Prague](https://docs.microsoft.com/en-us/gestures)
to integrate gestures in Unity games. Every scene in this project corresponds to the final product of one of our [Unity tutorials](https://docs.microsoft.com/en-us/gestures). 
2. [**Microsoft.Gestures.Toolkit.unitypackage**](Microsoft.Gestures.Toolkit.unitypackage) - [Project Prague's](https://docs.microsoft.com/en-us/gestures) Unity toolkit package. 
Import this package to enable the use of gestures and hand-skeleton in your Unity project.

    List of prefabs contained in the toolkit:

    Name | Purpose |
    -----|---------|
    CameraGesturesController | Use a gesture to "grab" the main camera with your hand. When camera is grabbed, you can intuitively control its tubmle and dolly state by moving your hand.
    GesturesManager | A client for the Project Prague [Gestures Service](https://docs.microsoft.com/en-us/gestures/getting-started-gestures-service). Provides an API for gestures and hand-skeleton. A single instance of this prefab is mandatory in every gesture-enabled Unity project. | 
    GestureTrigger | Allows you to create a gesture and link it to some functionality in your scene.
    HandCursor | A sample hand-cursor based on hand-skeleton from GesturesManager. 
    SkeletonVisualizer | A sample hand-skeleton visualization based on hand-skeleton coming from GesturesManager.
    UIManager | A UI manager for the GesturesManager, displaying information about the state of the connection to the [Gestures Service](https://docs.microsoft.com/en-us/gestures/getting-started-gestures-service) and providing a basic debugging log.
