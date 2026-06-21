using System.Collections.Generic;

namespace MCPForUnity.Editor.Helpers
{
    public static class MainMenuTemplateRegistry
    {
        public static string GetUXMLMainMenu()
        {
            return @"<ui:UXML xmlns:ui=""UnityEngine.UIElements"" xmlns:uie=""UnityEditor.UIElements"" xsi=""http://www.w3.org/2001/XMLSchema-instance"" engine=""UnityEngine.UIElements"" editor=""UnityEditor.UIElements"" noNamespaceSchemaLocation=""../../UIElementsSchema/UIElements.xsd"" editor-extension-mode=""False"">
    <Style src=""project://database/Assets/MainMenu1/UI/USS/MainMenu.uss"" />
    <ui:VisualElement name=""root"" class=""menu-root"">
        <ui:VisualElement name=""fade-overlay"" class=""fade-overlay"" />
        
        <ui:VisualElement name=""main-menu"" class=""menu-panel"">
            <ui:Label text=""MY AAA GAME"" class=""menu-title"" />
            <ui:VisualElement class=""menu-buttons-container"">
                <ui:Button text=""CONTINUE"" name=""continue-button"" class=""menu-button"" />
                <ui:Button text=""PLAY"" name=""play-button"" class=""menu-button"" />
                <ui:Button text=""SETTINGS"" name=""settings-button"" class=""menu-button"" />
                <ui:Button text=""CREDITS"" name=""credits-button"" class=""menu-button"" />
                <ui:Button text=""QUIT"" name=""quit-button"" class=""menu-button"" />
            </ui:VisualElement>
        </ui:VisualElement>
 
        <ui:VisualElement name=""settings-panel"" class=""menu-panel"" style=""display: none;"">
            <ui:VisualElement name=""rebind-overlay"" class=""rebind-overlay"" style=""display: none;"">
                <ui:Label text=""PRESS ANY KEY..."" class=""rebind-overlay-text"" />
            </ui:VisualElement>

            <ui:Label text=""SETTINGS"" class=""menu-title"" />
            
            <ui:VisualElement class=""settings-tabs-container"">
                <ui:Button text=""AUDIO"" name=""tab-audio-btn"" class=""settings-tab-button settings-tab-button--active"" />
                <ui:Button text=""GRAPHICS"" name=""tab-graphics-btn"" class=""settings-tab-button"" />
                <ui:Button text=""CONTROLS"" name=""tab-controls-btn"" class=""settings-tab-button"" />
            </ui:VisualElement>

            <ui:VisualElement class=""settings-container"">
                <ui:VisualElement name=""tab-content-audio"" class=""settings-tab-content"">
                    <ui:Slider label=""Master Volume"" name=""master-volume-slider"" value=""0.75"" high-value=""1"" class=""settings-slider"" />
                    <ui:Slider label=""Music Volume"" name=""music-volume-slider"" value=""0.75"" high-value=""1"" class=""settings-slider"" />
                    <ui:Slider label=""SFX Volume"" name=""sfx-volume-slider"" value=""0.75"" high-value=""1"" class=""settings-slider"" />
                </ui:VisualElement>

                <ui:VisualElement name=""tab-content-graphics"" class=""settings-tab-content"" style=""display: none;"">
                    <ui:DropdownField label=""Resolution"" name=""resolution-dropdown"" class=""settings-dropdown"" />
                    <ui:Toggle label=""Fullscreen"" name=""fullscreen-toggle"" class=""settings-toggle"" />
                    <ui:DropdownField label=""Quality"" name=""quality-dropdown"" class=""settings-dropdown"" />
                    <ui:Toggle label=""VSync"" name=""vsync-toggle"" class=""settings-toggle"" />
                </ui:VisualElement>

                <ui:VisualElement name=""tab-content-controls"" class=""settings-tab-content"" style=""display: none;"">
                    <ui:VisualElement class=""controls-sub-tabs-container"">
                        <ui:Button text=""KEYBOARD"" name=""controls-sub-tab-keyboard-btn"" class=""controls-sub-tab-button controls-sub-tab-button--active"" />
                        <ui:Button text=""CONTROLLER"" name=""controls-sub-tab-controller-btn"" class=""controls-sub-tab-button"" />
                    </ui:VisualElement>
                    <ui:ScrollView name=""controls-keyboard-scroll"" class=""keybinds-scroll-view"" />
                    <ui:ScrollView name=""controls-controller-scroll"" class=""keybinds-scroll-view"" style=""display: none;"" />
                </ui:VisualElement>

                <ui:Button text=""BACK"" name=""settings-back-button"" class=""menu-button"" />
            </ui:VisualElement>
        </ui:VisualElement>

        <ui:VisualElement name=""credits-panel"" class=""menu-panel"" style=""display: none;"">
            <ui:Label text=""CREDITS"" class=""menu-title"" />
            <ui:ScrollView class=""credits-scroll"">
                <ui:Label text=""DEVELOPED BY SHIVAM"" class=""credits-text"" />
                <ui:Label text=""HELP &amp; SUPPORT: lxcvam406@gmail.com"" class=""credits-text"" />
                <ui:Label text=""BUILT WITH UNITY 6"" class=""credits-text"" />
                <ui:Label text=""&quot;Imagination is the blueprint of reality.&quot;"" class=""credits-text"" style=""font-style: italic; color: #00ADB5; margin-top: 15px;"" />
            </ui:ScrollView>
            <ui:Button text=""BACK"" name=""credits-back-button"" class=""menu-button"" />
        </ui:VisualElement>
    </ui:VisualElement>
</ui:UXML>";
        }

