The contents of this directory is:

- [**Tutorials**](Tutorials) - contains several Unity projects demonstrating how to use [Project Prague](https://docs.microsoft.com/en-us/gestures)
to integrate gestures in Unity games. Each Unity project within this directory contains the final product of one of our Unity tutorials:

    - [Introduction](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-introduction) - learn how to wire a gesture to an existing functionality in your scene. This tutorial introduces you to the **GesturesManager**, **UIManager**, **GestureTrigger** and **CameraGesturesController** prefabs.
    - [3D Object Manipulation (Mouse)](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-3d-object-manipulation-mous) - create a 3D cursor that can move objects in the scene. The cursor is controlled by the mouse, using existing Unity capabilities.
    - [3D Object Manipulation (Hand)](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-3d-object-manipulation-hand) - create a 3D cursor which is controlled by the hand, enabling you to use a gesture to "grab" an object and move it around in the scene.

- [**Microsoft.Gestures.Toolkit.unitypackage**](Microsoft.Gestures.Toolkit.unitypackage) - [Project Prague's](https://docs.microsoft.com/en-us/gestures) Unity toolkit package.
Import this package to enable the use of gestures and hand-skeleton in your Unity project.

    List of prefabs contained in the toolkit, in alphabetical order:

    Name | Purpose | Usage Example
    -----|---------|--------------
    **CameraGesturesController** | Use a gesture to "grab" the main camera of the scene, allowing you to tumble and dolly the camera by moving your hand. | [Introduction tutorial - step 6](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-introduction#step-6---using-a-gesture-to-control-the-camera)
    **GesturesManager** | A client for the Project Prague [Gestures Service](https://docs.microsoft.com/en-us/gestures/getting-started-gestures-service). Provides a scripting API for gestures and hand-skeleton. A single instance of this prefab is mandatory in every gesture-enabled Unity project. | [Introduction tutorial - step 2](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-introduction#step-2---connecting-to-the-gestures-service)
    **GestureTrigger** | Allows you to specify a gesture and link it to some functionality in your scene. | [Introduction tutorial - step 4](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-introduction#step-4---using-a-gesture-to-generate-new-3d-primitives-in-the-scene)
    **HandCursor** | A 2D cursor, based on the projection of your palm center to the screen.
    **SkeletonVisualizer** | A visualization illustrating the hand-skeleton in real-time.
    **UIManager** | A UI manager for the GesturesManager, displaying information about the state of the connection to the [Gestures Service](https://docs.microsoft.com/en-us/gestures/getting-started-gestures-service) and providing a basic debugging log. | [Introduction tutorial - step 2](https://review.docs.microsoft.com/en-us/gestures/unity-tutorials-introduction#step-2---connecting-to-the-gestures-service)
