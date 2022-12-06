Invoke-Sqlcmd -Query "SELECT TOP 25 EventDate, EventStateDesc, EventDescription FROM Histo.dbo.Events" -ServerInstance "(localdb)\V11.0" | Out-String
Pause