# Prints the hash of a password, in the format expected by ExternalUsers.json (PBKDF2-SHA256, 100 000 iterations).
# Usage:  .\New-PasswordHash.ps1 "the password"
param([Parameter(Mandatory = $true)][string]$Password)

$salt = New-Object byte[] 16
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($salt)
$kdf = New-Object Security.Cryptography.Rfc2898DeriveBytes($Password, $salt, 100000, [Security.Cryptography.HashAlgorithmName]::SHA256)
'pbkdf2$100000$' + [Convert]::ToBase64String($salt) + '$' + [Convert]::ToBase64String($kdf.GetBytes(32))
