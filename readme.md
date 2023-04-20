# Creating a video embed widget with visibility conditions

Since the October 2022 refresh, Xperience by Kentico has supported the use of multiple visibility conditions in the properties of page builder and form builder components. This means that the visibility of one property can be dependent on multiple other properties at the same time.

Let's create a widget that demonstrates this functionality. A widget for embedding videos in the page could be a good opportunity for this-- Since certain features may vary depending on the video service provider, we can use visibility conditions to make use of the available features for each service, depending on the selection.

For the purposes of this example, the widget will be added into the **Dancing Goat** sample site, and will use *DancingGoat* namespaces. It will also include code snippets both for older, backwards-compatible C# form components, and newer React-based form components.



## Setting the goal

The goal of our widget will be to display a video embed in a page. Most video content shared on the internet is hosted on video sharing platforms, so we should include some of these services as options, along with the ability to embed a publicly accessible video file via the HTML `<video>` tag.

However, using multiple video sharing platforms introduces some complication-- not all platforms have the same features. For instance, Youtube and Vimeo allow for a video to be started midway, at a certain point in time, while Dailymotion does not. Conversely, Vimeo and Dailymotion can be sized dynamically (percentage-based widths in CSS), while Youtube did not play well with this in my testing.

Based on these available features, we can use visibility conditions to show or hide different configuration options.






## Defining the properties

### **Basic properties**

In the Dancing Goat project, go to **~/Components/Widgets** and add a new folder called **VideoEmbedWidget**. Within this folder, define a properties class that inherits from `IWidgetProperties`.

```c#
namespace DancingGoat.Widgets
{
    public class VideoEmbedWidgetProperties : IWidgetProperties
    {
        //...
    }
}
```

Add the necessary `using` directives to the file.

```c#
//For using old C# components
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using DancingGoat.FormComponents;
```

```c#
//For using React components
using Kentico.Forms.Web.Mvc;
using Kentico.PageBuilder.Web.Mvc;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using DancingGoat.FormComponents;
```

Next, define some constants within the class to hold the code names of the services we will use. Let's go with the examples cited above.
```c#
public const string YOUTUBE = "youtube";
public const string VIMEO = "vimeo";
public const string DAILYMOTION = "dailymotion";
public const string FILE = "file";
```
Now let's consider what kinds of properties we'll need.

Since the video can come from several places, and we'll need to react differently based on where, let's add property that signifies which service a video is from, and another to hold the url of the video itself.
```c#
public string Service { get; set; }

public string Url { get; set; }
```
Next, we can signify whether the video should be sized dynamically, with a boolean property, and also add properties for the dimensions when its size is explicitly specified.
```c#
public bool DynamicSize { get; set; }

public int Width { get; set; }

public int Height { get; set; }
```
Finally, let's add properties to specify whether the video should be played from the beginning, or from a timestamp, and what that timestamp should be.
```c#
public bool PlayFromBeginning { get; set; }

public int StartingTime { get; set; }
```



### **Editing controls**

Now we can assign editing controls to our properties.

Since the **`Service`** property should be picking from a finite list of options, let's make it use radio buttons, and have it default to the Youtube option, since this is the most popular video sharing service. We can assign a plain textbox to the Url property.
```c#
//For using old C# components

[EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.service$}", Order = 1)]
[EditingComponentProperty(nameof(RadioButtonsComponent.Properties.DataSource), YOUTUBE + ";YouTube\r\n" + VIMEO + ";Vimeo\r\n" + DAILYMOTION + ";Dailymotion\r\n" + FILE + ";File URL\r\n")]
public string Service { get; set; } = YOUTUBE;

[EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.url$}", Order = 2)]
public string Url { get; set; }
```

```c#
//For using React components

[RadioGroupComponent(Label = "{$videoembedwidget.properties.service$}", Inline = true, Order = 1, Options = YOUTUBE + ";YouTube\r\n" + VIMEO + ";Vimeo\r\n" + DAILYMOTION + ";Dailymotion\r\n" + FILE + ";File URL\r\n")]
public string Service { get; set; } = YOUTUBE;

[TextInputComponent(Label = "{$videoembedwidget.properties.url$}", Order = 2)]
public string Url { get; set; }
```
We can use checkbox components for the boolean properties **`DynamicSize`** and **`PlayFromBeginning`**, and number or integer components for **`Width`**, **`Height`**, and **`StartingTime`**. 

Set the default starting time to 0, since we don't know how long the provided videos will be, and choose default dimensions that seem appropriate. Below are the default dimensions Youtube seems to use when generating embeds for videos with the standard aspect ratio.
```c#
//For using old C# components

[EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.dynamicsize$}", Order = 3)]
public bool DynamicSize { get; set; } = true;

[EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.width$}", Order = 4)]
public int Width { get; set; } = 560;

[EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.height$}", Order = 5)]
public int Height { get; set; } = 315;

[EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.playfrombeginning$}", Order = 6)]
public bool PlayFromBeginning { get; set; } = true;

[EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.startingtime$}", Order = 7)]
public int StartingTime { get; set; } = 0;
```
```c#
//For using React components

[CheckBoxComponent(Label = "{$videoembedwidget.properties.dynamicsize$}", Order = 3)]
public bool DynamicSize { get; set; } = true;

[NumberInputComponent(Label = "{$videoembedwidget.properties.width$}", Order = 4)]
public int Width { get; set; } = 560;

[NumberInputComponent(Label = "{$videoembedwidget.properties.height$}", Order = 5)]
public int Height { get; set; } = 315;

[CheckBoxComponent(Label = "{$videoembedwidget.properties.playfrombeginning$}", Order = 6)]
public bool PlayFromBeginning { get; set; } = true;

[NumberInputComponent(Label = "{$videoembedwidget.properties.startingtime$}", Order = 7)]
public int StartingTime { get; set; } = 0;
```


### **Standard visibility conditions**

Now we can add visibility conditions to these properties.

In my testing, Youtube embeds seem to get a bit wonky when trying to size them dynamically (via percentage-based CSS). There may be some way around this with CSS wizardry, but that's not my strong suit, so let's hide the *DynamicSize* property when Youtube is the selected service.
```c#
//For using old C# components

[EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.dynamicsize$}", Order = 3)]
[VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, YOUTUBE)]
public bool DynamicSize { get; set; } = true;
```
```c#
//For using React components

[CheckBoxComponent(Label = "{$videoembedwidget.properties.dynamicsize$}", Order = 3)]
[VisibleIfNotEqualTo(nameof(Service), YOUTUBE)]
public bool DynamicSize { get; set; } = true;
```

