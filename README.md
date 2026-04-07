# 🌌 PvZ Constellation Translator Mod

![Mod Version](https://img.shields.io/badge/Version-0.2.0-brightgreen?style=for-the-badge)
![BepInEx](https://img.shields.io/badge/Requires-BepInEx_6-blue?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey?style=for-the-badge)

---

**PvZ Constellation Translator** is an advanced and comprehensive (All-in-One) localization framework built using the BepInEx 6 (Mono) framework for the fan-made game *Plants vs. Zombies: Constellation* (StarSign).

This mod goes beyond simple text replacement. It features an advanced hooking system to translate dynamic text, manipulate UI layouts in real-time, and provides a powerful scanning toolkit for developers and translators alike to handle the game's unique cosmic-themed interface.

---

## ✨ Key Features

Designed with ultimate flexibility, this mod allows you to modify things that were previously impossible to change.

### 1. 📖 Universal Text Translation
* **Multi-Engine Support**: Intercepts text from **UGUI**, **TextMeshPro (TMP)**, and **Legacy TextMesh 3D**.
* **Real-time Patching**: Automatically replaces Mandarin strings with localized versions via `translation_strings.json`.
* **Dynamic Regex**: Supports complex pattern matching for level numbers and dynamic stats using `translation_regexs.json`.

### 2. 🛠️ Absolute Path Detector (Scanner)
StarSign often uses nested UI that is hard to find. Our scanner bypasses all raycast blockers:
* **Absolute Scan (Ctrl + Right Click)**: Hover over any element to get its full "Path", "Internal Type", and "Raw Text".
* **Advanced Metrics**: Displays coordinates ($x$, $y$), object size ($w$, $h$), and pivot points—essential for UI overriding.
* **Texture Picker (Alt + Right Click)**: Instantly identifies the sprite names for future image replacement support.

### 3. 🖼️ Ultimate UI Overrides
Don't let long translations break the game's beauty. With `ui_overrides.json`, you can instantly modify any UI component:
* **Precision Scaling**: Force specific `width`, `height`, or `rotation`.
* **Smart Typography**: Adjust font `size`, disable forced auto-sizing (`bestfit`), and control text wrapping (`wrap` / `nowrap`).
* **Hacking Commands**:
  * `oneline=true` : Forces text to stay on a single line by injecting *Non-Breaking Spaces*.
  * `tabsize=X` : Converts `\t` tab characters into *X* normal spaces for perfect column alignments.

---

## 📥 Installation Guide (Step-by-Step)

To use this mod, you need the base game and **BepInEx 6 (Mono)**. Follow these steps carefully:

### Step 1: Installing BepInEx 6
1. Download the **BepInEx 6.0.0 (Mono x64)** from the official BepInEx GitHub repository.
2. Extract the downloaded `.zip` file.
3. Move all contents (including the `BepInEx` folder, `doorstop_config.ini`, and `winhttp.dll`) into your **PvZ Constellation game root directory** (where the `PlantsVsZombiesStarSign.exe` is located).
4. **Run the game once.** A black console window will appear. Wait until you reach the Main Menu, then close the game. This step is necessary to generate the required configuration folders.

### Step 2: Installing the Translator Mod
1. Download the latest `PvZStarSignTranslator.dll` and the `PvZ Constellation Translator` data folder from the **[Releases]** section of this GitHub.
2. Open your game folder and navigate to: `BepInEx/plugins/`.
3. Copy and paste the `PvZStarSignTranslator.dll` and the entire `PvZ Constellation Translator` folder into the `plugins` directory.
4. Your directory structure should look like this:
   `[Game Folder]/BepInEx/plugins/PvZStarSignTranslator.dll`
   `[Game Folder]/BepInEx/plugins/PvZ Constellation Translator/`

### Step 3: Verifying the Installation
1. Launch the game.
2. Check the black BepInEx console. You should see a message: `[Info :PvZ Constellation Translator] Starting PvZ Constellation Translator v0.1.0...`
3. If the folders are initialized, you are ready to mod!

---

## ⌨️ Quick Start Guide

* **Adding Text Translations**: Go to `BepInEx/plugins/PvZ Constellation Translator/Localization/Bahasa Indonesia/Strings/translation_strings.json`. Add the original Chinese text as the "Key" and your translation as the "Value".
* **Finding Paths for UI Overrides**: While in-game, hover your mouse over any text or button, hold **Left Ctrl**, and **Right-Click**. The mod will print the object's unique `Path` to the console. Copy this path into your `ui_overrides.json` to customize its layout.
* **Identifying Textures**: Use **Alt + Right Click** to quickly find the internal sprite name of any image you want to replace.

---

## 📸 Screenshots

### Indonesian Version
<table>
  <tr>
    <td><img src="https://via.placeholder.com/300x200?text=Main+Menu+Preview" alt="Main Menu"></td>
    <td><img src="https://via.placeholder.com/300x200?text=Almanac+Preview" alt="Almanac"></td>
    <td><img src="https://via.placeholder.com/300x200?text=Gameplay+Preview" alt="Gameplay"></td>
  </tr>
</table>

---

## 👨‍💻 Credits

* **Author/Developer**: [Ilham Nurjaman / Ilham Gimank]
* Developed with high dedication for the *Plants vs. Zombies* fan-made modding community.

Happy translating and modding! 🌠🧟‍♂️
