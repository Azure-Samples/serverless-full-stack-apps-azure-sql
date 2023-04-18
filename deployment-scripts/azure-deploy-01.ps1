# 1. Deploy database 
$adminSqlLogin = "cloudadmin"
$password = Read-Host "Your username is 'cloudadmin'. Please enter a password for your Azure SQL Database server that meets the password requirements"
# Prompt for local ip address
$ipAddress = Read-Host "Disconnect your VPN, open PowerShell on your machine and run '(Invoke-WebRequest -Uri "https://ipinfo.io/ip").Content'. Please enter the value (include periods) next to 'Address': "
# Get resource group and location and random string
$resourceGroupName = "Sandbox resource group name"
$resourceGroup = Get-AzResourceGroup | Where ResourceGroupName -like $resourceGroupName
$uniqueID = Get-Random -Minimum 100000 -Maximum 1000000
$location = $resourceGroup.Location
# The logical server name has to be unique in the system
$serverName = "bus-server$($uniqueID)"
# The sample database name
$databaseName = "bus-db"
# Create a new server with a system wide unique server name
$server = New-AzSqlServer -ResourceGroupName $resourceGroupName `
    -ServerName $serverName 
    -Location $location 
    -SqlAdministratorCredentials $(New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $adminSqlLogin, $(ConvertTo-SecureString -String $password -AsPlainText -Force))
# Create a server firewall rule that allows access from the specified IP range and all Azure services
$serverFirewallRule = New-AzSqlServerFirewallRule 
    -ResourceGroupName $resourceGroupName 
    -ServerName $serverName 
    -FirewallRuleName "AllowedIPs" 
    -StartIpAddress $ipAddress -EndIpAddress $ipAddress 
$allowAzureIpsRule = New-AzSqlServerFirewallRule 
    -ResourceGroupName $resourceGroupName 
    -ServerName $serverName 
    -AllowAllAzureIPs
# Create a database
$database = New-AzSqlDatabase  -ResourceGroupName $resourceGroupName 
    -ServerName $serverName 
    -DatabaseName $databaseName 
    -Edition "GeneralPurpose" -Vcore 4 -ComputeGeneration "Gen5" 
    -ComputeModel Serverless -MinimumCapacity 0.5
