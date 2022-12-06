
AppVision uses different databases:

	'Param' stores the configuration data
	'State' stores active alarms and current states of variables
	'Histo' stores past alarms and events
	'AppControl' stores access control specific data (badge holders, access right...)


UML directory contains the dababase diagrams
SQL directory contains the database creation scripts

The sample "Query events history.ps1" PowerShell script demonstrates how to run a SQL query against HISTO database.

To run the sample: right-click the script file and select "Run with PowerShell".