Similarly, Dailymotion does not allow embeds to start at a specific timestamp, so we can hide the **`PlayFromBeginning`** checkbox when it is the selected service.
```c#
//For using old C# components

[EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.playfrombeginning$}", Order = 6)]
[VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, DAILYMOTION)]
public bool PlayFromBeginning { get; set; } = true;
```
```c#
//For using React components

[CheckBoxComponent(Label = "{$videoembedwidget.properties.playfrombeginning$}", Order = 6)]
[VisibleIfNotEqualTo(nameof(Service), DAILYMOTION)]
public bool PlayFromBeginning { get; set; } = true;
```

Next, let's determine the visibility of the **`StartingTime`** numeric input. We want this to hidden when **`PlayFromBeginning`** is true, as well as when *Dailymotion* is selected, regardless of the value of **`PlayFromBeginning`**. Thankfully, these conditions can be applied by stacking multiple visibility conditions. The **`StartingTime`** property will only be visible when both conditions are met.
```c#
//For using old C# components

[EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.startingtime$}", Order = 7)]
[VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, DAILYMOTION)]
[VisibilityCondition(nameof(PlayFromBeginning), ComparisonTypeEnum.IsFalse)]
public int StartingTime { get; set; } = 0;
```
```c#
//For using React components

[NumberInputComponent(Label = "{$videoembedwidget.properties.startingtime$}", Order = 7)]
[VisibleIfFalse(nameof(PlayFromBeginning))]
[VisibleIfNotEqualTo(nameof(Service), DAILYMOTION)]
public int StartingTime { get; set; } = 0;
```






## Setting up the widget display

Now let's add a viewmodel and view for our widget.

### Viewmodel

Create a class called **`VideoEmbedWidgetViewModel`** in the **`DancingGoat.Widgets`** namespace.

```c#
namespace DancingGoat.Widgets
{
    public class VideoEmbedWidgetViewModel
    {
        //...
    }
}
```

Add a string property called **`Markup`**, which we can use to store the Html that will be rendered for the widget. Due to the variety of video platforms, and potential configuration within each one, a lot of conditional logic will go into assembling the markup for the embed, so we should evaluate this logic in the view component rather than the view.
```c#
public string Markup { get; set; }
```

For the purposes of this example, we won't add any other properties. We could optionally pass a string as a model instead, but in order to be consistent with other Dancing Goat widgets and facilitate further customization.

### View

Now let's add the view. 
Create a new view file in the same folder called **_VideoEmbedWidget.cshtml**. 

This aligns with the conventions set by the other widgets in the Dancing Goat project, such as HeroImageWidget and ProductCardWidget, though other projects may have their own conventions. The system automatically checks the **~/Views/Shared/Widgets** for a view named **_{your widget identifier}.cshtml** when no view is specified in the widget registration or the view component's result.

For the purposes of this example, we'll keep it simple, and simply render the markup passed in the viewmodel.

```
@model DancingGoat.Widgets.VideoEmbedWidgetViewModel
@Html.Raw(Model.Markup)
```






## Creating the ViewComponent

Now we can create a ViewComponent class that holds most of the widget's logic.
Add a new file to your **VideoEmbedWidget** folder called **VideoEmbedWidgetViewComponent.cs**.

Give this class the following using directives.

```c#
using Kentico.PageBuilder.Web.Mvc;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;
using DancingGoat.Widgets;
```

Use the **`DancingGoat.Widgets`** namespace, and make the class inherit form **`ViewComponent`**.

```c#
namespace DancingGoat.Widgets
{
    public class VideoEmbedWidgetViewComponent : ViewComponent
    {
        //...
    }
}
```

Add a constant for the widget's identifier. This is the value that is used to connect the widget configurations saved for each instance of your widget in the database with the code that is used to render it.
```c#
public const string IDENTIFIER = "DancingGoat.VideoEmbedWidget";
```

Then add the **`RegisterWidget`** assembly attribute to this class, passing each of the following parameters.
1. The identifier constant.
1. The type of the widget's view component.
1. A localization macro for the display name of the widget, which is rendered in the page builder.
1. The type of the widget's properties.
1. A localization macro for the description of the widget, which is rendered in the page builder
1. The CSS class of the icon which should visually represent the widget in the listing in page builder. Let's use the right-facing triangle icon, which resembles a "play" button.

```c#
[assembly: RegisterWidget(VideoEmbedWidgetViewComponent.IDENTIFIER, typeof(VideoEmbedWidgetViewComponent), "{$videoembedwidget.name$}", typeof(VideoEmbedWidgetProperties), Description = "{$videoembedwidget.description$}", IconClass = "icon-triangle-right")]
```

The path to the widget's view can also be included in the widget registration, but in keeping with this project's conventions, we will forgo this option and handle it elsewhere.

Add a private property for an **`ILocalizationService`**, and populate it through dependency injection in the constructor.

```c#
private readonly ILocalizationService localizationService;

public VideoEmbedWidgetViewComponent(ILocalizationService localizationService)
{
    this.localizationService = localizationService;
}
```
There is a built-in implementation of this service that will resolve resource strings registered with the system. (We will set up localization of resource strings later in this article.)

With this setup taken care of, we can look into the meat of the component.

The primary method of a view component is Invoke, of the type **`IViewComponentResult`**. For a parameter, it takes a **`ComponentViewModel`** with a generic type parameter to hold the type of its properties. This can be set to the **`IWidgetProperties`** implementation defined earlier, **`VideoEmbedWidgetProperties`**.

```c#
public IViewComponentResult Invoke(ComponentViewModel<VideoEmbedWidgetProperties> widgetProperties)
{
    //...
}
```
In keeping with the conventions of the other Dancing Goat widgets, this method should return a view, the path to which is passed directly to the **`View()`** method rather than through the widget registration attribute.

We'll pass an instance of the **`VideoEmbedWidgetViewModel`** class that we created earlier as the model for this view. This view model has a **`string`** property called **`Markup`**, which holds the HTML markup of the video embed. We should add a method to supply this markup, which uses the supplied **`VideoEmbedWidgetProperties`** to generate the proper markup.

Let's call it **`GetEmbedMarkup`**, and add a call in the Invoke method.

```c#
public IViewComponentResult Invoke(ComponentViewModel<VideoEmbedWidgetProperties> widgetProperties)
{
    string markup = GetEmbedMarkup(widgetProperties.Properties);
    return View("~/Components/Widgets/VideoEmbedWidget/_VideoEmbedWidget.cshtml", new VideoEmbedWidgetViewModel { Markup = markup });
}
```

Now, with this example of how it should work, let's implement this method.
It needs to take a **`VideoEmbedWidgetProperties`** object as a parameter.

```c#
private string GetEmbedMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    //...             
}
```
Since all of this functionality hinges on embedding a video, let's first make sure the video is not null, and return a message if it is missing. Since this text will be returned as markup, it will be rendered to the page builder interface, where the editor can read it and react accordingly.
```c#
if(widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
{
    //...
}
return localizationService.GetString("videoembedwidget.message.nourl");   
```
Now we can check which video service is selected, and create HTML markup accordingly. Let's use a **switch expression** to return a different method for each video service. We can use calls to methods that don't exist yet, and then go through an implement them one-at-a-time.

