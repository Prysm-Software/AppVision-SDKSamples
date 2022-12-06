
	Demo WPF control to be displayed in an AppVision client.

This C# project references 'appCommon.dll', 'appData.dll', 'appSDK.dll', 'appUI.dll'
The output 'WpfControlAdvanced.dll' is copied into 'AppVision 4.0\Bin\' folder.

This demo control uses AppVision SDK to control its application host and to interact with AppVision server.

It can be embedded in a synoptic. See "Third-party .Net object" in Config/Synoptics/Objects
or it can be loaded by code using the following syntax:
	Client.Load("WpfControlAdvanced.dll#WpfControlAdvanced.UserControl1");