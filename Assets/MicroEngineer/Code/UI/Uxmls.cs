using System;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.UIElements;
using ILogger = ReduxLib.Logging.ILogger;

namespace MicroEngineer.UI
{
    public class Uxmls
    {
        private static Uxmls _instance;
        private static readonly ILogger Logger = ReduxLib.ReduxLib.GetLogger("MicroEngineer.Uxmls");

        public const string MAIN_GUI_HEADER_PATH = "/microengineer_flightui/microengineer/ui/mainguiheader.uxml";
        public const string ENTRY_WINDOW_PATH = "/microengineer_flightui/microengineer/ui/entrywindow.uxml";
        public const string STAGE_INFO_HEADER_PATH = "/microengineer_flightui/microengineer/ui/stageinfoheader.uxml";
        public const string BASE_WINDOW_PATH = "/microengineer_flightui/microengineer/ui/basewindow.uxml";
        public const string EDIT_WINDOWS_PATH = "/microengineer_flightui/microengineer/ui/editwindows.uxml";
        public const string OAB_STAGE_INFO_PATH = "/microengineer_oabui/microengineer/ui/stageinfooab.uxml";
        public const string MANEUVER_HEADER_PATH = "/microengineer_flightui/microengineer/ui/maneuverheader.uxml";
        public const string MANEUVER_FOOTER_PATH = "/microengineer_flightui/microengineer/ui/maneuverfooter.uxml";

        public VisualTreeAsset MainGuiHeader;
        public VisualTreeAsset EntryWindow;
        public VisualTreeAsset StageInfoHeader;
        public VisualTreeAsset BaseWindow;
        public VisualTreeAsset EditWindows;
        public VisualTreeAsset StageInfoOAB;
        public VisualTreeAsset ManeuverHeader;
        public VisualTreeAsset ManeuverFooter;

        public static Uxmls Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Uxmls();

                return _instance;
            }
        }
        public Uxmls()
        {
            Initialize();
        }

        public void Initialize()
        {
            MainGuiHeader = LoadAsset($"{MAIN_GUI_HEADER_PATH}");
            EntryWindow = LoadAsset($"{ENTRY_WINDOW_PATH}");
            StageInfoHeader = LoadAsset($"{STAGE_INFO_HEADER_PATH}");
            BaseWindow = LoadAsset($"{BASE_WINDOW_PATH}");
            EditWindows = LoadAsset($"{EDIT_WINDOWS_PATH}");
            StageInfoOAB = LoadAsset($"{OAB_STAGE_INFO_PATH}");
            ManeuverHeader = LoadAsset($"{MANEUVER_HEADER_PATH}");
            ManeuverFooter = LoadAsset($"{MANEUVER_FOOTER_PATH}");
        }

        private VisualTreeAsset LoadAsset(string path)
        {
            Logger.LogInfo($"Loading asset at {path}");
            
            try
            {
                var result = path.StartsWith("/microengineer_flightui")
                    ? MicroEngineerPlugin.Instance.FlightUi.LoadAsset<VisualTreeAsset>(path.Replace("/microengineer_flightui","assets"))
                    : MicroEngineerPlugin.Instance.OabUi.LoadAsset<VisualTreeAsset>(path.Replace("/microengineer_oabui","assets"));

                if (result == null)
                {
                    Logger.LogError("Asset does not exist!");
                }
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to load VisualTreeAsset at path '{path}'\n" + ex.Message);
                return null;
            }
        }

        public WindowOptions InstantiateWindowOptions(string windowId, bool makeDraggable = true)
        {
            var options = WindowOptions.Default;
            options.WindowId = windowId;
            options.IsHidingEnabled = true;
            options.MoveOptions = new MoveOptions
            {
                IsMovingEnabled = makeDraggable,
                CheckScreenBounds = true
            };
            options.DisableGameInputForTextFields = true;
            return options;
        }
    }
}
