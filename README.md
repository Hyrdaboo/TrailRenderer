<h1 align="center">
  <br>
  <img src="https://github.com/Hyrdaboo/TrailRenderer/blob/master/TrailRendererIcon.png" width=30%>
  <br>
  Trail Renderer
  <br>
</h1>

<h4 align="center">A trail/ribbon renderer for Godot similar to Unity's <a href="https://docs.unity3d.com/Manual/class-TrailRenderer.html">TrailRenderer component</a></h4>

<img src="https://github.com/Hyrdaboo/TrailRenderer/blob/master/screenshots/scr1.png" width = 100%>
<img src="https://github.com/Hyrdaboo/TrailRenderer/blob/master/screenshots/scr2.png" width = 100%>

## Roadmap
1. [About](#about)
2. [Features](#features)
3. [Installation and Setup](#installation-and-setup)
4. [Usage](#usage)
5. [Trail Settings](#trail-settings)
6. [Known Issues](#known-issues)

## About
This is an implementation of trail/ribbon renderer similar to that in Unity. It can be used to give an emphasized feeling of motion to a moving object, or to highlight the path or position of moving objects. It also comes with a LineRenderer which is actually what's used by the TrailRenderer to draw the trail. Note that this plugin only supports 3D.
## Features
* Variable width with curve
* Variable color with gradient
* Different alignment modes
* Texture modes (tiling, stretching)

## Installation and Setup
### Installation
Clone this repo:
```
git clone https://github.com/Hyrdaboo/TrailRenderer/
```
If you don't have git you can also download zip by clicking _**Code>Download ZIP**_
### Setup
* After completing the installation, navigate to the downloaded files, and you will find the _addons_ folder.
* Drag and drop this folder into your project.
* If your project already has an _addons_ folder then drag and drop the contents of the _addons_ folder into your existing one.

You will also need this input configuration if you want to check out the demo:

![image](https://github.com/Hyrdaboo/TrailRenderer/assets/67780454/08fcc821-0e14-48b5-9bd5-9542fa365866)

## Usage
Simply create a new _Node3D_ and add a TrailRenderer script to it. You can move this object in your game either from code or parenting it to another object(whatever you wish) and it will draw a trail behind it. You can also change the parameters you set in the inspector from code.


## Trail Settings
### TrailRenderer
* **Lifetime:** Define lifetime of points of the trail in seconds
* **Min Vertex Distance:** The minimum distance between points in the trail, in world units.
* **Emitting:** When this is enabled new points are being added to the trail. Use this to pause/unpause the trail.
### LineRenderer
* **Curve:** Control the width of the trail along its length using a curve
* **Alignment:** Set the direction that the trail faces.
  * **View:** The trail faces the camera
  * **TransformZ:** The trail faces the Z axis of its GlobalBasis
  * **Static:** Every quad in the trail face the Z axis of the object's GlobalBasis when the point was emitted
* **World Space:** Sets whether the trail is emitted in world space or relative to the object its emitted from ( I recommend leaving this on)
#### Appearance
  * **Material:** Material override used by the trail
  * **Cast Shadows:** Set the shadow casting mode for the trail mesh
  * **Color Gradient:** Define a gradient to control the color of the trail along its length. (You need to enable _**Material>Vertex Color>Use As Albedo**_ for this to work)
  * **Texture Mode:** Control how the Texture is applied to the trail.
	* **Stretch:** Map the texture once along the entire length of the trail.
	* **Tile:** Repeat the texture along the trail, based on its length in world units. Use the material UV1 to change the tiling rate.
	* **Per Segment:** Repeat the texture along the trail, repeating at a rate of once per trail segment.

> [!NOTE]
> Trail renderer specific parameters are defined in the _TrailRenderer_ category and the things related to the way trail is drawn are in the _LineRenderer_ category

## Known Issues
While this plugin tries to replicate unity it's not perfect. There are a few bugs and glitches here and there and I haven't really tested its performance.
