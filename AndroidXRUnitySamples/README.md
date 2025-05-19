# Android XR Samples for Unity

These simple, yet playful, samples demonstrate Android XR platform features and
show how to implement the experiences. This project uses both the
[Android XR: OpenXR package](https://docs.unity3d.com/Packages/com.unity.xr.androidxr-openxr@1.0/manual/index.html) and [Android XR extensions for Unity](https://github.com/android/android-xr-unity-package).

## Prerequisites

1. Download [Unity 6000.1.0b12](https://unity.com/releases/editor/beta/6000.1.0b12)
2. Select “Android Build Support” module
3. Install OpenJDK
4. Install Android SDK & NDK tools

## Getting Started

1. Open the project in Unity
2. Open the Build Profiles menu and switch to the Android platform
3. Open the Window menu and click on TextMeshPro -> Import TMP Essential Resources
4. Build and run the project

The app will show up as Android XR Unity Samples on your device.

For more information see: https://developer.android.com/develop/xr/unity

## Showcase Samples

Explore the fun and interesting possibilities of Android XR through interactive
showcase samples. These samples are integrated into a single Unity project,
to make it convenient to discover, engage with, and learn how to implement these
experiences.

### Tabletop Mess

This sample uses plane tracking to visualize planes around the user
which allows them to drop visual items onto them.

### How to use

The device detects and labels nearby planes. Use hand rays to aim at object
icons in the UI and pinch to spawn interactive 3D objects on the virtual planes.
Manipulate objects by picking them up and move them with hand rays.
Tapping the UI button will toggle passthrough cutouts for the planes.

### Screenwiper

Aim and pinch to scrub away your virtual environment to reveal the real world
behind it. This showcase highlights one way to blend virtual worlds with the
real world by allowing the user to modify a virtual mask.

### How to use

Each hand controls a cursor. Pinch to activate a hand’s respective cursor to scrub
away the virtual environment at that position. Stop pinching with both hands to
reset the environment.

### Device Detector

Create interactable balloons from your bluetooth connected keyboard. The object
tracking feature identifies a real keyboard on your desk. When identified,
key presses generate playful balloons.

### How to use

Connect a bluetooth keyboard to your headset, each typed key will appear
above the keyboard.

### Gaze and Pinch

Use Eye Tracking and Pinch gestures to fire projectiles at virtual structures.

### How to use

The experience consists of two phases: Setup and Playing

During the setup phase, the system uses plane tracking to detect suitable planes
in the user's environment. Looking around will highlight detected planes and
indicate their suitability.

The user initiates the playing phase by selecting a plane using a pinch gesture
with either hand.

In the playing phase, a virtual structure with three projectile launchers
appears. Looking at a launcher activates it, and a pinch gesture launches
a projectile at the structure.

Looking at the virtual structure also displays a user interface on top of it.
This UI can be interacted with using eye tracking and pinch to reset the
structure, display a different structure, or return to the setup phase.

### Creature Gaze

Friendly creatures that mimic your eye position by using the eye tracking API.

### Talking Objects

Using the Face Tracking API control various objects with your face! Press the
record button to capture a short clip for endless playback or puppet the objects
in real time. This showcase highlights face tracking for humanoid and
non-humanoid objects.

### How to use

Face tracking will automatically begin when the scene starts.

* To switch between the Picture Frame Face, Balloon, and Couch, use the arrow
buttons located on the sides of the object.
* Tap the Record button to create a brief video clip.
* Once recording is complete, playback will start automatically.
* To halt any playback, press the Stop button.
* Press the Play button to initiate any recorded playback.

### Surface Canvas

Interact with various UI controls in XR through all the supported interaction
modes. Switch between the supported input modes: hand tracking only,
hand + eye tracking, eye tracking only, head + hand tracking, head tracking
only. Toggle passthrough on/off and set the blend level for passthrough.
Enable/disable an aiming reticle during eye and head tracking modes.

### How to use

Look around and locate the virtual table with the object on top of it and UI
panels floating above it.

**UI Panels:**

* Left & Middle: Control object parameters.
* Right: Switch input modes, toggle passthrough, and adjust passthrough blend.

**Interaction Modes:**

* Eye & Hands: Gaze to select, pinch to activate.
* Hands Only: Point hand raycast, pinch to activate.
* Eye Tracking Only: Gaze to select, dwell for 1 second to activate.

### Gesture Run

Uses the Hand Gestures API in order to control a running ball. Various gestures
are used as the input actions on the ball. Can collect coins or
destroy structures along the path.

### How to use

Each hand manages the ball on its corresponding lane.

* Pinch: To move a ball to the other lane.
* Open Palm Gesture: To jump.

### Sound Arena

The user is placed in a room with multiple sound sources and can play through a
scripted experience. Interact with the sound directivity by locating
audio sources in 3D space while everything is dark around you.

### How to use

Ensure the sound volume is audible (not muted); headphones are recommended for
optimal experience.

* Load the Sound Arena sample.
* Locate the experience guide, identified by floating tutorial text.
* Await the introduction, which begins shortly after the lights dim and a flashlight appears near the guide.
* Pinch with either hand to bring the flashlight to it; a single pinch with a specific hand attaches the flashlight to that hand.
* Using the auditory cues of the moving critter, direct the flashlight at it and maintain focus until it disappears. Repeat this process three more times.
* Once the lights return, you can continue interacting with the sample or exit.
