# Last 25 events of the AppVision history database (read only).
#
# Requires the SqlServer PowerShell module:  Install-Module SqlServer -Scope CurrentUser
# Run it as the Windows account that runs the AppVision server when the databases are on SQL Server LocalDB
# (a LocalDB instance belongs to one Windows account). Otherwise set -ServerInstance to the "Data Source" of the
# HistoContext connection string in "AppVision 4\Bin\appServer.exe.config" (e.g. .\SQLEXPRESS).
param(
    [string]$ServerInstance = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'Histo',
    [int]$Count = 25
)

$connection = "Server=$ServerInstance;Database=$Database;Integrated Security=True;TrustServerCertificate=True"
Invoke-Sqlcmd -ConnectionString $connection -Query @"
SELECT TOP ($Count) EventDate, EventStateDesc, EventDescription, EventAreaDesc, EventOriginDesc
FROM dbo.Events
ORDER BY EventDate DESC
"@ | Format-Table -AutoSize | Out-String -Width 200

Pause
