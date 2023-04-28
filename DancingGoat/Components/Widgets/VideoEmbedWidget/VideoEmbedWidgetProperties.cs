using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using DancingGoat.FormComponents;

namespace DancingGoat.Widgets
{
    public class VideoEmbedWidgetProperties :IWidgetProperties
    {
        public const string YOUTUBE = "youtube";
        public const string VIMEO = "vimeo";
        public const string DAILYMOTION = "dailymotion";
        public const string FILE = "file";

        private bool _showDimensions;

        /// <summary>
        /// Holds a complex boolean expression used in determinint other fields' visiblity.
        /// </summary>
        [InvisibleComponent(Order = 0)]
        public bool ShowDimensions
        {
            get => Service == YOUTUBE || !DynamicSize;
            set => _showDimensions = value;
        }


        /// <summary>
        /// Defines the video platform from which the embedded video originates.
        /// </summary>
        [RadioGroupComponent(Label = "Video service", Inline = true, Order = 1, Options = YOUTUBE + ";YouTube\r\n" + VIMEO + ";Vimeo\r\n" + DAILYMOTION + ";Dailymotion\r\n" + FILE + ";File URL\r\n")]
        public string Service { get; set; } = YOUTUBE;
        
        
        /// <summary>
        /// Defines the URL of the embedded video.
        /// </summary>
        [TextInputComponent(Label = "Url", Order = 2)]
        public string Url { get; set; }


        /// <summary>
        /// Determines whether the video should be sized dynamically or with explicit dimensions.
        /// </summary>
        [CheckBoxComponent(Label = "Size dynamically", Order = 3)]
        [VisibleIfNotEqualTo(nameof(Service), YOUTUBE)]
        public bool DynamicSize { get; set; } = true;


        /// <summary>
        /// Determines the width of the embed.
        /// </summary>
        [NumberInputComponent(Label = "Width (px)", Order = 4)]
        [VisibleIfTrue(nameof(ShowDimensions))]
        public int Width { get; set; } = 560;


        /// <summary>
        /// Detemines the height of the embed.
        /// </summary>
        [NumberInputComponent(Label = "Height (px)", Order = 5)]
        [VisibleIfTrue(nameof(ShowDimensions))]
        public int Height { get; set; } = 315;


        /// <summary>
        /// Defines the time to start the player at.
        /// </summary>        
        [CheckBoxComponent(Label = "Play from beginning", Order = 6)]
        [VisibleIfNotEqualTo(nameof(Service), DAILYMOTION)]
        public bool PlayFromBeginning { get; set; } = true;


        /// <summary>
        /// Determines whether the video will start at the beginning, or at a specified timestamp.
        /// </summary>
        [NumberInputComponent(Label = "Starting time (seconds)", Order = 7)]
        [VisibleIfFalse(nameof(PlayFromBeginning))]
        [VisibleIfNotEqualTo(nameof(Service), DAILYMOTION)]
        public int StartingTime { get; set; } = 0;


        /// <summary>
        /// Makes sure all necessary properties used in ShowDimensions are listened to.
        /// </summary>
        [InvisibleComponent(Order = 999)]
        [VisibleIfFalse(nameof(DynamicSize))]
        public bool DummyProperty { get; set; }
    }
}