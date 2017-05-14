# Release Notes
(It is always recommended to use the latest version SDK, as some of the older SDKs may not work with the hardware anymore)

SDK v2.0.1
* Release Date:
  * 12/28/2016

* New Features:
  * Supports Inside-out tracking solution with a quick setting switch.
  * Added quick switch for popular mobile HMD, such as GearVR. Daydream, Cardboard, etc. 
  * Device 3D models provided and used in the sample scene.
  * Add OS X support
  		* Known Issues:
  			* Sometimes, the Unity stops getting data from Xim devices. A Unity restart will solve this problem.
  			* BLE data may be swapped sometimes.(IE, left hand controller BLE data is received by right hand.)
  			* OS X only works on OS X platform. If switch to iOS in editor, it will throw DLL entry point errors.
  			
* Release Notes:
  * Removed un-used assets in SDK package. 
  * Removed FIFO buffer for positional tracking.
  * Added experiemental fix for time stamp sync issue. (Most notable in inside-out tracking solution).
  * Added 3D models for tracking camera and controllers.
  * Improved tutorial / user guide scene.
  * Improved Unity Input extension for Unity3D 5.5+.
  * Optimized API for getting HMD data from external VR SDK.
  * Optimized VRContext script.
  * Moved calculating mark offset to Ext folder.
  * Moved boundry API to C/C++.
  * Moved tracker rendering API to C/C++.
  * Fixed controller recenter bug.
  * Improved BLE connection algrithem. 
			
---------------------------------

SDK  v2.0
* New BLE connection solution. Please check out SDK website for details: https://ximmerse.github.io/SDK_Doc/hardwareguide/
* Added tracking area visual indicator.
* Added functionality to get controller battery info.

---------------------------------

SDK  v1.2
* Data Dispatching algorithm is greatly improved.
* Fixed occasional tracking object not smooth in previous version of SDK.

---------------------------------

SDK  v1.1
* Fixed xml config file issue.
* Included extensions for various headset systems and input systems, including HTV Vive, Oculus, Xbox controllers, etc. (Extensions are provided in a separate unity package.)
* Optimized tracking algorithm.

---------------------------------

SDK v1.0
* Official SDK that supports outside-in devices.

---------------------------------