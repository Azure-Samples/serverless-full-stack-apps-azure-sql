param (
    [Parameter(Mandatory=$true)]$server,
    [Parameter(Mandatory=$true)]$username    
)

$password = Read-Host "Azure SQL Password" -asSecureString

$sqlPackage="C:\Tools\SqlPackage\sqlpackage-win7-x64-en-US-15.0.4897.1\sqlpackage.exe"
$dacpac="../dacpac/bus-db.dacpac"

$PwdPointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
$PlainTextPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto($PwdPointer)
[Runtime.InteropServices.Marshal]::ZeroFreeBSTR($PwdPointer)

Invoke-Expression "$sqlPackage /a:publish /tcs:""Data Source=$server.database.windows.net;Initial Catalog=bus_db;UID=$username;PWD=$PlainTextPassword"" /sf:$dacpac"