Check the value of **`widgetProperties.Service`** against the various constants defined in the properties, and return a message if there are no matches.

```c#
return widgetProperties.Service switch
{
    VideoEmbedWidgetProperties.YOUTUBE => GetYoutubeMarkup(widgetProperties),
    VideoEmbedWidgetProperties.VIMEO => GetVimeoMarkup(widgetProperties),
    VideoEmbedWidgetProperties.DAILYMOTION => GetDailyMotionMarkup(widgetProperties),
    VideoEmbedWidgetProperties.FILE => GetFileMarkup(widgetProperties),
    _ => localizationService.GetString("videoembedwidget.message.servicenotfound"),
};
```
The resulting GetEmbedMarkup file should look like this.
```c#
private string GetEmbedMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    if(widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
    {
        return widgetProperties.Service switch
        {
            VideoEmbedWidgetProperties.YOUTUBE => GetYoutubeMarkup(widgetProperties),
            VideoEmbedWidgetProperties.VIMEO => GetVimeoMarkup(widgetProperties),
            VideoEmbedWidgetProperties.DAILYMOTION => GetDailyMotionMarkup(widgetProperties),
            VideoEmbedWidgetProperties.FILE => GetFileMarkup(widgetProperties),
            _ => localizationService.GetString("videoembedwidget.message.servicenotfound"),
        };
    }
    return localizationService.GetString("videoembedwidget.message.nourl");                
}
```
Let's start out with the Youtube-specific method first.
Create a **`string`** method called **`GetYoutubeMarkup`**, which takes a **`VideoEmbedWidgetProperties`** object as a parameter.

```c#
private string GetYoutubeMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    //...
}
```
Start out by validating the parameters, like in the GetEmbedMarkup method. The code should not reach this point if the properties or URL is null, but it's a good practice to validate nullable parameters anyway, in case there are future changes to how the method is called.
```c#
if (widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
{
    //...
}
return localizationService.GetString("videoembedwidget.message.nourl");
```
 let's add a call to a method that extracts the Youtube ID of the video from the URL, which we will implement next.

```c#
string videoId = GetYoutubeId(widgetProperties.Url);
```

Then, validate this video ID and assemble the markup if it is not empty.

Based on whether or not the video is set to play from the beginning, we can make a query string using the **`start`** parameter.

Then, we can drop all of our pieces into an **`iframe`**, which is configured to mirror the markup provided by the embed sharing option on Youtube itself.
```c#
if (!string.IsNullOrEmpty(videoId))
{
    string query = widgetProperties.PlayFromBeginning ? string.Empty : $"?start={widgetProperties.StartingTime}";
    return $"<iframe width=\"{widgetProperties.Width}\" height=\"{widgetProperties.Height}\" src=\"https://www.youtube.com/embed/{videoId}{query}\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer;autoplay;clipboard-write;encrypted-media;gyroscope;picture-in-picture;web-share\" allowfullscreen></iframe>";
}
return localizationService.GetString("videoembedwidget.message.noyoutubeid");
```
The ending result will look like this:
```c#
private string GetYoutubeMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    if (widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
    {
        string videoId = GetYoutubeId(widgetProperties.Url);
        if (!string.IsNullOrEmpty(videoId))
        {
            string query = widgetProperties.PlayFromBeginning ? string.Empty : $"?start={widgetProperties.StartingTime}";
            return $"<iframe width=\"{widgetProperties.Width}\" height=\"{widgetProperties.Height}\" src=\"https://www.youtube.com/embed/{videoId}{query}\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer;autoplay;clipboard-write;encrypted-media;gyroscope;picture-in-picture;web-share\" allowfullscreen></iframe>";
        }
        return localizationService.GetString("videoembedwidget.message.noyoutubeid");
    }
    return localizationService.GetString("videoembedwidget.message.nourl");
}
```
Moving on to the **`GetYoutubeId`** method mentioned above, it should be of the type **`string`**, and take a **`string`** parameter for the URL.
```c#
private string GetYoutubeId(string url)
{
    //...
}
```
Here, we have to find the video ID based on the provided Youtube URL.
However, we may encounter multiple formats of Youtube URL, as we don't know where the editor using the widget is going to be copy/pasting from. For example, all of the following URLs have the same video ID, **`dQw4w9WgXcQ`**.

- https://www.youtube.com/watch?v=dQw4w9WgXcQ
- http://www.youtube.com/watch?feature=player_embedded&v=dQw4w9WgXcQ
- https://youtu.be/dQw4w9WgXcQ
- http://m.youtube.com/v/dQw4w9WgXcQ
- https://youtube.com/v/dQw4w9WgXcQ?feature=youtube_gdata_player

It seems that if the video URL is passed through the query string, it will reliably be sent as the **`v`** parameter. Otherwise, it will be the final part of the path, prior to a possible query string.

To account for this, let's make two more methods-- one to pull the video ID from the query parameter, and another to get it from the end of the path. We can call the first, then if it returns no result, call the second.

```c#
if (!string.IsNullOrEmpty(url))
{
    string queryId = GetIdFromQuery(url, "v");
    return string.IsNullOrEmpty(queryId) ? GetFinalPathComponent(url) : queryId;
}
return string.Empty;
```
**`GetIdFromQuery`** should take two **`string`** parameters-- one for the Url, and one for the name of the query parameter.
```c#
private string GetIdFromQuery(string url, string paramName)
{
    //...
}
```
Validate the parameters, and construct a **`Uri`** from the URL if they are not empty. This object has a property called **`Query`**, which can be parsed by the **`HttpUtility`** class if it is not null.

