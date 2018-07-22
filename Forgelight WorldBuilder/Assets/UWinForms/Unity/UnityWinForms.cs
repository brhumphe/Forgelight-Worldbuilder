﻿namespace UWinForms.Unity
{
    using System.Drawing;
    using System.Windows.Forms;
    using API;
    using Core;
    using global::System;
    using global::System.Collections.Generic;
    using Utility;
    using Application = global::UWinForms.System.Windows.Forms.Application;
    using Color = UnityEngine.Color;
    using Cursor = global::UWinForms.System.Windows.Forms.Cursor;
    using Screen = global::UWinForms.System.Windows.Forms.Screen;
    using UE = UnityEngine;

    public sealed class UnityWinForms : UE.MonoBehaviour
    {
        public AppResources Resources;

        private static readonly List<invokeAction> actions = new List<invokeAction>();
        private static          UE.Texture2D       defaultSprite;

        private Application controller;
        private float       lastWidth;
        private float       lastHeight;
        private bool        shiftPressed;
        private float       shiftWait; // Shift pressing is really fast.
        private bool        paused;

        internal static UE.Texture2D DefaultSprite
        {
            get
            {
                if (defaultSprite != null) return defaultSprite;

                defaultSprite = new UE.Texture2D(32, 32);
                for (int i = 0; i < defaultSprite.height; i++)
                for (int k = 0; k < defaultSprite.width; k++)
                    defaultSprite.SetPixel(k, i, Color.white);
                defaultSprite.Apply();
                return defaultSprite;
            }
        }
        internal static AppGdiImages GdiImages { get; private set; }
        internal static AppResources gResources { get; private set; }

        internal static void Inspect(object obj)
        {
            if (obj == null)
                return;
#if UNITY_EDITOR
            if (Views.ControlInspector.Self != null)
                Views.ControlInspector.Self.DesignerObject = obj;
#endif
        }
        internal static invokeAction Invoke(Action a, float seconds)
        {
            if (a == null) return null;

            var ia = new invokeAction()
            {
                Action = a,
                Seconds = seconds
            };

            actions.Add(ia);
            return ia;
        }

        private void Awake()
        {
            ApiHolder.Graphics = new UnityGdi();
            ApiHolder.Input = new UnityInput();
            ApiHolder.System = new UnitySystem();
            ApiHolder.Timing = new UnityTiming();
            
            gResources = Resources;

            Screen.width = (int) (UE.Screen.width / Application.ScaleX);
            Screen.height = (int) (UE.Screen.height / Application.ScaleY);

            // Enum + dictionary?
            GdiImages = new AppGdiImages();
            GdiImages.ArrowDown = gResources.Images.ArrowDown.ToBitmap();
            GdiImages.ArrowLeft = gResources.Images.ArrowLeft.ToBitmap();
            GdiImages.ArrowRight = gResources.Images.ArrowRight.ToBitmap();
            GdiImages.ArrowUp = gResources.Images.ArrowUp.ToBitmap();
            GdiImages.Circle = gResources.Images.Circle.ToBitmap();
            GdiImages.Checked = gResources.Images.Checked.ToBitmap();
            GdiImages.Close = gResources.Images.Close.ToBitmap();
            GdiImages.CurvedArrowDown = gResources.Images.CurvedArrowDown.ToBitmap();
            GdiImages.CurvedArrowLeft = gResources.Images.CurvedArrowLeft.ToBitmap();
            GdiImages.CurvedArrowRight = gResources.Images.CurvedArrowRight.ToBitmap();
            GdiImages.CurvedArrowUp = gResources.Images.CurvedArrowUp.ToBitmap();
            GdiImages.DateTimePicker = gResources.Images.DateTimePicker.ToBitmap();
            GdiImages.DropDownRightArrow = gResources.Images.DropDownRightArrow.ToBitmap();
            GdiImages.FormResize = gResources.Images.FormResize.ToBitmap();
            GdiImages.NumericDown = gResources.Images.NumericDown.ToBitmap();
            GdiImages.NumericUp = gResources.Images.NumericUp.ToBitmap();
            GdiImages.RadioButton_Checked = gResources.Images.RadioButton_Checked.ToBitmap();
            GdiImages.RadioButton_Hovered = gResources.Images.RadioButton_Hovered.ToBitmap();
            GdiImages.RadioButton_Unchecked = gResources.Images.RadioButton_Unchecked.ToBitmap();
            GdiImages.TreeNodeCollapsed = gResources.Images.TreeNodeCollapsed.ToBitmap();
            GdiImages.TreeNodeExpanded = gResources.Images.TreeNodeExpanded.ToBitmap();

            GdiImages.Cursors.Default = gResources.Images.Cursors.Default.ToBitmap();
            GdiImages.Cursors.HSplit = gResources.Images.Cursors.HSplit.ToBitmap();
            GdiImages.Cursors.Hand = gResources.Images.Cursors.Hand.ToBitmap();
            GdiImages.Cursors.Help = gResources.Images.Cursors.Help.ToBitmap();
            GdiImages.Cursors.SizeAll = gResources.Images.Cursors.SizeAll.ToBitmap();
            GdiImages.Cursors.SizeNESW = gResources.Images.Cursors.SizeNESW.ToBitmap();
            GdiImages.Cursors.SizeNS = gResources.Images.Cursors.SizeNS.ToBitmap();
            GdiImages.Cursors.SizeNWSE = gResources.Images.Cursors.SizeNWSE.ToBitmap();
            GdiImages.Cursors.SizeWE = gResources.Images.Cursors.SizeWE.ToBitmap();
            GdiImages.Cursors.VSplit = gResources.Images.Cursors.VSplit.ToBitmap();

            ApplicationResources.Items = GdiImages;
            Cursors.images = GdiImages.Cursors;
            
            lastWidth = UE.Screen.width;
            lastHeight = UE.Screen.height;

            controller = new Application();
            controller.Resources = GdiImages;
            controller.UpdatePaintClipRect();

            Control.uwfDefaultController = controller;

#if UNITY_EDITOR
            MouseHook.MouseUp += (sender, args) => Inspect(sender);
#endif
        }
        private void Update()
        {
            var ueScreenWidth = UE.Screen.width;
            var ueScreenHeight = UE.Screen.height;

            Screen.width = (int) (ueScreenWidth / Application.ScaleX);
            Screen.height = (int) (ueScreenHeight / Application.ScaleY);

            if (controller != null)
            {
                if (lastWidth != ueScreenWidth || lastHeight != ueScreenHeight)
                {
                    Size deltaSize = new Size(
                        (int) (lastWidth - UE.Screen.width),
                        (int) (lastHeight - UE.Screen.height));
                    for (int i = 0; i < controller.ModalForms.Count; i++)
                        controller.ModalForms[i].uwfAddjustSizeToScreen(deltaSize);
                    for (int i = 0; i < controller.Forms.Count; i++)
                        controller.Forms[i].uwfAddjustSizeToScreen(deltaSize);
                    controller.UpdatePaintClipRect();
                }

                lastWidth = ueScreenWidth;
                lastHeight = ueScreenHeight;

                controller.Update();
            }

            for (int i = 0; i < actions.Count; i++)
            {
                var a = actions[i];
                a.Seconds -= UE.Time.deltaTime;
                if (a.Seconds > 0) continue;

                a.Action();
                actions.RemoveAt(i);
                i--;
            }
        }
        private void OnApplicationFocus(bool focusStatus)
        {
            paused = !focusStatus;

            UE.Cursor.visible = Cursor.IsVisible;
        }
        private void OnGUI()
        {
            if (controller == null) return;

            if (paused == false)
            {
                var currentEvent = UE.Event.current;
                
                var currentButton = currentEvent.button;
                var currentClicks = currentEvent.clickCount;
                var currentDelta = currentEvent.delta.y;
                var currentKeyCode = currentEvent.keyCode;
                var currentKeyModifiers = currentEvent.modifiers;
                var currentType = currentEvent.type;
                
                // Prepare mouse.
                var mouseButton = MouseButtons.None;
                var mouseEvent = Application.MouseEvents.None;
                var mouseWheelDelta = 0f;
                
                switch (currentType)
                {
                    case UE.EventType.MouseDown:
                        switch (currentButton)
                        {
                            case 0:
                                mouseButton = MouseButtons.Left;
                                mouseEvent = Application.MouseEvents.Down;
                                if (currentClicks > 1)
                                    mouseEvent = Application.MouseEvents.DoubleClick;
                                break;
                            case 1:
                                mouseButton = MouseButtons.Right;
                                mouseEvent = Application.MouseEvents.Down;
                                break;
                            case 2:
                                mouseButton = MouseButtons.Middle;
                                mouseEvent = Application.MouseEvents.Down;
                                break;
                        }

                        break;
                    case UE.EventType.MouseUp:
                        switch (currentButton)
                        {
                            case 0:
                                mouseButton = MouseButtons.Left;
                                mouseEvent = Application.MouseEvents.Up;
                                break;
                            case 1:
                                mouseButton = MouseButtons.Right;
                                mouseEvent = Application.MouseEvents.Up;
                                break;
                            case 2:
                                mouseButton = MouseButtons.Middle;
                                mouseEvent = Application.MouseEvents.Up;
                                break;
                        }

                        break;
                    case UE.EventType.ScrollWheel:
                        mouseEvent = Application.MouseEvents.Wheel;
                        mouseWheelDelta = currentDelta;
                        break;
                }

                // Mouse.
                var mouseX = UE.Input.mousePosition.x;
                var mouseY = UE.Screen.height - UE.Input.mousePosition.y;
                
                controller.ProccessMouse(mouseEvent, mouseX, mouseY, mouseButton, currentClicks, (int)mouseWheelDelta);

                // Keys.
                
                // Manualy set event for 'shift' key.
                if (shiftPressed && currentKeyCode == UE.KeyCode.None)
                {
                    if (shiftWait <= 0)
                    {
                        currentKeyCode = UE.KeyCode.LeftShift;
                        currentType = UE.EventType.KeyDown;

                        shiftWait = .2f;
                    }
                    else
                        shiftWait -= swfHelper.GetDeltaTime();
                }

                // Try release 'shift'. 
                // Fix: it's presses additional time when using Shift + any key.
                /*var prevShift = shiftPressed;
                shiftPressed = currentEvent.shift;

                if (prevShift && shiftPressed == false && currentKeyCode == UE.KeyCode.LeftShift)
                    currentEventType = UE.EventType.KeyUp;*/

                // Proccess.
                if (currentKeyCode != UE.KeyCode.None)
                {
                    var keyData = UnityKeyTranslator.ToKeyData(currentKeyModifiers, currentKeyCode);
                    var keyArgs = new KeyEventArgs(keyData);
                    keyArgs.uwfKeyCode = currentKeyCode;
                    keyArgs.uwfModifiers = currentKeyModifiers;
                    if ((keyArgs.uwfModifiers & UE.EventModifiers.FunctionKey) != 0)
                        keyArgs.uwfModifiers &= ~UE.EventModifiers.FunctionKey;

                    var keyEventType = (Application.KeyEvents) (currentType - 3);
                    if (keyEventType == Application.KeyEvents.Down || keyEventType == Application.KeyEvents.Up)
                        controller.ProccessKeys(keyArgs, keyEventType);
                }
            }

            // Scale if needed.
            var scaleX = Application.ScaleX;
            var scaleY = Application.ScaleY;
            if (scaleX != 1f || scaleY != 1f)
                UnityEngine.GUI.matrix = UnityEngine.Matrix4x4.TRS(
                    UE.Vector3.zero,
                    UE.Quaternion.AngleAxis(0, UE.Vector3.up),
                    new UE.Vector3(scaleX, scaleY, 1));

            controller.Redraw();
        }

        internal class invokeAction
        {
            public Action Action { get; set; }
            public float Seconds { get; set; }
        }
    }
}