UIEffect Snapshot
===

Capture a screenshot with effect and display it.  
Light-weight, non-realtime, no-camera (and no-PostProcessingStack), but be effective enough.  
The captured snapshot can be used as a background for a UI panel.

[![](https://img.shields.io/npm/v/com.coffee.ui-effect-snapshot?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.ui-effect-snapshot/)
[![](https://img.shields.io/github/v/release/mob-sakai/UIEffectSnapshot?include_prereleases)](https://github.com/mob-sakai/UIEffectSnapshot/releases)
[![](https://img.shields.io/github/release-date-pre/mob-sakai/UIEffectSnapshot)](https://github.com/mob-sakai/UIEffectSnapshot/releases)
![](https://img.shields.io/badge/unity-2018.3%20or%20later-green.svg)
[![](https://img.shields.io/github/license/mob-sakai/UIEffectSnapshot.svg)](https://github.com/mob-sakai/UIEffectSnapshot/blob/upm/LICENSE.txt)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)


<< [Description](#description) | [Demo](#demo) | [Installation](#installation) | [Usage](#usage) | [Change log](https://github.com/mob-sakai/UIEffectSnapshot/blob/upm/CHANGELOG.md) >>



<br><br><br><br>

## Description

Do you like blurred backgrounds, like iOS home screen?
Blurring effect is easy to implement in Unity.
- Grabs the current screen contents into a texture with [GrabPass](https://docs.unity3d.com/Manual/SL-GrabPass.html).
- Sample `_GrabTexture` with [blur-algorithms].
- Implementation example: https://pastebin.com/z3w8jhs4

Alternatively, you can use [PostProcessingStack] package.

[blur-algorithms]: https://software.intel.com/content/www/us/en/develop/blogs/an-investigation-of-fast-real-time-gpu-based-image-blur-algorithms.html

[PostProcessingStack]: https://docs.unity3d.com/Packages/com.unity.postprocessing@2.3/manual/index.html

However, this method requires some caution to be used as a UI.

- You'll need a camera and canvas for the blur effect
- `Screen Space - Overlay` is not supported.
- (Especially in mobile,) GrabPass is expensive

<br>

This package uses **static screen content of one frame** instead of real-time screen content to provide a very light screen blur.  

The movement will disappear from the background, but it will be enough to use it as a background for panels, dialog windows and menus.  

Objects that are further back than the background are (consequently) invisible.  
Disabling their animations or hiding them would improve performance and power consumption.

![](https://user-images.githubusercontent.com/12690315/94590637-f35f8800-02c1-11eb-969a-13574753f17e.gif)

### Features

* Easy to use: the package is out-of-the-box!
* Light weight design:
  * Non realtime capturing. 
  * Support for mobile.
  * No extra camera and `PostProcessingStack` are needed.
* Support render mode: `Screen Space - Overlay` and `Screen Space - Camera`
* Effect Mode: Grayscale, Sepia, Nega, Pixelation
* Color Mode: Multiply, Fill, Additive, Subtract
* Blur Mode: Fast Blur, Medium Blur, Detail Blur
* Global Mode: Capture a screenshot in cases where there is no UIEffectSnapshot instance.
* Capture On Enable: When the component is enable, capture screen automatically.
* Fit to screen: Fit the RectTransform to the screen.

### Future Plans

* Support render mode: `World Space - Camera`
* Custom effect material
* Support pre-generated RenderTexture as result
* Support [Universal Render Pipeline](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@10.0/manual/index.html)

<br><br><br><br>

## Demo

* [WebGL Demo](https://mob-sakai.github.io/Demos/UIEffectSnapshot)

<br><br><br><br>

## Installation

### Requirement

* Unity 2018.3 or later

### Using OpenUPM

This package is available on [OpenUPM](https://openupm.com).  
You can install it via [openupm-cli](https://github.com/openupm/openupm-cli).
```
openupm add com.coffee.ui-effect-snapshot
```

### Using Git

Find the manifest.json file in the Packages folder of your project and edit it to look like this:
```
{
  "dependencies": {
    "com.coffee.ui-effect-snapshot": "https://github.com/mob-sakai/UIEffectSnapshot.git",
    ...
  },
}
```

To update the package, change suffix `#{version}` to the target version.

* e.g. `"com.coffee.ui-effect-snapshot": "https://github.com/mob-sakai/UIEffectSnapshot.git#1.0.0",`

Or, use [UpmGitExtension](https://github.com/mob-sakai/UpmGitExtension) to install and update the package.



<br><br><br><br>

## Usage

### UIEffectSnapshot

- This is a component to capture a screenshot with effects and display it.
- Select `Game Object/UI/Effect Snapshot` to create.
- Adjust the capture effect and press the `Capture` or `Release` button in the inspector to preview the snapshot.

| Properties | Screenshot |
| -- | -- |
| **Effect Mode:** Grayscale, Sepia, Nega, Pixelation <br>**Color Mode:** Multiply, Fill, Additive, Subtract <br>**Blur Mode:** Fast, Medium, Detail <br><br>**Global Mode:** Use a global image instead of an instance image. <br>**Capture On Enable:** When the component is enable, capture screen automatically.<br>**Fit To Screen:** Fit transform to the root canvas on enable/captured. | ![][snapshot] |

[snapshot]:https://user-images.githubusercontent.com/12690315/94591029-784aa180-02c2-11eb-81e2-47683f4bb44f.png

#### Script usage

```cs
// Add UIEffectSnapshot instance at runtime.
uiEffectSnapshot = gameObject.AddComponent<UIEffectSnapshot>();

// Capture a screenshot for instance.
uiEffectSnapshot.Capture(callback: request => { Debug.Log("Captured"); });

// Capture a screenshot for global.
// You can capture screen in cases where there is no UIEffectSnapshot instance.
// The captured image will be used with 'Global Mode' instance.
// Or, you can use 'UIEffectSnapshot.globalCapturedTexture' property to get it.
UIEffectSnapshot.CaptureForGlobal(callback: request => { Debug.Log("Captured"); });
```
<br><br>

### UIEffectSnapshotPanel

- This is a component for easy control of a panel with snapshot background/panel.
- Select `Game Object/UI/UI Effect Snapshot Panel/***` to create a panel with snapshot.

| Properties | Screenshot |
| -- | -- |
| **Snapshots:** UIEffectSnapshot instances to control. <br>**Transition Duration:** Duration of show/hide transition. <br>**Show On Enable:** When the component is enable, show the panel automatically. | ![][panel] |

[panel]:https://user-images.githubusercontent.com/12690315/94591035-7a146500-02c2-11eb-9daa-c4b81311ae32.png

#### Script usage

```cs
// Show/Hide the panel with snapshot.
// Before capturing, CanvasGroup.alpha will be set to 0 and capture the screen except for the panel.
panel.Show(callback: request => { Debug.Log("Shown"); });
panel.Hide(callback: request => { Debug.Log("Hidden"); });
```

<br><br><br><br>

## How to play demo

### For Unity 2019.1 or later

1. Open `Package Manager` window
2. Select `UI Effect Snapshot` package in package list
3. Click `Import Sample` button
4. The demo project is imported into `Assets/Samples/UI Effect Snapshot/{version}/Demo`
5. Open `UIEffectSnapshot_Demo` scene and play it

### For Unity 2018.4 or earlier

1. Select `Assets/Samples/UI Effect Snapshot Demo` from menu
2. The demo project is imported into `Assets/Samples/UI Effect Snapshot/{version}/Demo`
3. Open `UIEffectSnapshot_Demo` scene and play it

<br><br><br><br>

## Contributing

### Issues

Issues are very valuable to this project.

- Ideas are a valuable source of contributions others can make
- Problems show where this project is lacking
- With a question you show where contributors can improve the user experience

### Pull Requests

Pull requests are, a great way to get your ideas into this repository.  
See [CONTRIBUTING.md](/../../blob/develop/CONTRIBUTING.md).

### Support

This is an open source project that I am developing in my spare time.  
If you like it, please support me.  
With your support, I can spend more time on development. :)

[![](https://user-images.githubusercontent.com/12690315/50731629-3b18b480-11ad-11e9-8fad-4b13f27969c1.png)](https://www.patreon.com/join/mob_sakai?)  
[![](https://user-images.githubusercontent.com/12690315/66942881-03686280-f085-11e9-9586-fc0b6011029f.png)](https://github.com/users/mob-sakai/sponsorship)



<br><br><br><br>

## License

* MIT



## Author

[mob-sakai](https://github.com/mob-sakai)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)



## See Also

* GitHub page : https://github.com/mob-sakai/UIEffectSnapshot
* Releases : https://github.com/mob-sakai/UIEffectSnapshot/releases
* Issue tracker : https://github.com/mob-sakai/UIEffectSnapshot/issues
* Change log : https://github.com/mob-sakai/UIEffectSnapshot/blob/upm/CHANGELOG.md
