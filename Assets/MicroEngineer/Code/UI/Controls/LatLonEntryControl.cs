using UnityEngine;
using UnityEngine.UIElements;

namespace MicroEngineer.UI
{
    [UxmlElement]
    public partial class LatLonEntryControl : VisualElement
    {
        public static string UssClassName = "entry";
        public static string UssEntryClassName = UssClassName + "__name";

        public static string UssOuterValueClassName = UssClassName + "__outer-value";
        public static string UssInnerValueClassName = UssClassName + "__inner-value";
        public static string UssValueClassName = UssClassName + "__value";

        public static string UssInnerUnitClassName = UssClassName + "__inner-unit";
        public static string UssUnitClassName = UssClassName + "__unit";

        public const string UNIT_DEGREE = "°";
        public const string UNIT_MINUTE = "'";
        public const string UNIT_SECOND = "\"";

        public VisualElement ValueContainer;

        public Label NameLabel;
        [UxmlAttribute]
        public string EntryName
        {
            get => NameLabel.text;
            set => NameLabel.text = value;
        }

        public Label DegreesValueLabel;
        public Label DegreesUnitLabel;
        [UxmlAttribute]
        public string Degrees
        {
            get => DegreesValueLabel.text;
            set => DegreesValueLabel.text = value;
        }

        public Label MinutesValueLabel;
        public Label MinutesUnitLabel;
        [UxmlAttribute]
        public string Minutes
        {
            get => MinutesValueLabel.text;
            set => MinutesValueLabel.text = value;
        }

        public Label SecondsValueLabel;
        public Label SecondsUnitLabel;
        [UxmlAttribute]
        public string Seconds
        {
            get => SecondsValueLabel.text;
            set => SecondsValueLabel.text = value;
        }

        public Label UnitLabel;
        [UxmlAttribute]
        public string Unit
        {
            get => UnitLabel.text;
            set => UnitLabel.text = value;
        }

        public void SetValue(int degrees, int minutes, int seconds, string direction)
        {

            degrees = Mathf.Clamp(degrees, 0, 359);
            minutes = Mathf.Clamp(minutes, 0, 59);
            seconds = Mathf.Clamp(seconds, 0, 59);

            Degrees = degrees.ToString("0");
            Minutes = minutes.ToString("00");
            Seconds = seconds.ToString("00");
            Unit = direction;
        }

        // public LatLonEntryControl(BaseEntry entry) : this()
        // {
        //     EntryName = entry.Name;
        //
        //     var latLon = Utility.ParseDegreesToDMSFormat((double?)entry.EntryValue ?? 0);
        //     SetValue(latLon.Degrees, latLon.Minutes, latLon.Seconds, entry.BaseUnit);
        //
        //     entry.OnEntryLatLonChanged += HandleEntryLatLonChanged;
        // }

        public LatLonEntryControl()
        {
            AddToClassList(UssClassName);
            style.flexDirection = FlexDirection.Row;

            NameLabel = new Label()
            {
                //Name that you access with Q<Name>(NameHere)
                name = "entry-name",
                text = "placeholder"
            };
            NameLabel.AddToClassList(UssEntryClassName);
            hierarchy.Add(NameLabel);

            ValueContainer = new VisualElement();
            ValueContainer.style.flexGrow = 1;
            ValueContainer.style.flexDirection = FlexDirection.Row;
            ValueContainer.style.justifyContent = Justify.FlexEnd;
            hierarchy.Add(ValueContainer);

            {
                DegreesValueLabel = new Label()
                {
                    name = "degrees-value"
                };
                DegreesValueLabel.AddToClassList(UssValueClassName);
                DegreesValueLabel.AddToClassList(UssOuterValueClassName);
                ValueContainer.Add(DegreesValueLabel);
                DegreesUnitLabel = new Label()
                {
                    name = "degrees-unit",
                    text = UNIT_DEGREE
                };
                DegreesUnitLabel.AddToClassList(UssUnitClassName);
                DegreesUnitLabel.AddToClassList(UssInnerUnitClassName);
                ValueContainer.Add(DegreesUnitLabel);
            }//days

            {
                MinutesValueLabel = new Label()
                {
                    name = "minutes-value"
                };
                MinutesValueLabel.AddToClassList(UssValueClassName);
                MinutesValueLabel.AddToClassList(UssInnerValueClassName);
                ValueContainer.Add(MinutesValueLabel);
                MinutesUnitLabel = new Label()
                {
                    name = "minutes-unit",
                    text = UNIT_MINUTE
                };
                MinutesUnitLabel.AddToClassList(UssUnitClassName);
                MinutesUnitLabel.AddToClassList(UssInnerUnitClassName);
                ValueContainer.Add(MinutesUnitLabel);

            }//minutes

            {
                SecondsValueLabel = new Label()
                {
                    name = "seconds-value"
                };
                SecondsValueLabel.AddToClassList(UssValueClassName);
                SecondsValueLabel.AddToClassList(UssInnerValueClassName);
                ValueContainer.Add(SecondsValueLabel);
                SecondsUnitLabel = new Label()
                {
                    name = "seconds-unit",
                    text = UNIT_SECOND
                };
                SecondsUnitLabel.AddToClassList(UssUnitClassName);
                SecondsUnitLabel.AddToClassList(UssInnerUnitClassName);
                ValueContainer.Add(SecondsUnitLabel);
            }//seconds

            UnitLabel = new Label()
            {
                name = "entry-unit",
                text = string.Empty
            };
            UnitLabel.AddToClassList(UssUnitClassName);
            hierarchy.Add(UnitLabel);
        }

        // public void HandleEntryLatLonChanged(int degrees, int minutes, int seconds, string direction) => SetValue(degrees, minutes, seconds, direction);
    }
}