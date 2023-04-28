using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using DancingGoat.FormComponents;

namespace DancingGoat.Widgets
{
    public class VideoEmbedWidgetProperties : IWidgetProperties
    {
        public const string YOUTUBE = "youtube";
        public const string VIMEO = "vimeo";
        public const string DAILYMOTION = "dailymotion";
        public const string FILE = "file";

        private bool _showDimensions;


        /// <summary>
        /// Determines whether the dimensions controls should be shown
        /// </summary>
        [EditingComponent(InvisibleFormComponent.IDENTIFIER, Order = 0)]
        public bool ShowDimensions
        {
            get => Service == YOUTUBE || !DynamicSize;
            set => _showDimensions = value;
        }


        /// <summary>
        /// Defines the video platform from which the embedded video originates.
        /// </summary>
        [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "Video service", Order = 1)]
        [EditingComponentProperty(nameof(RadioButtonsComponent.Properties.DataSource), YOUTUBE + ";YouTube\r\n" + VIMEO + ";Vimeo\r\n" + DAILYMOTION + ";Dailymotion\r\n" + FILE + ";File URL\r\n")]
        public string Service { get; set; } = YOUTUBE;


        /// <summary>
        /// Defines the URL of the embedded video.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "Url", Order = 2)]
        public string Url { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "Url", Order = 3)]
        [VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, YOUTUBE)]
        public bool DynamicSize { get; set; } = true;


        /// <summary>
        /// Determines the width of the embed.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "Width (px)", Order = 4)]
        [VisibilityCondition(nameof(ShowDimensions), ComparisonTypeEnum.IsTrue)]
        public int Width { get; set; } = 560;


        /// <summary>
        /// Detemines the height of the embed.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "Height (px)", Order = 5)]
        [VisibilityCondition(nameof(ShowDimensions), ComparisonTypeEnum.IsTrue)]
        public int Height { get; set; } = 315;


        /// <summary>
        /// Defines the time to start the player at.
        /// </summary>
        [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "Play from beginning", Order = 6)]
        [VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, DAILYMOTION)]
        public bool PlayFromBeginning { get; set; } = true;


        /// <summary>
        /// Determines whether the video will start at the beginning, or at a specified timestamp.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "Starting time (seconds)", Order = 7)]
        [VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, DAILYMOTION)]
        [VisibilityCondition(nameof(PlayFromBeginning), ComparisonTypeEnum.IsFalse)]
        public int StartingTime { get; set; } = 0;


        /// <summary>
        /// Makes sure the controls of other properties referenced by visibility conditions are listened to for changes
        /// </summary>
        [EditingComponent(InvisibleFormComponent.IDENTIFIER, Order = 999)]
        [VisibilityCondition(nameof(DynamicSize), ComparisonTypeEnum.IsTrue)]
        public bool DummyProperty { get; set; }
    }
}
