# 2. Deploy Azure Function
# Get resource group and location and random string
$resourceGroupName = "Sandbox resource group name"
$resourceGroup = Get-AzResourceGroup | Where ResourceGroupName -like $resourceGroupName
$uniqueID = Get-Random -Minimum 100000 -Maximum 1000000
$location = $resourceGroup.Location
# Azure function name
$azureFunctionName = $("azfunc$($uniqueID)")
# Get storage account name
$storageAccountName = (Get-AzStorageAccount -ResourceGroup $resourceGroupName).StorageAccountName

# Uncomment ro deploy Azure Function for dotnet
<#
$functionApp = New-AzFunctionApp -Name $azureFunctionName `
    -ResourceGroupName $resourceGroupName -StorageAccount $storageAccountName `
    -FunctionsVersion 3 -RuntimeVersion 3 -Runtime dotnet -Location $location
#>

# Uncomment to deploy Azure Function for python
<#
$functionApp = New-AzFunctionApp -Name $azureFunctionName `
    -ResourceGroupName $resourceGroupName -StorageAccount $storageAccountName `
    -FunctionsVersion 3 -RuntimeVersion 3.8 -Runtime python -Location $location
#>

# Uncomment to deploy Azure Function for node
<#
$functionApp = New-AzFunctionApp -Name $azureFunctionName `
    -ResourceGroupName $resourceGroupName -StorageAccount $storageAccountName `
    -FunctionsVersion 3 -RunTimeVersion 12 -Runtime node -Location $location
#>