```c#
if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(paramName))
{
    Uri uri = new Uri(url);
    if (!string.IsNullOrEmpty(uri.Query))
    {
        var query = HttpUtility.ParseQueryString(uri.Query);
        //...
    }
}
return string.Empty;
```
Finally, we can validate this parsed collection of values and extract the necessary parameter.
```c#
if (query != null)
{
    return query.Get(paramName);
}
```
Altogether, the method should look like this.
```c#
private string GetIdFromQuery(string url, string paramName)
{
    if (!string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(paramName))
    {
        Uri uri = new Uri(url);
        if (!string.IsNullOrEmpty(uri.Query))
        {
            var query = HttpUtility.ParseQueryString(uri.Query);
            if (query != null)
            {
                return query.Get(paramName);
            }
        }
    }
    return string.Empty;
}
```
The other method for finding the ID from the path only needs one parameter, the URL.
```c#
private string GetFinalPathComponent(string url)
{
    //...
}
```
After validating the URL, use the **`string.Split()`** method to isolate everything that comes before the query string, if there is one.
```c#
if(!string.IsNullOrEmpty(url))
{
    string baseUrl = url.Split('?')[0];
    //...
}
return string.Empty;
```
Then, you can split the string again, this time on the **`'/'`** character. If the resulting array has more than 3 elements, it means that the URL contains more than just the protocol and domain, so you can return the final element of this array.
```c#
var urlComponents = baseUrl.Split('/');

if(urlComponents.Length > 3)
{
    return urlComponents[urlComponents.Length - 1];
}
```
The resulting method should look like this, once all is finished.
```c#
private string GetFinalPathComponent(string url)
{
    if(!string.IsNullOrEmpty(url))
    {
        string baseUrl = url.Split('?')[0];
        var urlComponents = baseUrl.Split('/');
        
        if(urlComponents.Length > 3)
        {
            return urlComponents[urlComponents.Length - 1];
        }
    }
    return string.Empty;
}
```
That should wrap up the Youtube method, so now we can move on to the next.

 **`GetVimeoMarkup`** will be similar overall, but with the addition of dynamic sizing, and no need to look for the video ID in the query string.

 Start out by validating the properties and the URL, before using the **`GetFinalPathComponent`** method to retrieve the Vimeo ID. Validate the retrieved ID as well.
 ```c#
private string GetVimeoMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    if (widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
    {
        var videoId = GetFinalPathComponent(widgetProperties.Url);
        if (!string.IsNullOrEmpty(videoId))
        {
            //...
        }
        return localizationService.GetString("videoembedwidget.message.novimeoid");
    }
    return localizationService.GetString("videoembedwidget.message.nourl");
}

```
Vimeo's embeds use an anchor rather than a query string parameter to specify what time the video should start at, but the process of getting the anchor should be nearly identical to that of the query string in the Youtube example.
 ```c#
string anchor = widgetProperties.PlayFromBeginning ? string.Empty : $"#t={widgetProperties.StartingTime}s";
```
Next, we want to render different markup depending on whether the video should be sized dynamically, or with explicit width and height. 

The share embed functionality on Vimeo nests the **`iframe`** within a **`div`** with certain styles, and provides a script for adapting to the size responsively. The markup for the static sized version looks somewhat similar to Youtube.
```c#
if(widgetProperties.DynamicSize)
{
    
    return "<div style=\"padding: 56.25 % 0 0 0; position: relative;\"><iframe src=\"https://player.vimeo.com/video/{videoId}{anchor}\" style=\"position:absolute;top:0;left:0;width:100%;height:100%;\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen></iframe></div><script src=\"https://player.vimeo.com/api/player.js\"></script>";
}
else
{
    return $"<iframe src=\"https://player.vimeo.com/video/{videoId}{anchor}\" width=\"{widgetProperties.Width}\" height=\"{widgetProperties.Height}\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen ></iframe >";
}
```
The completed **`GetVimeoMarkup`** method should look like this.
```c#
private string GetVimeoMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    if (widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
    {
        var videoId = GetFinalPathComponent(widgetProperties.Url);
        if (!string.IsNullOrEmpty(videoId))
        {
            string anchor = widgetProperties.PlayFromBeginning ? string.Empty : $"#t={widgetProperties.StartingTime}s";
            if(widgetProperties.DynamicSize)
            {
                
                return "<div style=\"padding: 56.25 % 0 0 0; position: relative;\"><iframe src=\"https://player.vimeo.com/video/{videoId}{anchor}\" style=\"position:absolute;top:0;left:0;width:100%;height:100%;\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen></iframe></div><script src=\"https://player.vimeo.com/api/player.js\"></script>";
            }
            else
            {
                return $"<iframe src=\"https://player.vimeo.com/video/{videoId}{anchor}\" width=\"{widgetProperties.Width}\" height=\"{widgetProperties.Height}\" frameborder=\"0\" allow=\"autoplay; fullscreen; picture-in-picture\" allowfullscreen ></iframe >";
            }
        }
        return localizationService.GetString("videoembedwidget.message.novimeoid");
    }
    return localizationService.GetString("videoembedwidget.message.nourl");
}
```
Continuing down the switch statement in **`GetEmbedMarkup`**, we have GetDailyMotionMarkup next, which is nearly identical to the Vimeo method, except that it has no functionality for starting the video partway through or special script. At this point, I think you should be able to make sense of it all.

```c#
        private string GetDailyMotionMarkup(VideoEmbedWidgetProperties widgetProperties)
        {
            if (widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
            {
                var videoId = GetFinalPathComponent(widgetProperties.Url);
                if (!string.IsNullOrEmpty(videoId))
                {
                    if (widgetProperties.DynamicSize)
                    {
                        return $"<div style=\"position:relative;padding-bottom:56.25%;height:0;overflow:hidden;\"> <iframe style=\"width:100%;height:100%;position:absolute;left:0px;top:0px;overflow:hidden\" frameborder=\"0\" type=\"text/html\" src=\"https://www.dailymotion.com/embed/video/{videoId}\" width=\"100%\" height=\"100%\" allowfullscreen title=\"Dailymotion Video Player\" allow=\"autoplay\"></iframe></div>";
                    }
                    else
                    {
                        return $"<iframe src=\"https://www.dailymotion.com/embed/video/{videoId}\" width=\"{widgetProperties.Width}\" height=\"{widgetProperties.Height}\" frameborder=\"0\" type=\"text/html\" allowfullscreen title=\"Dailymotion Video Player\"></iframe>";
                    }
                }
                return localizationService.GetString("videoembedwidget.message.nodailymotionid");
            }
            return localizationService.GetString("videoembedwidget.message.nourl");
        }
```
The last of the Markup methods is **`GetFileMarkup`**. It will start out similarly to the others.
```c#
private string GetFileMarkup(VideoEmbedWidgetProperties widgetProperties)
{
    if (widgetProperties != null && !string.IsNullOrEmpty(widgetProperties.Url))
    {
        //...
    }
    return localizationService.GetString("videoembedwidget.message.nourl");
}
```
However, this final method introduces a new requirement- The **`<video>`** tag in Html utilizes an attribute called **`type`** which is typically set to values such as **`"video/mp4"`** or **`"video/ogg"`**. In order to populate this attribute, we'll need to find the file extension of the provided video.

Let's add a call to a new method, **`GetFileExtension`**, and validate its result.

