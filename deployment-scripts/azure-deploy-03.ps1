# 3. Deploy Azure Static Web App and Logic App
# Get resource group and location and random string
$resourceGroupName = "Sandbox resource group name"
$resourceGroup = Get-AzResourceGroup | Where ResourceGroupName -like $resourceGroupName
$uniqueID = Get-Random -Minimum 100000 -Maximum 1000000
$location = $resourceGroup.Location
# Azure static web app name
$webAppName = $("bus-app$($uniqueID)")


# Get the repository name
$appRepository = Read-Host "Please fork the GitHub repository and enter the forked URL (e.g. https://github.com/<username>/mslearn-full-stack-azure-sql):"
# Get user's GitHub personal access token
$githubToken = (Read-Host "In your GitHub account settings, near the bottom left, select Developer settings > Personal access tokens > check all boxes and generate the token. Enter the token").ToString()
# App service plan name
$appServicePlanName = (Get-AzAppServicePlan -resourceGroupName $resourceGroupName).Name

# Deploy Azure static web app
$staticWebApp = az staticwebapp create -n $webAppName -g $resourceGroupName `
    -s $appRepository -l 'westus2' -b main --token $githubToken
# Store creds
$storeCreds = git config --global credential.helper store

# Deploy logic app
# Clone the repo -- note this also asks for the token
$cloneRepository = git clone $appRepository
# Get subscription ID 
$subId = [Regex]::Matches($resourceGroup.ResourceId, "(\/subscriptions\/)+(.*\/)+(.*\/)").Groups[2].Value
$subId = $subId.Substring(0,$subId.Length-1)
# Deploy logic app
az deployment group create --name DeployResources --resource-group $resourceGroupName `
    ` --template-file ./sql-bus/deployment-scripts/template.json `
    --parameters subscription_id=$subId location=$location