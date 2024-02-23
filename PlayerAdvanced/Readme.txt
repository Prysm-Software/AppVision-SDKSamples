
	Demo AppVision player

This sample demonstrates how to build a custom video player for AppVision.

This C# project references 'appCommon.dll', 'appSDK.dll', 'appVideo.dll'
The output 'PlayerClock.dll' is copied into 'AppVision 4\Bin\' folder.

A video player inherits from PlayerBaseControl. Based on the video system capabilities, the player implements some of the following interfaces :
IPlayerLive, IPlayerPlayback, IPlayerPtz, IPlayerExport

It can be embedded in a synoptic. See "Third-party .Net object" in Config/Synoptics/Objects
or it can be loaded by code using the following syntax:
	Client.Load("PlayerClock.dll#PlayerClock.PlayerClockControl");