```c#
string extension = GetFileExtension(widgetProperties.Url);
if (!string.IsNullOrEmpty(extension))
{
    //...
}
return localizationService.GetString("videoembedwidget.message.nofileextension");
```
The video tag supports starting times set through an anchor tag on the URL, similar to Vimeo.
```c#
string anchor = widgetProperties.PlayFromBeginning ? string.Empty : $"#t={widgetProperties.StartingTime}";
```
Lastly, we can return a different video tag that uses either the **`style`** or **`width`**/**`height`** attributes to set its size, depending on whether the properties indicate that it should be sized dynamically.

```c#
if (widgetProperties.DynamicSize)
{
    return $"<video style=\"width:100%;\" controls><source src=\"{widgetProperties.Url}{anchor}\" type=\"video/{extension}\"></video>";
}
else
{
    return $"<video width=\"{widgetProperties.Width}\" height=\"{widgetProperties.Height}\" controls><source src=\"{widgetProperties.Url}{anchor}\" type=\"video/{extension}\"></video>";
}
```

Now all that's left is to implement the GetFileExtension method. This method should take a URL like `https://www.mywebsite.com/files/videofile.mp4` and isolate the **`.mp4`** at the end.

Take the URL as a parameter, and set the return type to **`string`**.
```c#
private string GetFileExtension(string url)
{
    //...
}
```
After validating the URL, use the **`GetFinalPathComponent`** method from earlier to get what comes after the last slash in the path.
```c#
if (!string.IsNullOrEmpty(url))
{
    string finalComponent = GetFinalPathComponent(url);
    //...
}
return string.Empty;
```
Split this string on the **`'.'`** character, and if the resulting array has more than one element, return the final one as the file extension.
```c#
string[] parts = finalComponent.Split('.');
if (parts.Length > 1)
{
    return parts[parts.Length - 1];
}
```
The resulting method should look like this.
```c#
private string GetFileExtension(string url)
{
    if (!string.IsNullOrEmpty(url))
    {
        string finalComponent = GetFinalPathComponent(url);
        string[] parts = finalComponent.Split('.');
        if (parts.Length > 1)
        {
            return parts[parts.Length - 1];
        }
    }
    return string.Empty;
}
```






## Handling more advanced visibility scenarios

You may have noticed that the properties that specify explicit dimensions for the video are still always visible. This is because there's a bit more complicated of logic to determine whether they should be displayed or not.

They should always be displayed when the selected service is Youtube, but only when the checkbox for dynamic size is not enabled for any other service.
Logically, it should look something like this:

`(Service is Youtube) OR (DynamicSize is disabled)`

or alternatively,

`NOT((Service is not youtube) AND (Dynamic size is enabled))`

With stacked visibility conditions, the field will only display when both of them are true. And since there's no way to negate the entire combination, we have to find an alternate way to evaluate a more complex boolean condition.

A [custom visibility condition](https://docs.xperience.io/xp/developers-and-admins/customization/extend-the-administration-interface/ui-form-components/ui-form-component-visibility-conditions#UIformcomponentvisibilityconditions-Visibilityconditionswithfielddependencies) also does not quite cut it for this scenario. While they allow for more complex logic, they can only access the value of the property to which they are applied, and a single other property.

A complicating factor to adding this logic is that visibility conditions must depend on properties that are rendered in the properties form. We can't rely on the value of a **`get`** accessor for a property that does not have an editing component, or which is currently hidden.

However, if we can get a field that technically renders without actually displaying anything, we can use it's **`get`** accessor however we please, without worrying about its value becoming inaccessible due to a visibility condition change.

So in order to accomplish this end, let's create an invisible form component.






### **Invisible component in C#**

Under the **~/Components** folder in the solution, add a folder called **FormComponents**, then a folder called **InvisibleComponent** within it. This will be the directory for our invisible component. 

Create a new class called **`InvisibleProperties`** in the **`DancingGoat.FormComponents`** namespace, extending **`FormComponentProperties<bool>`**. You'll need using directives for **`CMS.DataEngine`** and **`Kentico.Forms.Web.Mvc`**.
```c#
using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;

namespace DancingGoat.FormComponents
{
    public class InvisibleProperties : FormComponentProperties<bool>
    {
        //...
    }
}
```
Passing a **`bool`** to the generic type of **`FormComponentProperties`** allows this editing component to apply to be used on boolean properties.

Next, call the constructor of the base class, specifying the Boolean data type for the field.
```c#
public InvisibleProperties() : base(FieldDataType.Boolean)
{
}
```
Override the **`DefaultValue`** and **`Label`** properties, adjusting the latter so that its **`get`** accessor always returns an empty string.
```c#
public override bool DefaultValue { get; set; }

public override string Label { get => string.Empty; set => base.Label = value; }
```
The finished properties class should look like this.
```c#
using CMS.DataEngine;
using Kentico.Forms.Web.Mvc;

namespace DancingGoat.FormComponents
{
    public class InvisibleProperties : FormComponentProperties<bool>
    {
        public InvisibleProperties() : base(FieldDataType.Boolean)
        {
        }


        public override bool DefaultValue { get; set; }


        public override string Label { get => string.Empty; set => base.Label = value; }
    }
}
```

Next, add a completely blank file called **`_Invisible.cshtml`** to the same folder.

Lastly, add a final file called **InvisibleFormComponent.cs**. Give it the following **`using`** directives.
```
using Kentico.Forms.Web.Mvc;
using DancingGoat.FormComponents;
```

Use the **`namespace DancingGoat.FormComponents`** and have the class inherit from **`FormComponent<InvisibleProperties,bool>`**. This ensures that the component is applicable to a boolean property, and utilizes the previously defined properties.
```c#
namespace DancingGoat.FormComponents
{
    public class InvisibleFormComponent : FormComponent<InvisibleProperties,bool>
    {
        //...
    }
}
```
Add constants for the identifier and name of the component.
```c#
public const string IDENTIFIER = "Custom.InvisibleComponent";
public const string NAME = "InvisibleComponent";
```
Using these new constants we can add the **`RegisterFormComponent`** assembly attribute to register the editing component. Place the attribute above the namespace declaration.
```c#
[assembly: RegisterFormComponent(InvisibleFormComponent.IDENTIFIER, typeof(InvisibleFormComponent), InvisibleFormComponent.NAME, ViewName = "~/Components/FormComponents/InvisibleComponent/_Invisible.cshtml", IsAvailableInFormBuilderEditor = false)]
```
Getting back to the class, we can add a property to hold the value, and use it to override the required abstract methods **`GetValue`** and **`SetValue`**. This allows the component to react to changes in the value of its property, enabling the visibility conditions to work properly.
```c#
public bool Value { get; set; }


public override bool GetValue()
{
    return Value;
}


public override void SetValue(bool value)
{
    Value = value;
}
```
The finished component class should look like this.
```c#
using Kentico.Forms.Web.Mvc;
using DancingGoat.FormComponents;

[assembly: RegisterFormComponent(InvisibleFormComponent.IDENTIFIER, typeof(InvisibleFormComponent), InvisibleFormComponent.NAME, ViewName = "~/Components/FormComponents/InvisibleComponent/_Invisible.cshtml", IsAvailableInFormBuilderEditor = false)]
namespace DancingGoat.FormComponents
{
    public class InvisibleFormComponent : FormComponent<InvisibleProperties,bool>
    {
        public const string IDENTIFIER = "Custom.InvisibleComponent";
        public const string NAME = "InvisibleComponent";


        public bool Value { get; set; }


        public override bool GetValue()
        {
            return Value;
        }


        public override void SetValue(bool value)
        {
            Value = value;
        }
    }
}
```





### **Invisible component in React**

#### **Installing and setting up the boilerplate**

To start out in React, download the admin customization boilerplate as described in the [documentation](https://docs.xperience.io/xp/developers-and-admins/customization/extend-the-administration-interface/prepare-your-environment-for-admin-development#Prepareyourenvironmentforadmindevelopment-Clientdevelopmentboilerplate) with the name *DancingGoat.WebAdmin*.

Don't forget to add a reference from your Dancing Goat solution to this new admin project.
```
dotnet add reference <the relative path from your main project's root to your custom admin csproj>
```

Optionally, open the boilerplate project, and delete the **~/UIPages** folder, as well as the **~/Client/src/custom-layout/CustomLayoutTemplate.tsx** file. Then, open **~/Client/src/entry.tsx** and delete the following line:
```tsx
export * from './custom-layout/CustomLayoutTemplate';
```
This will get rid of sample customizations for the UI which are not relevant to this article.

Follow the steps outlined [in the documentation](https://docs.xperience.io/xp/developers-and-admins/customization/extend-the-administration-interface/prepare-your-environment-for-admin-development#Prepareyourenvironmentforadmindevelopment-Renametheboilerplateproject) to rename the organization from **acme** to **dancinggoat**.

#### **Creating the form component**

Next, create a folder called **invisible-form-component** in the **~/Client** directory, and add a file called **InvisibleFormComponent.tsx**

This will be our front-end file for the invisible form component, used by the administration UI. Because the whole point of our form component is to display nothing, it will be even simpler than the [example](https://docs.xperience.io/xp/developers-and-admins/customization/extend-the-administration-interface/ui-form-components#UIformcomponents-Formcomponentfrontend) provided by the documentation. 

Import react and the default form component properties, as in the example, then export **`InvisibleFormComponent`** to return nothing.

```tsx
import React from 'react';
import { FormComponentProps } from '@kentico/xperience-admin-base';

export const InvisibleFormComponent = (props: FormComponentProps) => {
    return;
};
```
Now, switch back to the **entry.tsx** file and export everything from this file.
```tsx
export * from './invisible-form-component/InvisibleFormComponent';
```


Next, we can create the C# files that this control needs.

Add a folder called **FormComponents** to the root of the project, and a folder called **InvisibleComponent** inside of it.

Add a new C# file called **InvisibleClientProperties.cs**. This class represents the properties passed to the administration application when it renders the react component.

Add a **`using`** directive for **`Kentico.Xperience.Admin.Base.Forms;`** and set the namespace to **`DancingGoat.FormComponents`**

```c#
using Kentico.Xperience.Admin.Base.Forms;

namespace DancingGoat.FormComponents
```

Make the class inherit from **`FormComponentClientProperties<bool>`**. This uses the **`bool`** type so that we can assign the component to a boolean property, which will work most easily with visibility conditions.

Since the component doesn't display anything, we can leave the class empty, with a final result like this.
```c#
using Kentico.Xperience.Admin.Base.Forms;

namespace DancingGoat.FormComponents
{
    public class InvisibleClientProperties : FormComponentClientProperties<bool>
    {
    }
}
```

Next, add a similar class called **InvisibleProperties.cs**, which represents the configuration of the component. Again, since this component renders nothing, the class can be empty.
```c#
using Kentico.Xperience.Admin.Base.Forms;

namespace DancingGoat.FormComponents
{
    public class InvisibleProperties : FormComponentProperties
    {
    }
}
```

Next, let's add an attribute class, which will allow us to use an attribute to assign the component to our widget property. This class doesn't need to contain anything-- it will only be used to map any widget properties that use it to the proper form component class.

Use the same namespace as the previous two files, and have it inherit from the **`FormComponentAttribute`** class.

```C#
using Kentico.Xperience.Admin.Base.FormAnnotations;
namespace DancingGoat.FormComponents
{
    public class InvisibleComponentAttribute : FormComponentAttribute
    {
    }
}
```
Finally, we can tie all of these together with the component class. Create a new file, **InvisibleFormComponent.cs**. 

Add **`using`** directives for **`DancingGoat.FormComponents`** and **`Kentico.Xperience.Admin.Base.Forms`**, and place the class in the **`DancingGoat.FormComponents`** namespace.

```c#
using DancingGoat.FormComponents;
using Kentico.Xperience.Admin.Base.Forms;

namespace DancingGoat.FormComponents
{
    //...
}
```

Make the **`InvisibleFormComponent`** class inherit from **`FormComponent<InvisibleProperties,InvisibleClientProperties,bool>`**. This connects the class with the properties and client properties, and specifies that it should be used on a boolean property.

Define **`IDENTIFIER`** and **`NAME`** constants for the class, and point the **`ClientComponentName`** property to the front-end we defined previously. (Note that the app will automatically add "FormComponent" to the end of the name passed here.) 

```c#
public const string IDENTIFIER = "Custom.InvisibleComponent";
public const string NAME = "InvisibleComponent";

public override string ClientComponentName => "@dancinggoat/web-admin/Invisible";;
```

Next, use the **`ComponentAttributeAttribute`** to map **`InvisibleComponentAttribute`** to this class.
```c#
[ComponentAttribute(typeof(InvisibleComponentAttribute))]
```

Finally, use the RegisterFormComponent assembly attribute to register the form component.
```c#
[assembly: RegisterFormComponent(InvisibleFormComponent.IDENTIFIER,typeof(InvisibleFormComponent), InvisibleFormComponent.NAME)]
```
Altogether, the class should look like this
```c#
using DancingGoat.FormComponents;
using Kentico.Xperience.Admin.Base.Forms;

[assembly: RegisterFormComponent(InvisibleFormComponent.IDENTIFIER,typeof(InvisibleFormComponent), InvisibleFormComponent.NAME)]

namespace DancingGoat.FormComponents
{
    [ComponentAttribute(typeof(InvisibleComponentAttribute))]
    public class InvisibleFormComponent : FormComponent<InvisibleProperties,InvisibleClientProperties,bool>
    {
        public const string IDENTIFIER = "Custom.InvisibleComponent";
        public const string NAME = "InvisibleComponent";

        public override string ClientComponentName => "@dancinggoat/web-admin/Invisible";;
    }
}
```
#### **Building the project**

Now build the C# portion of the project through visual studio, and build the **~/Client** app through the command line with the command **`npm run build`**. Thanks to the boilerplate's use of [Babel](https://babeljs.io/docs/), this will automatically transpile our typescript files to work in browsers.

Depending on whether your future changes involve client files, C# files, or both, you will need to determine which of these build options to use.

Then, go back to the Dancing Goat Xperience project and **clean** and **build** the solution. 






### **Using the editing component on properties**

With this form component in place, we can use it in the properties of our widget. Return to the **`VideoEmbedWidgetProperties.cs`** file.

Let's create a new public **`bool`** property called **`ShowDimensions`**. Since we're going to be putting custom functionality in the accessors, it will need an associated private variable. Create a private **`bool`** variable called **`_showDimensions`**.
```c#
private bool _showDimensions;

public bool ShowDimensions
{
    get;
    set;
}
```
Now we can update the **get** accessor to return true when the dimensions should be displayed. 

As discussed earlier, the dimensions should display when the service is youtube, or when DynamicSize is disabled, so the the following boolean expression **`Service == YOUTUBE || !DynamicSize;`** should be sufficient for this case, though the logic here can be as complex as necessary. 

Add the boolean expression to the getter through an expression body definition.
```c#
get => Service == YOUTUBE || !DynamicSize;
```
Now that we've set the **`get`** accessor in this way, the compiler will expect the same to be done for the **`set`** accessor. We can simply set the private variable to the provided value, even though this value will never be used.
```c#
set => _showDimensions = value;
```
Now we can assign our invisible component to the property. If you made the React component, use the **`InvisibleComponentAttribute`** that we created earlier. If you made the C# component instead, use the typical **`EditingComponentAttribute`**. 

In either case, Set the order to 0 so that it comes before the fields that depend on it.
```c#
//For using old C# components

[EditingComponent(InvisibleFormComponent.IDENTIFIER, Order = 0)]
```
```c#
//For using React components

[InvisibleComponent(Order = 0)]
```
Now that this special property is in place, we can add visibility conditions depending on it to the **`Width`** and **`Height`** properties.

```c#
//For using old C# components

[EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.width$}", Order = 4)]
[VisibilityCondition(nameof(ShowDimensions), ComparisonTypeEnum.IsTrue)]
public int Width { get; set; } = 560;

[EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.height$}", Order = 5)]
[VisibilityCondition(nameof(ShowDimensions), ComparisonTypeEnum.IsTrue)]
public int Height { get; set; } = 315;
```
```c#
//For using React components

[NumberInputComponent(Label = "{$videoembedwidget.properties.width$}", Order = 4)]
[VisibleIfTrue(nameof(ShowDimensions))]
public int Width { get; set; } = 560;

[NumberInputComponent(Label = "{$videoembedwidget.properties.height$}", Order = 5)]
[VisibleIfTrue(nameof(ShowDimensions))]
public int Height { get; set; } = 315;
```


If you run the site now, you may notice a problem-- toggling the checkbox for the **`DynamicSize`** property in the UI does not seem to make a difference. Currently, the dialog is not listening to changes to this control, because no other properties reference DynamicSize through a normal visibility condition.

Let's use the invisible form component on another property, and make it depend on the **`DynamicSize`** property, to ensure that the value of **`ShowDimensions`** is re-evaluated when it changes.

We can call it DummyProperty, and set its order to a very high number so that it logically comes after any properties it depends on.
```c#
//For using old C# components

[EditingComponent(InvisibleFormComponent.IDENTIFIER, Order = 999)]
[VisibilityCondition(nameof(DynamicSize), ComparisonTypeEnum.IsTrue)]
public bool DummyProperty { get; set; }
```
```c#
//For using React components

[InvisibleComponent(Order = 999)]
[VisibleIfFalse(nameof(DynamicSize))]
public bool DummyProperty { get; set; }
```
Now, **`ShowDimensions`** should be evaluated, and thus show and hide the **`Width`** and **`Height`** properties, whenever the **`DynamicSize`** checkbox value changes.

In the end, with added summary comments, the properties class should look like this.
```c#
//For using old C# components

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
        [EditingComponent(RadioButtonsComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.service$}", Order = 1)]
        [EditingComponentProperty(nameof(RadioButtonsComponent.Properties.DataSource), YOUTUBE + ";YouTube\r\n" + VIMEO + ";Vimeo\r\n" + DAILYMOTION + ";Dailymotion\r\n" + FILE + ";File URL\r\n")]
        public string Service { get; set; } = YOUTUBE;


        /// <summary>
        /// Defines the URL of the embedded video.
        /// </summary>
        [EditingComponent(TextInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.url$}", Order = 2)]
        public string Url { get; set; }


        /// <summary>
        /// 
        /// </summary>
        [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.dynamicsize$}", Order = 3)]
        [VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, YOUTUBE)]
        public bool DynamicSize { get; set; } = true;


        /// <summary>
        /// Determines the width of the embed.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.width$}", Order = 4)]
        [VisibilityCondition(nameof(ShowDimensions), ComparisonTypeEnum.IsTrue)]
        public int Width { get; set; } = 560;


        /// <summary>
        /// Determines the height of the embed.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.height$}", Order = 5)]
        [VisibilityCondition(nameof(ShowDimensions), ComparisonTypeEnum.IsTrue)]
        public int Height { get; set; } = 315;


        /// <summary>
        /// Defines the time to start the player at.
        /// </summary>
        [EditingComponent(CheckBoxComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.playfrombeginning$}", Order = 6)]
        [VisibilityCondition(nameof(Service), ComparisonTypeEnum.IsNotEqualTo, DAILYMOTION)]
        public bool PlayFromBeginning { get; set; } = true;


        /// <summary>
        /// Determines whether the video will start at the beginning, or at a specified timestamp.
        /// </summary>
        [EditingComponent(IntInputComponent.IDENTIFIER, Label = "{$videoembedwidget.properties.startingtime$}", Order = 7)]
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

```
```c#
//For using React components

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
        /// Holds a complex boolean expression used in determining other fields' visibility.
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
        [RadioGroupComponent(Label = "{$videoembedwidget.properties.service$}", Inline = true, Order = 1, Options = YOUTUBE + ";YouTube\r\n" + VIMEO + ";Vimeo\r\n" + DAILYMOTION + ";Dailymotion\r\n" + FILE + ";File URL\r\n")]
        public string Service { get; set; } = YOUTUBE;
        
        
        /// <summary>
        /// Defines the URL of the embedded video.
        /// </summary>
        [TextInputComponent(Label = "{$videoembedwidget.properties.url$}", Order = 2)]
        public string Url { get; set; }


        /// <summary>
        /// Determines whether the video should be sized dynamically or with explicit dimensions.
        /// </summary>
        [CheckBoxComponent(Label = "{$videoembedwidget.properties.dynamicsize$}", Order = 3)]
        [VisibleIfNotEqualTo(nameof(Service), YOUTUBE)]
        public bool DynamicSize { get; set; } = true;


        /// <summary>
        /// Determines the width of the embed.
        /// </summary>
        [NumberInputComponent(Label = "{$videoembedwidget.properties.width$}", Order = 4)]
        [VisibleIfTrue(nameof(ShowDimensions))]
        public int Width { get; set; } = 560;


        /// <summary>
        /// Determines the height of the embed.
        /// </summary>
        [NumberInputComponent(Label = "{$videoembedwidget.properties.height$}", Order = 5)]
        [VisibleIfTrue(nameof(ShowDimensions))]
        public int Height { get; set; } = 315;


        /// <summary>
        /// Defines the time to start the player at.
        /// </summary>        
        [CheckBoxComponent(Label = "{$videoembedwidget.properties.playfrombeginning$}", Order = 6)]
        [VisibleIfNotEqualTo(nameof(Service), DAILYMOTION)]
        public bool PlayFromBeginning { get; set; } = true;


        /// <summary>
        /// Determines whether the video will start at the beginning, or at a specified timestamp.
        /// </summary>
        [NumberInputComponent(Label = "{$videoembedwidget.properties.startingtime$}", Order = 7)]
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
```






## Localizing resource strings

Throughout this article, I'm sure you've noticed the use of several resource strings.

Let's create a resource file for these strings and register it with the system. In the widget folder, add a c# class called **`VideoEmbedWidgetResources`** in the namespace **`DancingGoat.Widgets`**, and make sure it has the following using directives
- using CMS.Localization;
- using CMS.Base;

Add the RegisterLocalizationResource assembly attribute to the class, registering this class with the system culture.
```c#
[assembly: RegisterLocalizationResource(typeof(DancingGoat.Widgets.VideoEmbedWidgetResources), SystemContext.SYSTEM_CULTURE_NAME)]
```
Now, add a .resx file to the same folder, with the same name, **`VideoEmbedWidgetResources.resx`**. This should then be displayed underneath the associated .cs file in Visual Studio's Solution Explorer.

When you're starting out with your own project, it's often easiest to add them through the editing UI that Visual Studio provides. However, since we already have the resource strings prepared, let's just paste them into the code view.

Right click the .resx file in the solution explorer, and choose **Open With...** > **XML (Text) Editor**, or just open the file with a text editor directly from the folder.

Paste in the following.

```xml
<?xml version="1.0" encoding="utf-8"?>
<root>
  <!-- 
    Microsoft ResX Schema 
    
    Version 2.0
    
    The primary goals of this format is to allow a simple XML format 
    that is mostly human readable. The generation and parsing of the 
    various data types are done through the TypeConverter classes 
    associated with the data types.
    
    Example:
    
    ... ado.net/XML headers & schema ...
    <resheader name="resmimetype">text/microsoft-resx</resheader>
    <resheader name="version">2.0</resheader>
    <resheader name="reader">System.Resources.ResXResourceReader, System.Windows.Forms, ...</resheader>
    <resheader name="writer">System.Resources.ResXResourceWriter, System.Windows.Forms, ...</resheader>
    <data name="Name1"><value>this is my long string</value><comment>this is a comment</comment></data>
    <data name="Color1" type="System.Drawing.Color, System.Drawing">Blue</data>
    <data name="Bitmap1" mimetype="application/x-microsoft.net.object.binary.base64">
        <value>[base64 mime encoded serialized .NET Framework object]</value>
    </data>
    <data name="Icon1" type="System.Drawing.Icon, System.Drawing" mimetype="application/x-microsoft.net.object.bytearray.base64">
        <value>[base64 mime encoded string representing a byte array form of the .NET Framework object]</value>
        <comment>This is a comment</comment>
    </data>
                
    There are any number of "resheader" rows that contain simple 
    name/value pairs.
    
    Each data row contains a name, and value. The row also contains a 
    type or mimetype. Type corresponds to a .NET class that support 
    text/value conversion through the TypeConverter architecture. 
    Classes that don't support this are serialized and stored with the 
    mimetype set.
    
    The mimetype is used for serialized objects, and tells the 
    ResXResourceReader how to depersist the object. This is currently not 
    extensible. For a given mimetype the value must be set accordingly:
    
    Note - application/x-microsoft.net.object.binary.base64 is the format 
    that the ResXResourceWriter will generate, however the reader can 
    read any of the formats listed below.
    
    mimetype: application/x-microsoft.net.object.binary.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Binary.BinaryFormatter
            : and then encoded with base64 encoding.
    
    mimetype: application/x-microsoft.net.object.soap.base64
    value   : The object must be serialized with 
            : System.Runtime.Serialization.Formatters.Soap.SoapFormatter
            : and then encoded with base64 encoding.

    mimetype: application/x-microsoft.net.object.bytearray.base64
    value   : The object must be serialized into a byte array 
            : using a System.ComponentModel.TypeConverter
            : and then encoded with base64 encoding.
    -->
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <data name="videoembedwidget.properties.service" xml:space="preserve">
	<value>Video service</value>
  </data>
    <data name="videoembedwidget.properties.url" xml:space="preserve">
	  <value>Url</value>
  </data>
    <data name="videoembedwidget.properties.dynamicsize" xml:space="preserve">
	  <value>Size dynamically</value>
  </data>
    <data name="videoembedwidget.properties.width" xml:space="preserve">
	  <value>Width (px)</value>
  </data>
    <data name="videoembedwidget.properties.height" xml:space="preserve">
	  <value>Height (px)</value>
  </data>
    <data name="videoembedwidget.properties.playfrombeginning" xml:space="preserve">
	  <value>Play from beginning</value>
  </data>
    <data name="videoembedwidget.properties.startingtime" xml:space="preserve">
	  <value>Starting time (seconds)</value>
  </data>
    <data name="videoembedwidget.description" xml:space="preserve">
	  <value>Embeds a video in the page.</value>
  </data>
    <data name="videoembedwidget.name" xml:space="preserve">
	  <value>Video embed</value>
  </data>
    <data name="videoembedwidget.message.servicenotfound" xml:space="preserve">
	  <value>Specified video service not found.</value>
  </data>
    <data name="videoembedwidget.message.nourl" xml:space="preserve">
	  <value>Please make sure the URL property is filled in.</value>
  </data>
    <data name="videoembedwidget.message.nofileextension" xml:space="preserve">
	  <value>Unable to parse file extension from the provided Url.</value>
  </data>
    <data name="videoembedwidget.message.novimeoid" xml:space="preserve">
	  <value>Unable to parse Vimeo video ID from the provided Url.</value>
  </data>
    <data name="videoembedwidget.message.nodailymotionid" xml:space="preserve">
	  <value>Unable to parse Dailymotion video ID from the provided Url.</value>
  </data>
    <data name="videoembedwidget.message.noyoutubeid" xml:space="preserve">
	  <value>Unable to parse Youtube video ID from the provided Url.</value>
  </data>
</root>
```
This should handle the resolution of any of the resource string keys present in this widget.

