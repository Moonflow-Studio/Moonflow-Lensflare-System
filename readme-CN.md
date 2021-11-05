![skybox_water_lensflare_bolt](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200909013222.png)



### MoonFlow™ 镜头光晕系统

该进攻图光晕系统通过在相机镜头前渲染面片达成效果，因此可以适用于所有管线。

该系统支持同时多光源的镜头光晕, 每条镜头光晕需要一个draw call.

![QQ截图20200909014148](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200909014244.png)

![QQ截图20200909014225](https://raw.githubusercontent.com/Reguluz/ImageBed/master/20200909014307.png)



### 0. 不同类型图集Asset的特殊设置

##### Slice 版本

​	在该Asset版本中, 光晕图集由Sprite Editor进行切割, 你需要修改图集的 "Texture Type"的设置为 "Sprite(2D and UI)", 然后把 "Sprite Mode" 设置为 "Multiple",然后自行切割. 

##### Cell 版本

​	无需手动切割图集，但是图集以cell形式存在，在Asset中需要额外设置cell的横纵向单图数量

### 1. 设置

​	下载Package导入到你需要的工程

​	你可以看到这些文件 :

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-package.png)



### 2. 初始化资产

​	移动光标到_project_视图, 右键依次选择

​	Create  ->  MFLensflare  ->  Create MFFlareData split by SpriteEditor.

​	Create  ->  MFLensflare  ->  Create MFFlareData split by Cell.

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-assetcreator.png)

​	然后选择新建的资产你可以看到默认参数.

###### SpriteEditor版

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-slicer.png)![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-slicer-exp.png)

###### Cell 版

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-cell.png)![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-cell-exp.png)



### 3.资产设置

​	有三个资产选项是公用的.

##### 1) Texture 贴图

​	选择需要用于渲染的图集.

##### 2) Fade With Scale 缩放消散

​	光源在屏幕边界消失的时候，开启本选项光晕贴图会在设置的短时间内(参数见下方MFLensflare脚本设置)缩放到极小然后以消失（可与透明消散叠加使用）

##### 3) Fade With Alpha 透明消散

​	光源在屏幕边界消失的时候，开启本选项光晕贴图会在设置的短时间内(参数见下方MFLensflare脚本设置)渐变至完全透明然后消失（可与缩放消散叠加使用）

#### 不同种类图集切割的特有设置：

 1. SpriteEditor版本

    图集的TextureImporter设置中，Texture Type需要设置为 Sprite(2D  and UI)，Sprite Mode需要设置为 Multiple，使用

 2. Cell版本

    资产设置中多了一个Cell的选项，x为图集横向单图数量，y为纵向单图数量


### 4.资产修改 - 创建新的Flare

​	Lens flares 基于一排贴图的绘制，使用该方案时需要手动设置调节每一张flare贴图的参数。在设置好贴图切割数据后，在资产的Inspector视图内点击需要的flare贴图对该资产进行flare添加操作。你可以在inspector试图内看到如下设置.

![BlockSettings](https://github.com/Reguluz/ImageBed/blob/master/BlockSettings.png)

#### 1) Index 序号

本方案通过检索序号来查找flare贴图在整个图集中的位置。如果是Cell模式，在没有指定贴图的时候可以看到每块相对位置的贴图序号。在创建单块之后，你仍可以通过滑条调整已生成单块的贴图，以便之后的修改

#### 2) Rotation 旋转

​	如果勾选，则该贴图会一直指向屏幕中心，其显示效果会随光源相对屏幕中心位置而旋转，通常用于明显的多边形flare贴图

#### 3) LightColor 光源色

​	该参数平衡本flare贴图受光影响的程度。滑条为0时，该贴图完全使用自身color颜色，滑条为1时，该贴图颜色结果会叠加光源颜色（是否叠加光源亮度取决于在光源发射器设置，见后文）。

#### 4) Offset 偏移

​	该参数决定flare最终在屏幕上的显示位置。

​	值基于光源到屏幕中心的距离。

​	值为 -1时，本单块将和光源完全重合

​	值为0时，本单块将出现在屏幕正中心

#### 5) Color 颜色

​	你可以使用该参数在flare单图本身再叠加一层颜色进行调整

#### 6) Scale

​	该参数决定flare在屏幕上显示的大小，值为负数时，flare单图将会反向渲染

#### 7) Remove

​	点击移除该flare单图



### 5.添加发射器到需要显示镜头光的光源

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-addlauncher.png)

在有Light组件的对象上添加MFLensflareLauncher组件。该组件要求被挂载的游戏对象必须具有Light组件（需要从中获取值）

现在你可以看到发射器的选项：

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-launcher.png)

#### 1) Directional Light

​	勾选以后将该光源标记为直射光（非直射光是否显示将会受其光源物体在场景中的位置影响，直射光不会）

#### 2) Use Light Intensity

​	勾选以后该光源的Lens flare中需要受光源颜色影响的同时会叠加光源亮度

#### 3) Asset

​	设置用于该光源的配好参数的Flare资产



### 6. 添加镜头光晕渲染器到相机上

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-addmanager.png)

添加MFLensflare到挂载相机脚本的对象上，可以看到如下设置

![](https://raw.githubusercontent.com/Reguluz/ImageBed/master/mflensflare-manager1.png)

#### 1) Debug Mode - Debug模式

​	打开后在编辑器下运行该方案时将会在Scene视图绘制从相机到最终渲染面片位置的连线

#### 2) Material

​	设置用于渲染的材质。注意：请确保着色器能在当前渲染管线下正常渲染。

​	该材质上的着色器必须具有以下功能，并在MFLensflare.shader进行了演示：

 1. 深度图读取与遮挡判定

    本方案需要读取光源在屏幕坐标对应深度计算遮挡关系，因此需要：传入光源ScreenSpace坐标、读取深度图、进行相对深度计算 三个功能

 2. 顶点色叠加

    本方案的颜色叠加基于手动设置渲染mesh的顶点色，因此着色器最终渲染结果必须叠加顶点色才会起效

 3. Transparent队列与Additive混合

    光晕最终以半透明面片渲染在屏幕前，因此shader需要设置混合模式为"Blend One One"， ，并设置渲染队列大于3000以在半透明批次进行渲染

 4. _MainTex

    在MFLensflare.cs中，指定图集将通过MaterialPropertyBlock赋值到材质球的"\_MainTex"参数，因此需要保证着色器读取了\_MainTex。你也可以同时修改和统一脚本和着色器的命名以保证名字修改后功能仍能正常运行

#### 3) Fade out time

​	该项决定光源在离开屏幕边缘时，光晕消失的时间，0为立即消失



### 其他可选优化项/问题

1. 基于Cinemachine的镜头移动导致光源计算位置未能及时同步

   请在ProjectSettings - Script Execution Order中添加MFLensflare脚本，并将队列设置在CinemachineBrain之后（即右侧数值大于CinemachineBrain的数值）可以解决问题。参照本文 [Execution order problem with assets (rainyrizzle.github.io)](https://rainyrizzle.github.io/en/AdvancedManual/AD_ExecutionOrderProblem.html)

2. 多光源Drawcall冗余

   当前方案为最大程度兼容设置，允许多光源使用多图集的方案，即每个光源生成的镜头光晕序列可以使用不同图集。理论上如果所有光源的镜头光晕使用同一图集，则数个光源的光晕可以一次性渲染只消耗一个draw call。方案不做跟进处理，可以使用者自行尝试修改代码

3. 待补充

