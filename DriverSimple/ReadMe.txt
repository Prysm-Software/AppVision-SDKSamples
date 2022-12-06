
	Demo AppVision driver

This C# project references 'appCommon.dll', 'appData.dll', 'appSDK.dll'
The output 'DriverSample.exe' is copied into 'AppVision 4\Bin\' folder.


This driver commands and monitors 8 input variables, 8 output variables and 1 buffer variable.
AppVision configuration should include the following set of protocol and variables:

	Config/Protocols:

		DrvSample					Manager: DriverSample.exe


	Config/Variables:

		DrvSampleVar				Source: $P.DrvSample
		DrvSampleVar.Input1			Type: logical
		DrvSampleVar.Input2			Type: logical
		...
		DrvSampleVar.Input8			Type: logical
		DrvSampleVar.Output1		Type: logical	Flag Command: checked	Source: =$parent.Input1
		DrvSampleVar.Output2		Type: logical	Flag Command: checked	Source: =$parent.Input2
		...
		DrvSampleVar.Output8		Type: logical	Flag Command: checked	Source: =$parent.Input8
		DrvSampleVar.Buffer			Type: buffer	Buffer type: Int32




