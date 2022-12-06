
This sample project demonstrate how to build a AppVision Configuration Wizard

A configuration wizard allows to add one or more devices into AppVision configuration through a series of steps.
The flow of execution should be adjusted to reflect the system capabilities.

The "<todo>" placeholder indicates where to insert system specific tasks.

Wizard steps:
WelcomePage			Startup page
AppLoginPage		Connection to AppVision server. If credentials are provided in the command line (http://user:pwd@host), this step is skipped.
DiscoveryPage		If a discovery mechanism is available, this step displays all discovered devices. Remove this page if there is no discovery feature.
DeviceLoginPage		Collect device information such as address, username, password and log in the device. Remove this page if authentication is not suppored.
DeviceDetailsPage	Scan device and display device's entities. e.g. list all cameras of a VMS
SettingsPage		Adjust settings before importing device into AppVision configuration
SummaryPage			Final review before import
ImportPage			Start import and display progress
CompletedPage		Display import results.