        public static string GetUSSMainMenu()
        {
            return @".menu-root {
    flex-grow: 1;
    background-color: rgba(18, 18, 18, 0.95);
    font-size: 20px;
    color: white;
}

/* Alignment Classes */
.menu-root--left {
    align-items: flex-start;
    justify-content: center;
}
.menu-root--left .menu-panel {
    margin-left: 100px;
}
.menu-root--right {
    align-items: flex-end;
    justify-content: center;
}
.menu-root--right .menu-panel {
    margin-right: 100px;
}
.menu-root--middle-center {
    align-items: center;
    justify-content: center;
}
.menu-root--top {
    align-items: center;
    justify-content: flex-start;
}
.menu-root--top .menu-panel {
    margin-top: 50px;
}
.menu-root--bottom {
    align-items: center;
    justify-content: flex-end;
}
.menu-root--bottom .menu-panel {
    margin-bottom: 50px;
}

.fade-overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: black;
    opacity: 0;
}

.menu-panel {
    width: 600px;
    padding: 40px;
    background-color: rgba(30, 30, 30, 0.9);
    border-width: 2px;
    border-color: rgba(100, 100, 100, 0.3);
    border-radius: 10px;
    align-items: center;
}

.menu-title {
    font-size: 48px;
    -unity-font-style: bold;
    color: #00ADB5;
    margin-bottom: 40px;
}

.menu-buttons-container {
    width: 100%;
    align-items: center;
}

/* AAA Button styling: Transparent by default, glowing accents on hover */
.menu-button {
    width: 80%;
    padding: 12px;
    margin: 8px 0;
    font-size: 22px;
    -unity-font-style: bold;
    background-color: rgba(0, 0, 0, 0);
    color: #E5E7EB;
    border-width: 0px;
    border-left-width: 0px;
    border-left-color: rgba(0, 173, 181, 0);
    border-radius: 4px;
    transition-property: background-color, color, scale, border-left-width, border-left-color, padding-left;
    transition-duration: 0.2s;
    transition-timing-function: ease-out-back;
}

.menu-button:hover, .menu-button:focus {
    color: #00ADB5;
    background-color: rgba(0, 173, 181, 0.12);
    border-left-width: 4px;
    border-left-color: #00ADB5;
    padding-left: 20px;
    scale: 1.05 1.05;
}

.settings-container {
    width: 100%;
}

.settings-slider {
    margin: 15px 0;
}

.settings-toggle {
    margin: 15px 0;
}

.credits-scroll {
    height: 200px;
    width: 100%;
    margin-bottom: 20px;
}

.credits-text {
    font-size: 18px;
    margin: 5px 0;
    -unity-text-align: middle-center;
}

.settings-tabs-container {
    flex-direction: row;
    justify-content: center;
    margin-bottom: 20px;
    width: 100%;
}

.settings-tab-button {
    flex-grow: 1;
    padding: 10px;
    font-size: 16px;
    background-color: rgba(0, 0, 0, 0);
    color: #E5E7EB;
    border-width: 1px;
    border-color: #393E46;
    border-radius: 0;
    transition-property: background-color, color;
    transition-duration: 0.15s;
}

.settings-tab-button:hover {
    background-color: rgba(255, 255, 255, 0.05);
}

.settings-tab-button--active {
    background-color: #00ADB5;
    color: #222831;
    border-color: #00ADB5;
    -unity-font-style: bold;
}

.settings-tab-content {
    width: 100%;
    margin-bottom: 20px;
}

.settings-dropdown {
    margin: 15px 0;
}

.controls-header {
    font-size: 18px;
    -unity-font-style: bold;
    margin-bottom: 10px;
    color: #00ADB5;
}

.keybinds-scroll-view {
    max-height: 250px;
    background-color: rgba(20, 20, 20, 0.5);
    padding: 10px;
    border-radius: 5px;
}

.controls-sub-tabs-container {
    flex-direction: row;
    justify-content: center;
    margin-bottom: 10px;
    width: 100%;
}

.controls-sub-tab-button {
    flex-grow: 1;
    padding: 6px;
    font-size: 14px;
    background-color: rgba(0, 0, 0, 0);
    color: #E5E7EB;
    border-width: 1px;
    border-color: #393E46;
    border-radius: 0;
    transition-property: background-color, color;
    transition-duration: 0.15s;
}

.controls-sub-tab-button:hover {
    background-color: rgba(255, 255, 255, 0.05);
}

.controls-sub-tab-button--active {
    background-color: #00ADB5;
    color: #222831;
    border-color: #00ADB5;
    -unity-font-style: bold;
}

.keybind-row {
    flex-direction: row;
    justify-content: space-between;
    align-items: center;
    padding: 8px 0;
    border-bottom-width: 1px;
    border-bottom-color: rgba(255, 255, 255, 0.1);
}

.keybind-label {
    font-size: 14px;
    color: white;
    flex-grow: 1;
}

.keybind-buttons-container {
    flex-direction: row;
}

.keybind-button {
    width: 100px;
    margin-left: 5px;
    background-color: #393E46;
    color: white;
    border-width: 1px;
    border-color: #222831;
    border-radius: 3px;
    padding: 5px;
    font-size: 13px;
}

.keybind-button:hover {
    background-color: #00ADB5;
    color: #222831;
}

.keybind-button:disabled {
    background-color: #1a1a1a;
    color: #555555;
    border-color: #1a1a1a;
}

.rebind-overlay {
    position: absolute;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.85);
    justify-content: center;
    align-items: center;
    border-radius: 10px;
}

.rebind-overlay-text {
    font-size: 28px;
    -unity-font-style: bold;
    color: #00ADB5;
}";
        }
    }
}
