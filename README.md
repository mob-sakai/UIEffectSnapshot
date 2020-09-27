UIEffect Snapshot
===

Capture a snapshot with effect and display it.  
The effect is non-realtime, light-weight, less-camera, but be effective enough.  
The captured snapshot can be used as a background for a dialog or window.

[![](https://img.shields.io/npm/v/com.coffee.ui-effect-snapshot?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.coffee.ui-effect-snapshot/)
[![](https://img.shields.io/github/v/release/mob-sakai/UIEffectSnapshot?include_prereleases)](https://github.com/mob-sakai/UIEffectSnapshot/releases)
[![](https://img.shields.io/github/release-date/mob-sakai/UIEffectSnapshot.svg)](https://github.com/mob-sakai/UIEffectSnapshot/releases)
![](https://img.shields.io/badge/unity-2018.2%20or%20later-green.svg)
[![](https://img.shields.io/github/license/mob-sakai/UIEffectSnapshot.svg)](https://github.com/mob-sakai/UIEffectSnapshot/blob/upm/LICENSE.txt)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-orange.svg)](http://makeapullrequest.com)
[![](https://img.shields.io/twitter/follow/mob_sakai.svg?label=Follow&style=social)](https://twitter.com/intent/follow?screen_name=mob_sakai)


<< [Description](#description) | [Demo](#demo) | [Installation](#installation) | [Usage](#usage) | [Change log](https://github.com/mob-sakai/UIEffectSnapshot/blob/upm/CHANGELOG.md) >>



<br><br><br><br>

## Description

Capture a screenshot with effect and display it.  
The effect is non-realtime, light-weight, less-camera, but be effective enough.  
The captured snapshot can be used as a background for a dialog or window.

![](https://user-images.githubusercontent.com/12690315/94373370-0e09f380-0140-11eb-83f9-ab73b22f8b14.gif)

### Features


* Effect Mode: Grayscale, Sepia, Nega, Pixelation
* Color Mode: Multiply, Fill, Additive, Subtract
* Blur Mode: Fast Blur, Medium Blur, Detail Blur
* Global Mode: 
* Capture On Enable: When the component is enable, capture screen automatically.
* Fit to screen: Fit the RectTransform to the screen.
* Quality Mode: Fast, Medium, Detail, Custom
  * Down-sampling Rate: None, x1, x2, x4, x8
  * Reduction Rate: None, x1, x2, x4, x8
  * Blur Iterations: 1 - 8 times
  * Filter Mode: Point, Bilinear, Trilinear


<br><br><br><br>

## Demo

* [WebGL Demo](https://mob-sakai.github.io/Demos/UIEffectSnapshot)

![](https://user-images.githubusercontent.com/12690315/94373370-0e09f380-0140-11eb-83f9-ab73b22f8b14.gif)

<br><br><br><br>

## Installation

### Requirement

* Unity 2018.2 or later

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

![](https://user-images.githubusercontent.com/12690315/94373459-b0c27200-0140-11eb-9d88-237873539a16.png)

### Capture from script

### Example: A window with blurred background

### Example: Blurred window

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
