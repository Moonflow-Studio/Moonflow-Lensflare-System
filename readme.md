### Universal Render Pipeline Lens Flare System

### 1. Setup

​	Download this repository to your computer.

​	You should take care that this solution based on URP（with default settings, but it can use in build-in render pipeline by change some code）

​	Open the project you can see these Files :

![Catalog](https://github.com/Reguluz/ImageBed/blob/master/Catalog.png)

### 2. Init Asset

​	Move your cursor to Project view, right-click and find the option: Create URPFlareData to create a new flare data.

![RightClickToCreate](https://github.com/Reguluz/ImageBed/blob/master/RightClickToCreate.png)

​	Then you can see the default settings in Inspector view when the asset was chosen.

![DefaultAsset](https://github.com/Reguluz/ImageBed/blob/master/DefaultAsset.png)

### 3.Asset Settings

​	There are 4 global properties to set flare asset.

​	![ChangeModel](https://github.com/Reguluz/ImageBed/blob/master/ChangeModel.png)

#### 1) Texture

​			Choose the flare atlas you need.

#### 2) Fade With Scale

​			Lens flare will change the sprite scale while they are fading in or 	fading out

#### 3) Fade With Alpha

​			Lens flare will change the sprite Alpha while they are fading in or 	fading out

#### 4) TextureLayout

​			There are several prepared model to slicing atlas, you are easily to find the difference between them. Choose the right one so that your atlas will not be separated incorrectly. __Other implementation methods are in research.__

### 4.Add new piece of flare

​	Lens flares based on a series of lined up sprites. So that we need to set every sprite pieces in flare assets. Click the separated atlas piece to create new flare piece. You can see following settings added in Inspector view.

![BlockSettings](https://github.com/Reguluz/ImageBed/blob/master/BlockSettings.png)

#### 1) Index

​			This solution search piece by setting index.you will find the index of every blocks on the board if you haven't chosen a sprite atlas. You can change this option to switch the piece you will use.

#### 2) Rotation

​			If chosen, this piece will rotate along with light source while it is moving in view.

#### 3) LightColor

​			If chosen, this piece will be influenced by the color and intensity of light source;

#### 4) Offset

​		    It controls the position where this piece will finally shown on screen.

​			The value based on the distance between light source and the center of display screen.

​			value = -1, this piece will coincide with the light source.

​			value = 0, this piece will coincide with the center of display screen.

#### 5) Color

​			You can overlying a color to change the result of this piece( coexistence with light source color)

#### 6) Scale

​			This piece shown on the screen based on this property.

