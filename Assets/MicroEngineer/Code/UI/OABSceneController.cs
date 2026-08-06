using System.Linq;
using KSP.UI.Binding;
using MicroEngineer.Entries;
using MicroEngineer.Managers;
using MicroEngineer.Windows;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    public class OABSceneController
    {
        private static OABSceneController _instance;
        private bool _showGui = false;

        public PanelRenderer StageInfoWindow { get; set; }

        public bool ShowGui
        {
            get => _showGui;
            set
            {
                _showGui = value;

                GameObject.Find("BTN-MicroEngineerOAB")?.GetComponent<UIValue_WriteBool_Toggle>()?.SetValue(value);

                RebuildUI();
            }
        }

        public static OABSceneController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new OABSceneController();

                return _instance;
            }
        }

        public void InitializeUI()
        {
            StageInfoWindow = Window.Create(Uxmls.Instance.InstantiateWindowOptions("StageInfoOAB"), Uxmls.Instance.StageInfoOAB);
            StageInfoOABController controller = StageInfoWindow.gameObject.AddComponent<StageInfoOABController>();

            // Manually trigger StageInfo refreshing (regular refreshing is triggered by VesselDeltaVCalculationMessage event)
            MessageManager.Instance.RefreshStagingDataOAB();
        }

        public void RebuildUI()
        {
            DestroyUI();
            if (ShowGui)
                InitializeUI();
        }

        public void DestroyUI()
        {
            // Drop the handlers held by the controls we're about to destroy. The entries outlive the
            // UI, so stale handlers would keep writing to released VisualElements and - being
            // multicast delegates - block the rebuilt controls from ever being updated.
            var stageInfoOabWindow = Manager.Instance.Windows?.OfType<StageInfoOabWindow>().FirstOrDefault();
            foreach (var entry in stageInfoOabWindow?.Entries ?? Enumerable.Empty<BaseEntry>())
                entry?.ClearUiSubscriptions();

            if (StageInfoWindow != null && StageInfoWindow.gameObject != null)
                StageInfoWindow.gameObject.DestroyGameObject();
            GameObject.Destroy(StageInfoWindow);
        }
    }
}