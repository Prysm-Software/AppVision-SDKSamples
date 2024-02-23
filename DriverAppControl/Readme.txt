
	Demo AppControl driver

This C# project references 'appCommon.dll', 'appControl.dll', 'appData.dll', 'appSDK.dll'
The output 'DriverAppControl.exe' is copied into 'AppVision 4\Bin\' folder.


It requires the 'DriverAppControl' protocol to be created in AppVision configuration

	Configurator/Protocols:
		add 'DriverAppControl' protocol			Manager: DriverAppControl.exe

This driver need some nodes with Parameters.Source set to "$P.DriverAppControl" to work: they are called "linked variables".
It allow to download data to the devices.