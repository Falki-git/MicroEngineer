using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    [UxmlElement]
    public partial class SeparatorEntryControl : VisualElement
    {
        public static string UssClassName = "separator";

        public SeparatorEntryControl()
        {
            AddToClassList(UssClassName);
        }
    }
}