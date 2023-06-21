﻿using BepInEx.Logging;
using MicroMod;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    public class EditWindowsController : MonoBehaviour
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("MicroEngineer.EditWindowsController");
        
        private EditWindowsItemControl _selectedAvailableEntry;
        private EditWindowsItemControl _selectedInstalledEntry;
        private List<EntryWindow> _editableWindows;
        private List<EditWindowsItemControl> _installedControls = new();
        private int _selectedWindowId = 0;

        public UIDocument EditWindows { get; set; }
        public VisualElement Root { get; set; }
        public Button CloseButton { get; set; }
        public ScrollView AvailableScrollView { get; set; }
        public ScrollView InstalledScrollView { get; set; }
        public DropdownField CategoryDropdown { get; set; }        
        public TextField SelectedWindow { get; set; }
        public Button PreviousWindow { get; set; }
        public Button NextWindow { get; set; }
        public Button NewWindow { get; set; }
        public Button DeleteWindow { get; set; }
        public Toggle LockWindow { get; set; }
        public Button AddEntry { get; set; }
        public Button RemoveEntry { get; set; }
        public Button MoveUp { get; set; }
        public Button MoveDown { get; set; }

        // TODO revert button, save button
        // TODO changed disable uss

        public EditWindowsController()
        { }

        public void OnEnable()
        {
            _logger.LogDebug("Entering OnEnable() of EditWindowsController");
            EditWindows = GetComponent<UIDocument>();
            Root = EditWindows.rootVisualElement;

            CloseButton = Root.Q<Button>("close-button");
            CloseButton.RegisterCallback<PointerUpEvent>(_ => CloseWindow());
            AvailableScrollView = Root.Q<ScrollView>("available-scrollview");
            InstalledScrollView = Root.Q<ScrollView>("installed-scrollview");
            CategoryDropdown = Root.Q<DropdownField>("category__dropdown");
            SelectedWindow = Root.Q<TextField>("selected-window");
            SelectedWindow.RegisterValueChangedCallback(RenameWindow);
            PreviousWindow = Root.Q<Button>("prev-window");
            PreviousWindow.RegisterCallback<PointerUpEvent>(SelectPreviousWindow);
            NextWindow = Root.Q<Button>("next-window");
            NextWindow.RegisterCallback<PointerUpEvent>(SelectNextWindow);
            NewWindow = Root.Q<Button>("new-window");
            NewWindow.RegisterCallback<PointerUpEvent>(CreateNewWindow);
            DeleteWindow = Root.Q<Button>("delete-window");
            DeleteWindow.RegisterCallback<PointerUpEvent>(DeleteDeletableWindow);
            LockWindow = Root.Q<Toggle>("lock-window");
            LockWindow.RegisterValueChangedCallback(LockUnlockWindow);
            AddEntry = Root.Q<Button>("add-entry");
            AddEntry.RegisterCallback<PointerUpEvent>(AddEntryToSelectedWindow);
            AddEntry.SetEnabled(false);
            RemoveEntry = Root.Q<Button>("remove-entry");
            RemoveEntry.RegisterCallback<PointerUpEvent>(RemoveEntryFromInstalledWindow);
            RemoveEntry.SetEnabled(false);
            MoveUp = Root.Q<Button>("move-up");
            MoveUp.RegisterCallback<PointerUpEvent>(MoveEntryUp);
            MoveDown = Root.Q<Button>("move-down");
            MoveDown.RegisterCallback<PointerUpEvent>(MoveEntryDown);

            BuildCategoryDropdown();
            GetEditableWindows();
            ResetSelectedWindow();

            //BuildMainGuiHeader();
            //BuildDockedWindows();

            //Root[0].RegisterCallback<PointerUpEvent>(UpdateWindowPosition);
        }

        public void Update()
        {
            return;
        }

        //// AVAILABLE ////
        private void BuildCategoryDropdown()
        {
            CategoryDropdown.choices = Enum.GetNames(typeof(MicroEntryCategory)).ToList();
            CategoryDropdown.RegisterValueChangedCallback(BuildAvailableEntries);
        }       

        private void BuildAvailableEntries(ChangeEvent<string> _)
        {
            AvailableScrollView.Clear();
            _selectedAvailableEntry = null;
            AddEntry.SetEnabled(false);
            if (CategoryDropdown.value == null)
                return;

            List<BaseEntry> entriesByCategory = Manager.Instance.Entries.FindAll(e => e.Category.ToString() == CategoryDropdown.value); // All entries belong to a category, but they can still be placed in any window
            foreach (var e in entriesByCategory)
            {
                var control = new EditWindowsItemControl(e, true);
                var textField = control.Q<TextField>();
                textField.RegisterCallback<MouseDownEvent>(evt => OnAvailableEntryClicked(evt, control));
                AvailableScrollView.Add(control);
            }
        }

        private void OnAvailableEntryClicked(MouseDownEvent evt, EditWindowsItemControl control)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (control != _selectedAvailableEntry)
                    SelectAvailable(control);
                else
                    UnselectAvailable(control);
            }
        }

        /// <summary>
        /// Implement your logic here when the label is selected
        /// For example, change the label's appearance or perform some action.
        /// </summary>
        public void SelectAvailable(EditWindowsItemControl control)
        {
            _logger.LogDebug("Select entered");

            if (_selectedAvailableEntry != null && _selectedAvailableEntry != control)
            {
                _selectedAvailableEntry.Unselect();
            }

            _selectedAvailableEntry = control;
            control.Select();
            AddEntry.SetEnabled(true);
        }

        /// <summary>
        /// Implement your logic here when the label is unselected
        /// For example, revert the label's appearance or perform some action.
        /// </summary>
        public void UnselectAvailable(EditWindowsItemControl control)
        {
            _logger.LogDebug("Unselect entered");
            if (control == _selectedAvailableEntry)
            {
                _selectedAvailableEntry = null;
            }
            control.Unselect();
            AddEntry.SetEnabled(false);
        }

        //// INSTALLED ////
        private void GetEditableWindows()
        {
            _editableWindows = Manager.Instance.Windows.FindAll(w => w is EntryWindow).Cast<EntryWindow>().ToList().FindAll(w => w.IsEditable);
        }

        private void ResetSelectedWindow()
        {
            SelectedWindow.SetValueWithoutNotify(_editableWindows[_selectedWindowId].Name);
            BuildInstalledEntries();
            RemoveEntry.SetEnabled(false);
            DeleteWindow.SetEnabled(_editableWindows[_selectedWindowId].IsDeletable);
            LockWindow.value = _editableWindows[_selectedWindowId].IsLocked;
        }

        private void BuildInstalledEntries()
        {
            _installedControls.Clear();
            InstalledScrollView.Clear();
            _selectedInstalledEntry = null;

            List<BaseEntry> installedEntries = _editableWindows[_selectedWindowId].Entries;
            foreach (var e in installedEntries)
            {
                var control = new EditWindowsItemControl(e, false);
                var textField = control.Q<TextField>();
                textField.RegisterCallback<MouseDownEvent>(evt => OnInstalledEntryClicked(evt, control));
                textField.RegisterValueChangedCallback(evt => RenameEntry(evt, control));
                _installedControls.Add(control);
                InstalledScrollView.Add(control);
            }
        }

        private void OnInstalledEntryClicked(MouseDownEvent evt, EditWindowsItemControl control)
        {
            if (evt.button == (int)MouseButton.LeftMouse)
            {
                if (control != _selectedInstalledEntry)
                    SelectInstalled(control);
                else
                    UnselectInstalled(control);
            }
        }

        public void SelectInstalled(EditWindowsItemControl control)
        {
            _logger.LogDebug("Select entered");

            if (_selectedInstalledEntry != null && _selectedInstalledEntry != control)
            {
                _selectedInstalledEntry.Unselect();
            }

            _selectedInstalledEntry = control;
            control.Select();
            RemoveEntry.SetEnabled(true);
        }

        public void UnselectInstalled(EditWindowsItemControl control)
        {
            _logger.LogDebug("Unselect entered");
            if (control == _selectedInstalledEntry)
            {
                _selectedInstalledEntry = null;
            }
            control.Unselect();
            RemoveEntry.SetEnabled(false);
        }

        private void SelectPreviousWindow(PointerUpEvent evt)
        {
            if (_selectedWindowId > 0)
                _selectedWindowId--;
            else
                _selectedWindowId = _editableWindows.Count - 1;

            ResetSelectedWindow();
        }

        private void SelectNextWindow(PointerUpEvent evt)
        {
            if (_selectedWindowId < _editableWindows.Count-1)
                _selectedWindowId++;
            else
                _selectedWindowId = 0;

            ResetSelectedWindow();
        }

        private void MoveEntryUp(PointerUpEvent evt)
        {
            if (_selectedInstalledEntry == null)
                return;

            var index = _installedControls.IndexOf(_selectedInstalledEntry);
            if (index == 0)
                return;

            _editableWindows[_selectedWindowId].MoveEntryUp(_selectedInstalledEntry.Entry);
            
            ResetSelectedWindow();
            RebuildFlightUI();

            var control = _installedControls[index - 1];
            SelectInstalled(control);
        }

        private void MoveEntryDown(PointerUpEvent evt)
        {
            if (_selectedInstalledEntry == null)
                return;

            var index = _installedControls.IndexOf(_selectedInstalledEntry);
            if (index == _installedControls.Count - 1)
                return;

            _editableWindows[_selectedWindowId].MoveEntryDown(_selectedInstalledEntry.Entry);

            ResetSelectedWindow();
            RebuildFlightUI();

            var control = _installedControls[index + 1];
            SelectInstalled(control);
        }

        private void AddEntryToSelectedWindow(PointerUpEvent evt)
        {
            _editableWindows[_selectedWindowId].AddEntry(Activator.CreateInstance(_selectedAvailableEntry.Entry.GetType()) as BaseEntry);

            UnselectAvailable(_selectedAvailableEntry);
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void RemoveEntryFromInstalledWindow(PointerUpEvent evt)
        {
            _editableWindows[_selectedWindowId].RemoveEntry(_selectedInstalledEntry.Entry);
            UnselectInstalled(_selectedInstalledEntry);
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void RenameEntry(ChangeEvent<string> evt, EditWindowsItemControl control)
        {
            control.Entry.Name = evt.newValue;
            RebuildFlightUI();
        }

        private void RenameWindow(ChangeEvent<string> evt)
        {
            _editableWindows[_selectedWindowId].Name = evt.newValue;
            RebuildFlightUI();
        }

        private void CreateNewWindow(PointerUpEvent evt)
        {
            _selectedWindowId = Manager.Instance.CreateCustomWindow(_editableWindows);
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void DeleteDeletableWindow(PointerUpEvent evt)
        {
            if (_editableWindows[_selectedWindowId].IsDeletable)
            {
                Manager.Instance.Windows.Remove(_editableWindows[_selectedWindowId]);
                _editableWindows.Remove(_editableWindows[_selectedWindowId]);
                _selectedWindowId--;
            }
            ResetSelectedWindow();
            RebuildFlightUI();
        }

        private void LockUnlockWindow(ChangeEvent<bool> evt)
        {
            _editableWindows[_selectedWindowId].IsLocked = evt.newValue;
            if (evt.newValue)
                _editableWindows[_selectedWindowId].IsFlightActive = true;

            RebuildFlightUI();
        }

        private void RebuildFlightUI()
        {
            Utility.SaveLayout(Manager.Instance.Windows);
            FlightSceneController.Instance.RebuildUI();
        }

        public void CloseWindow()
        {
            if (EditWindows != null && EditWindows.gameObject != null)
                EditWindows.gameObject.DestroyGameObject();
            GameObject.Destroy(EditWindows);
        }
    }
}
