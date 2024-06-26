﻿using KSP.UI.Binding;
using MicroEngineer.Managers;
using MicroEngineer.Utilities;
using MicroEngineer.Windows;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    public class MainGuiController : MonoBehaviour
    {
        public MainGuiWindow MainGuiWindow { get; set; }
        public UIDocument MainGui { get; set; }
        public VisualElement Root { get; set; }
        public VisualElement Header { get; set; }
        public Button EditWindowsButton { get; set; }
        public Button CloseButton { get; set; }
        public Button MinimizeButton { get; set; }
        public VisualElement Body { get; set; }

        public MainGuiController()
        { }

        public void OnEnable()
        {
            MainGui = GetComponent<UIDocument>();
            Root = MainGui.rootVisualElement;
            Header = Root.Q<VisualElement>("header");
            BuildMainGuiHeader();
            Body = Root.Q<VisualElement>("body");
            BuildDockedWindows();

            Root[0].RegisterCallback<PointerUpEvent>(UpdateWindowPosition);

            MainGuiWindow = (MainGuiWindow)Manager.Instance.Windows.Find(w => w is MainGuiWindow);
            Root[0].transform.position = MainGuiWindow.FlightRect.position;
        }

        private void UpdateWindowPosition(PointerUpEvent evt)
        {
            if (MainGuiWindow == null)
                return;

            MainGuiWindow.FlightRect.position = Root[0].transform.position;
            Utility.SaveLayout();
        }

        public void Update()
        {
            return;
        }

        private void BuildMainGuiHeader()
        {
            var mainGuiHeader = Uxmls.Instance.MainGuiHeader.CloneTree();
            CloseButton = mainGuiHeader.Q<Button>("close-button");
            CloseButton.RegisterCallback<ClickEvent>(OnCloseButton);
            MinimizeButton = mainGuiHeader.Q<Button>("minimize-button");
            MinimizeButton.RegisterCallback<ClickEvent>(OnMinimizeButton);
            EditWindowsButton = mainGuiHeader.Q<Button>("editwindows-button");
            EditWindowsButton.RegisterCallback<ClickEvent>(evt => FlightSceneController.Instance.ToggleEditWindows());
            Header.Add(mainGuiHeader);
        }

        public void BuildDockedWindows()
        {
            foreach (EntryWindow entryWindow in Manager.Instance.Windows.Where(w => w is EntryWindow && !((EntryWindow)w).IsFlightPoppedOut))
            {
                EntryWindowController ewc = new EntryWindowController(entryWindow, Root);

                Body.Add(ewc.Root);
            }
        }

        private void OnCloseButton(ClickEvent evt)
        {            
            MainGuiWindow.IsFlightActive = false;

            Utility.SaveLayout();
            FlightSceneController.Instance.ShowGui = false;
            GameObject.Find("BTN-MicroEngineerBtn")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(false);
        }

        private void OnMinimizeButton(ClickEvent evt)
        {
            MainGuiWindow.IsFlightMinimized = true;

            Utility.SaveLayout();
            FlightSceneController.Instance.RebuildUI();
        }
    }
}
