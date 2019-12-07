# Zeta HTML Edit Control

This project presents you a small wrapper class around the Windows Forms 2.0 [`WebBrowser` control](http://msdn.microsoft.com/en-us/library/2te2y1x6.aspx) control.

![Screenshot of Zeta Html Edit Control](http://i.imgur.com/5medndw.png)

We do use this project inside several "real world" projects in our company, e.g. a [Test Management tool](http://www.zeta-test.com) and a Windows-based [Content Management System (CMS)](http://www.zeta-producer.com/de/cms.html).

## Features

The wrapper is no rocket science but provides some features I struggled with in the past. We use the control in several of our own (internal) applications.

Some of the features include:

  * Enable the setting of text even when the control is not yet fully initialized.
  * Allow pasting of images from the clipboard.
  * Allows for resizing the images within the editor with the mouse 
  * Provide an alternative context menu for applying formatting options.
  * Optionally directly edit the underlying HTML source code.
  * Translated resources in English and German.
  * Interface `IExternalInformationProvider` for externally persisting and restoring settings.
  * Provide standard CSS in the control; usually you do not need to define your own CSS.

## Using the code

To include the code in your own project, simply include the "ZetaHtmlEditControl.dll" assembly into your project.

Add the assembly to your Visual Studio .NET 2008 Windows Forms Designer Toolbox if you want to be able to drag the `ExtendedWebBrowser` control to your forms. Alternatively create and initialize an instance of the ExtendedWebBrowser control by code.

## Setting HTML

To put HTML from your code into the control, assign the HTML code to the `ExtendedWebBrowser.DocumentText` property. You do _not_ have to pass a complete HTML document with `HEAD` and `BODY` tags but only the actual content that you would write inside the `BODY` tag.

## Getting the HTML

To read out the HTML from the control, call the `ExtendedWebBrowser.GetDocumentText( string folderPath )` method. The method takes one parameter "folderPath" that tells the control where to store newly passed images from the clipboard.

## Other notes

You need to have the `<NetFx40_LegacySecurityPolicy enabled="true" />` switch inside your application's configuration file in order to use the control within .NET 4. E.g. something like that:

    <?xml version="1.0"?>
    <configuration>
      <startup>
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.0"/>
      </startup>
      <runtime>
        <loadFromRemoteSources enabled="true" />
        <NetFx40_LegacySecurityPolicy enabled="true" />
      </runtime>
    </configuration>

A more [detailed article](http://www.codeproject.com/Articles/43954/ZetaplusHTMLplusEditplusControl) is available [over on The Code Project](http://www.codeproject.com/Articles/43954/ZetaplusHTMLplusEditplusControl).

## History

  * *2013-11-12* - Added functions to paste text from Microsoft Word and clean that up.
  * *2013-11-05* - Foreground and background colors with CSS.
  * *2013-08-10* - Added app.config switch for .NET 4.
  * *2013-05-30* - First public release to Github.
