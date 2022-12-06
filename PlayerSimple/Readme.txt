
	Demo video player for AppVision

This basic sample demontrates how to developt a custom video player for AppVision.
This player uses FFmpeg library for video decoding.

The player is an UserControl that inherit from PlayerBaseControl to integrate in AppVision framework.
It implements the IPlayerLive interface, hence only live stream will be available in AppVision client software. 
Check PlayerAdvanced sample for more details about Playback, Export and PTZ features.
