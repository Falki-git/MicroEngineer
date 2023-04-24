﻿using BepInEx.Logging;
using KSP.Game;
using KSP.Messages;

namespace MicroMod
{
    internal class MessageManager
    {
        MicroEngineerMod _plugin;
        private Manager _manager;
        private UI _ui;
        internal List<BaseWindow> Windows;

        private static readonly ManualLogSource _logger = Logger.CreateLogSource("MicroEngineer.MessageManager");

        internal MessageManager(MicroEngineerMod plugin, Manager manager, UI ui)
        {
            _plugin = plugin;
            _manager = manager;
            _ui = ui;
            Windows = _manager.Windows;

            SubscribeToMessages();
        }

        /// <summary>
        /// Subscribe to KSP2 messages
        /// </summary>
        internal void SubscribeToMessages()
        {
            Utility.RefreshGameManager();

            // While in OAB we use the VesselDeltaVCalculationMessage event to refresh data as it's triggered a lot less frequently than Update()
            Utility.MessageCenter.Subscribe<VesselDeltaVCalculationMessage>(new Action<MessageCenterMessage>(this.RefreshStagingDataOAB));

            // We are loading layout state when entering Flight or OAB game state
            Utility.MessageCenter.Subscribe<GameStateEnteredMessage>(new Action<MessageCenterMessage>(this.GameStateEntered));

            // We are saving layout state when exiting from Flight or OAB game state
            Utility.MessageCenter.Subscribe<GameStateLeftMessage>(new Action<MessageCenterMessage>(this.GameStateLeft));

            // Sets the selected node index to the newly created node
            Utility.MessageCenter.Subscribe<ManeuverCreatedMessage>(new Action<MessageCenterMessage>(this.OnManeuverCreatedMessage));

            // Resets node index
            Utility.MessageCenter.Subscribe<ManeuverRemovedMessage>(new Action<MessageCenterMessage>(this.OnManeuverRemovedMessage));

            // Torque update for StageInfoOAB
            Utility.MessageCenter.Subscribe<PartManipulationCompletedMessage>(new Action<MessageCenterMessage>(this.OnPartManipulationCompletedMessage));
        }

        private void OnManeuverCreatedMessage(MessageCenterMessage message)
        {
            var maneuverWindow = Windows.Find(w => w.GetType() == typeof(ManeuverWindow)) as ManeuverWindow;
            maneuverWindow.OnManeuverCreatedMessage(message);
        }

        private void OnManeuverRemovedMessage(MessageCenterMessage message)
        {
            var maneuverWindow = Windows.Find(w => w.GetType() == typeof(ManeuverWindow)) as ManeuverWindow;
            maneuverWindow.OnManeuverRemovedMessage(message);
        }

        private void OnPartManipulationCompletedMessage(MessageCenterMessage obj)
        {
            EntryWindow stageInfoOabWindow = Windows.FindAll(w => w is EntryWindow).Cast<EntryWindow>().ToList().Find(w => w.MainWindow == MainWindow.StageInfoOAB);

            Torque torque = (Torque)Windows.FindAll(w => w is EntryWindow).Cast<EntryWindow>().ToList().Find(w => w.MainWindow == MainWindow.StageInfoOAB).Entries.Find(e => e.Name == "Torque");
            torque.RefreshData();
        }

        private void GameStateEntered(MessageCenterMessage obj)
        {
            _logger.LogInfo("Message triggered: GameStateEnteredMessage");

            Utility.RefreshGameManager();
            if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.VehicleAssemblyBuilder || Utility.GameState.GameState == GameState.Map3DView)
            {
                Utility.LoadLayout(Windows);
                _manager.Windows = Windows;
                _ui.Windows = Windows;

                if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.Map3DView)
                    _ui.ShowGuiFlight = Windows.OfType<MainGuiWindow>().FirstOrDefault().IsFlightActive;

                if (Utility.GameState.GameState == GameState.VehicleAssemblyBuilder)
                {
                    _ui.ShowGuiOAB = Windows.FindAll(w => w is EntryWindow).Cast<EntryWindow>().ToList().Find(w => w.MainWindow == MainWindow.StageInfoOAB).IsEditorActive;
                    _ui.CelestialBodies.GetBodies();
                    _ui.CelestialBodySelectionStageIndex = -1;
                    Styles.SetActiveTheme(Theme.Gray); // TODO implement other themes in OAB
                }
            }
        }

        private void GameStateLeft(MessageCenterMessage obj)
        {
            _logger.LogInfo("Message triggered: GameStateLeftMessage");

            Utility.RefreshGameManager();
            if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.VehicleAssemblyBuilder || Utility.GameState.GameState == GameState.Map3DView)
            {
                Utility.SaveLayout(Windows);

                if (Utility.GameState.GameState == GameState.FlightView || Utility.GameState.GameState == GameState.Map3DView)
                    _ui.ShowGuiFlight = false;

                if (Utility.GameState.GameState == GameState.VehicleAssemblyBuilder)
                    _ui.ShowGuiOAB = false;
            }
        }

        /// <summary>
        /// Refresh all staging data while in OAB
        /// </summary>
        private void RefreshStagingDataOAB(MessageCenterMessage obj)
        {
            // Check if message originated from ships in flight. If yes, return.
            VesselDeltaVCalculationMessage msg = (VesselDeltaVCalculationMessage)obj;
            if (msg.DeltaVComponent.Ship == null || !msg.DeltaVComponent.Ship.IsLaunchAssembly()) return;

            Utility.RefreshGameManager();
            if (Utility.GameState.GameState != GameState.VehicleAssemblyBuilder) return;

            Utility.RefreshStagesOAB();

            EntryWindow stageWindow = Windows.FindAll(w => w is EntryWindow).Cast<EntryWindow>().ToList().Find(w => w.MainWindow == MainWindow.StageInfoOAB);

            if (Utility.VesselDeltaVComponentOAB?.StageInfo == null)
            {
                stageWindow.Entries.Find(e => e.Name == "Stage Info (OAB)").EntryValue = null;
                return;
            }

            foreach (var entry in stageWindow.Entries)
                entry.RefreshData();
        }
    }
}