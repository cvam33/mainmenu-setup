# Main Menu Setup Wizard (UPM Package)

A comprehensive setup wizard and configurator dashboard for Unity to create, customize, and manage AAA game main menus. Supports both 2D and 3D gameplay presets, dynamic UI Toolkit layouts, input actions key-binding rebinding, and an audio management system.

## Features

- **Preset Wizard Setup:** Get started instantly with 6 presets: Single Player, Local Multiplayer, Online Multiplayer, Mobile, VR/XR, and Game Jam / Prototype.
- **Dynamic Render Dimension Setup:** Set up either URP 2D or 3D Renderers, Cinemachine virtual cameras, and standard input systems.
- **Controls Configuration Tab:** Exposed control binds for Keyboard & Controller. Bind and rebind input action keys directly inside the Editor window with interactive rebind overlays.
- **Sound Configuration Tab:**
  - Sequential background music playlist with loop toggle and dedicated volume slider.
  - UI SFX clip slots for Click, Toggle, Panel Change, and Navigate focus sounds, with individual volume controls.
  - Automatic runtime UI Toolkit bindings to buttons, sliders, dropdowns, and toggles.
- **Save Changes Separately:** Safe edit-time configuration changes, saved back to the scene GameObjects when clicking "Save Changes".

## Installation

You can install this package directly into your Unity project via git URL:

1. In the Unity Editor, open **`Window` → `Package Manager`**.
2. Click the **`+`** (plus) icon in the top-left corner.
3. Select **`Add package from git URL...`**.
4. Enter the GitHub repository URL (e.g., `https://github.com/lxcvam/mainmenu-setup.git` or your own URL).
5. Click **`Add`**.

## How to Use

1. Once installed, go to the Unity menu bar and select **`Tools` → `Main Menu Wizard`**.
2. Run through Steps 1 to 4 to verify dependencies, choose presets, configure properties, and generate the main menu assets.
3. On Step 6 (Dashboard), select your active MainMenu scene:
   - **Overview Tab:** Manage the scene reference to load next, and link custom Input Action assets.
   - **Controls Tab:** Configure and rebind controls in the scene.
   - **Sound Tab:** Drag and drop audio clips, toggle playlist looping, and adjust volume sliders.
4. Click **`Save Changes`** to write all changes to the scene.

## Developer Contact

- **Developer Name:** shivam
- **Help & Support Email:** lxcvam406@gmail.com
- **Short Quote:** "Simplify your game setup, amplify your player experience."
