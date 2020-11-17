![skybox_water_lensflare_bolt](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200909013222.png)



### Universal Render Pipeline Lens Flare System

This is a _Lens flare_ render system by render quad meshes in front of your camera. So it can be used in any render pipeline.

These system support __multiple__ lens flares at the same time, and __each flare line cost one draw call__.

![QQ截图20200909014148](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200909014244.png)

![QQ截图20200909014225](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200909014307.png)



### 0. SpriteEditor version special settings

​	In this version, flare atlas is separated by Sprite Editor, you need to change the "Texture Type" setting of atlas to "Sprite(2D and UI)", and change "Sprite Mode" setting to "Multiple",then separate the atlas to several sprite cell by yourself. 

### 1. Setup

​	Download this repository to your computer.

​	You should take care that this solution based on URP（with default settings, but it can use in build-in render pipeline by change some code）

​	Open the project you can see these Files :

![Catalog](https://github.com/Reguluz/ImageBed/blob/master/Catalog.png)



### 2. Init Asset

​	Move your cursor to _project view_, right-click and find the option: Create _URPFlareData_ to create a new flare data.

![RightClickToCreate](https://github.com/Reguluz/ImageBed/blob/master/RightClickToCreate.png)

​	Then you can see the default settings in _Inspector view_ when the asset was chosen.

![DefaultAsset](https://github.com/Reguluz/ImageBed/blob/master/DefaultAsset.png)



### 3.Asset Settings

​	There are 4 global properties to set flare asset.

​	![ChangeModel](https://github.com/Reguluz/ImageBed/blob/master/ChangeModel.png)

#### 1) Texture

​	Choose the flare atlas you need.

​	![QQ截图20200919235430](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200919235457.png)

​	__Caution: In the latest version, the flare texture must be saved in Resources folder, or it will not be loaded. I'm still trying to find a more convenient way.__

#### 2) Fade With Scale

​	Lens flare will change the sprite scale while they are fading in or fading out

#### 3) Fade With Alpha

​	Lens flare will change the sprite Alpha while they are fading in or fading out

#### 4) TextureLayout

​	There are several prepared model to slicing atlas, you are easily to find the difference between them. Choose the right one so that your atlas will not be separated incorrectly. __Other implementation methods are in research.__



### 4.Add new piece of flare

​	Lens flares based on a series of lined up sprites. So that we need to set every sprite pieces in flare assets. Click the separated atlas piece to create new flare piece. You can see following settings added in _Inspector view_.

![BlockSettings](https://github.com/Reguluz/ImageBed/blob/master/BlockSettings.png)

#### 1) Index

​	This solution search piece by setting index.you will find the index of every blocks on the board if you haven't chosen a sprite atlas. You can change this option to switch the piece you will use.

#### 2) Rotation

​	If chosen, this piece will rotate along with light source while it is moving in view.

#### 3) LightColor

​	If chosen, this piece will be influenced by the color and intensity of light source;

#### 4) Offset

​	It controls the position where this piece will finally shown on screen.

​	The value based on the distance between light source and the center of display screen.

​	value = -1, this piece will coincide with the light source.

​	value = 0, this piece will coincide with the center of display screen.

#### 5) Color

​	You can overlying a color to change the result of this piece( coexistence with light source color)

#### 6) Scale

​	This piece shown on the screen based on this property.

#### 7) Remove

​	Click to remove this piece from flare pieces list.



### 5.Add flare launcher to light source

![AddLauncher](https://github.com/Reguluz/ImageBed/blob/master/AddLauncher.png)

Add _URPFlareLauncher_ component to your game object. There are some settings based on light source, so that it requires Light component.

Now you can see launcher settings.

![LauncherSettings](https://github.com/Reguluz/ImageBed/blob/master/LauncherSettings.png)

#### 1) Directional Light

​	If the light source is directional light, set it true,  otherwise set it false. This option will determine whether the flare will disappear or not if you are far away from the light source.

#### 2) Use Light Intensity

​	the intensity of light will influence the intensity of flare on this light source if you set it true.

#### 3) Asset

​	Set the flare asset you want to use for this light source, it can't be null



### 6. Add flare render to your  camera

![AddRender](https://github.com/Reguluz/ImageBed/blob/master/AddRender.png)

Add _URPLensFlare_ component to your camera, now you can see flare render settings.

![RenderSettings](https://github.com/Reguluz/ImageBed/blob/master/RenderSettings.png)

#### 1) Debug Mode

​	There will render some line between real flare position and camera in scene view

#### 2) Material

​	Set a material to render your flare. Be sure that it is just used for flare, and in default settings, there must be a property called __BaseMap_ **(you can change the name in _URPLensFlare.cs_, line 34, change the name in brackets to match your shader, and it will show that this system can run correctly in buildin render pipeline if you set matched properties in your shader. )**.

​	Also you should known that lens flares are some transparency mesh, we need to set the render in transparent queue and in additive blend mode to show correctly. Be sure your shader in this material can make correct settings.

​	I recommend you to use _Universal Render Pipeline / Particles / Unlit_ shader.

#### 3) Fade out time

​	It shows how much time will cost to disappear the flares after light source disappeared from screen.

