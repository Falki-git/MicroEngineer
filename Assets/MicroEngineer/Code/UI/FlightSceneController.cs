using System.Collections.Generic;
using System.Linq;
using MicroEngineer.Managers;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    public class FlightSceneController
    {
        private static FlightSceneController _instance;
        private bool _showGui = false;
        private EditWindowsController _editWindowsController;
        private bool _maneuverWindowShown = true;
        private bool _targetWindowShown = true;
        private float _snapDistance = ((SettingsWindow)Manager.Instance.Windows.Find(w => w is SettingsWindow)).SnapDistance;
        private bool _isDragging = false;

        public UIDocument MainGui { get; set; }
        public List<UIDocument> Windows = new ();
        public UIDocument EditWindows { get; set; }
        
        private readonly WindowOptions _windowOptions = new()
        {
            IsHidingEnabled = true,
            MoveOptions = new MoveOptions
            {
                IsMovingEnabled = true,
                CheckScreenBounds = true
            }
        };

        public bool ShowGui
        {
            get => _showGui;
            set
            {
                _showGui = value;

                RebuildUI();
                
                // If UI is closing, close EditWindows as well
                if (!value && EditWindows != null)
                    ToggleEditWindows();
            }
        }

        public bool ManeuverWindowShown
        {
            get => _maneuverWindowShown;
            set
            {
                if (_maneuverWindowShown != value)
                {
                    _maneuverWindowShown = value;
                    RebuildUI();
                }
            }
        }

        public bool TargetWindowShown
        {
            get => _targetWindowShown;
            set
            {
                if (_targetWindowShown != value)
                {
                    _targetWindowShown = value;
                    RebuildUI();
                }
            }
        }

        public static FlightSceneController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new FlightSceneController();

                return _instance;
            }
        }

        public void InitializeUI()
        {
            //Build MainGui
            if ((Manager.Instance.Windows.Find(w => w is MainGuiWindow) as MainGuiWindow).IsFlightMinimized == false)
            {
                MainGui = Window.Create(Uxmls.Instance.InstantiateWindowOptions("MainGui"), Uxmls.Instance.BaseWindow);
                MainGuiController mainGuiController = MainGui.gameObject.AddComponent<MainGuiController>();
            }

            //Build poppedout windows
            foreach (EntryWindow poppedOutWindow in Manager.Instance.Windows.Where(w => w is EntryWindow && ((EntryWindow)w).IsFlightPoppedOut))
            {
                // Skip creating Maneuver and/or Target windows if maneuver/target do not exist
                if ((poppedOutWindow is ManeuverWindow && !ManeuverWindowShown) || (poppedOutWindow is TargetWindow && !TargetWindowShown))
                    continue;
                
                var window =
                    Window.Create(
                        Uxmls.Instance.InstantiateWindowOptions(poppedOutWindow.Name, !poppedOutWindow.IsLocked),
                        Uxmls.Instance.BaseWindow);
                var header = window.rootVisualElement.Q<VisualElement>("header");
                var body = window.rootVisualElement.Q<VisualElement>("body");
                var footer = window.rootVisualElement.Q<VisualElement>("footer");
                EntryWindowController ewc = new EntryWindowController(poppedOutWindow, window.rootVisualElement);
                body.Add(ewc.Root);

                if (poppedOutWindow.IsLocked)
                {
                    header.AddToClassList("no-border");
                    body.AddToClassList("no-border");
                    footer.AddToClassList("no-border");
                    var entryRoot = body.Q<VisualElement>("window-root");
                    entryRoot.AddToClassList("no-border");
                }

                //Handle window snapping
                window.rootVisualElement[0].RegisterCallback<MouseMoveEvent>(_ =>
                {
                    if (_isDragging)
                        HandleSnapping(window);
                });

                window.rootVisualElement[0].RegisterCallback<MouseDownEvent>(_ => _isDragging = true);
                window.rootVisualElement[0].RegisterCallback<MouseUpEvent>(_ => _isDragging = false);
                

                Windows.Add(window);
            }
        }

        public void RebuildUI()
        {
            DestroyUI();
            if (ShowGui)
                InitializeUI();
        }

        public void DestroyUI()
        {
            if (MainGui != null && MainGui.gameObject != null)
                MainGui.gameObject.DestroyGameObject();
            GameObject.Destroy(MainGui);

            if (Windows != null)
            {
                foreach (var w in Windows)
                {
                    if (w != null && w.gameObject != null)
                        w.gameObject?.DestroyGameObject();
                    GameObject.Destroy(w);
                }
                Windows.Clear();
            }
        }

        public void ToggleEditWindows() => ToggleEditWindows(false);
        public void ToggleEditWindows(bool needToOpenWithSpecificWindowSelected, int editableWindowId = 0)
        {
            if (EditWindows == null)
            {
                EditWindows = Window.Create(Uxmls.Instance.InstantiateWindowOptions("EditWindows"), Uxmls.Instance.EditWindows);

                EditWindows.rootVisualElement[0].RegisterCallback<GeometryChangedEvent>((evt) => Utility.CenterWindow(evt, EditWindows.rootVisualElement[0]));

                _editWindowsController = EditWindows.gameObject.AddComponent<EditWindowsController>();
                _editWindowsController.SelectedWindowId = editableWindowId;
            }
            else if (needToOpenWithSpecificWindowSelected)
            {
                _editWindowsController.SelectedWindowId = editableWindowId;
                _editWindowsController.ResetSelectedWindow();
            }
            else
            {
                var controller = EditWindows.GetComponent<EditWindowsController>();
                controller.CloseWindow();
                EditWindows = null;
            }
        }

        public List<EntryWindow> GetEditableWindows()
        {
            return Manager.Instance.Windows.FindAll(w => w is EntryWindow).Cast<EntryWindow>().ToList().FindAll(w => w.IsEditable);
        }

        public void HandleSnapping(UIDocument draggedWindow)
        {
            var draggedRect = draggedWindow.rootVisualElement[0].worldBound;

            foreach (var otherWindow in Windows)
            {
                var otherRect = otherWindow.rootVisualElement[0].worldBound;

                // Check if the current window is close to any edge of the other window
                if (otherWindow != draggedWindow && Utility.AreRectsNear(draggedRect, otherRect))
                {
                    var distance = 0f;

                    // Snap to the left edge
                    distance =  Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.xMin - otherRect.xMin); 
                    if (distance < _snapDistance && distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(otherRect.xMin, draggedWindow.rootVisualElement[0].worldBound.y);
                            = new Vector3(otherRect.xMin - draggedWindow.rootVisualElement[0].worldBound.xMin, 0);

                        break;
                    }

                    // Snap to the right edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.xMax - otherRect.xMin);
                    if (distance < _snapDistance && distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(otherRect.xMin - draggedWindow.rootVisualElement[0].worldBound.width, draggedWindow.rootVisualElement[0].worldBound.y);
                            = new Vector3(otherRect.xMin - draggedWindow.rootVisualElement[0].worldBound.width - draggedWindow.rootVisualElement[0].worldBound.xMin, 0);

                        break;
                    }

                    // Snap to the left edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.xMin - otherRect.xMax); 
                    if (distance < _snapDistance && distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(otherRect.xMax, draggedWindow.rootVisualElement[0].worldBound.y);
                            = new Vector3(otherRect.xMax - draggedWindow.rootVisualElement[0].worldBound.xMin, 0);

                        break;
                    }
                        

                    // Snap to the right edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.xMax - otherRect.xMax);
                    if (distance < _snapDistance &&  distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(otherRect.xMax - draggedWindow.rootVisualElement[0].worldBound.width, draggedWindow.rootVisualElement[0].worldBound.y);
                            = new Vector3(otherRect.xMax - draggedWindow.rootVisualElement[0].worldBound.width - draggedWindow.rootVisualElement[0].worldBound.xMin, 0);

                        break;
                    }

                    // Snap to the top edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.yMin - otherRect.yMin);
                    if (distance < _snapDistance && distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(draggedWindow.rootVisualElement[0].worldBound.x, otherRect.yMin);
                            = new Vector3(0, otherRect.yMin - draggedWindow.rootVisualElement[0].worldBound.yMin);

                        break;
                    }

                    // Snap to the bottom edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.yMax - otherRect.yMin); 
                    if (distance < _snapDistance &&  distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(draggedWindow.rootVisualElement[0].worldBound.x, otherRect.yMin - draggedWindow.rootVisualElement[0].worldBound.height);
                            = new Vector3(0, otherRect.yMin - draggedWindow.rootVisualElement[0].worldBound.height - draggedWindow.rootVisualElement[0].worldBound.yMin);

                        break;
                    }

                    // Snap to the top edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.yMin - otherRect.yMax);
                    if (distance < _snapDistance && distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(draggedWindow.rootVisualElement[0].worldBound.x, otherRect.yMax);
                            = new Vector3(0, otherRect.yMax - draggedWindow.rootVisualElement[0].worldBound.yMin);

                        break;
                    }

                    // Snap to the bottom edge
                    distance = Mathf.Abs(draggedWindow.rootVisualElement[0].worldBound.yMax - otherRect.yMax);
                    if (distance < _snapDistance && distance != 0)
                    {
                        draggedWindow.rootVisualElement[0].transform.position
                            // = new Vector3(draggedWindow.rootVisualElement[0].worldBound.x, otherRect.yMax - draggedWindow.rootVisualElement[0].worldBound.height);
                            = new Vector3(0, otherRect.yMax - draggedWindow.rootVisualElement[0].worldBound.height - draggedWindow.rootVisualElement[0].worldBound.yMin);
                    }
                }
            }
        }
    }
}