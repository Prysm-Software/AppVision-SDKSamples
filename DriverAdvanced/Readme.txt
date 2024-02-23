
Advanced demo driver

This C# project references 'appCommon.dll', 'appData.dll', 'appSDK.dll'
The output 'DriverAdvanced.exe' is copied into 'AppVision 4\Bin\' folder.


It requires the 'DriverAdvanced' protocol to be created in AppVision configuration

	Configurator/Protocols:
		add 'DriverAdvanced' protocol			Manager: DriverAdvanced.exe

This driver need some nodes with Parameters.Source set to "$P.DriverAdvanced" to work: they are called "linked variables".
It will activate them by setting their Variable Comm to Connected.

	Configurator/Variables:
	
		Device1			Source: $P.DriverAdvanced
		Device1.Comm	Type: logical	Flag Command: checked
		Device2			Source: $P.DriverAdvanced
		Device2.Comm	Type: logical	Flag Command: checked