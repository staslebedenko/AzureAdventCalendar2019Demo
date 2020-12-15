# Azure Advent Calendar 2019 SQL Serverless.
Demo repository of Azure SQL Serverless Database + Azure Functions C# application for load testing.
https://azureadventcalendar.com/ 

https://www.youtube.com/channel/UCJL9wCcmeMBbah4J0uOWIPg

Goal is to observe Azure SQL Serveless Database behavior under the load from Azure Functions.
Loader.io free account is used for load testing.

Azure CLI for solution infrastructure.

```bash

#----------------------------------------------------------------------------------
# Resource group
#----------------------------------------------------------------------------------

location=northeurope
postfix=$RANDOM

# resource group
groupName=AzureAdventCalendar$postfix

az group create --name $groupName --location $location
#az group delete --name $groupName


#----------------------------------------------------------------------------------
# Storage account with Blob container
#----------------------------------------------------------------------------------

location=northeurope
accountSku=Standard_LRS
accountName=${groupName,,}
echo "accountName  = " $accountName

az storage account create --name $accountName --location $location --kind StorageV2 \
--resource-group $groupName --sku $accountSku --access-tier Hot  --https-only true

accountKey=$(az storage account keys list --resource-group $groupName --account-name $accountName --query "[0].value" | tr -d '"')
echo "storage account key = " $accountKey

connString="DefaultEndpointsProtocol=https;AccountName=$accountName;AccountKey=$accountKey;EndpointSuffix=core.windows.net"
echo "connection string = " $connString

blobName=${groupName,,}

az storage container create --name $blobName \
--account-name $accountName --account-key $accountKey --public-access off

#----------------------------------------------------------------------------------
# Application insights instance
#----------------------------------------------------------------------------------

insightsName=${groupName,,}
echo "insightsName  = " $insightsName

# drop this command with ctrl+c after 3 minutes of execution
az resource create --resource-group $groupName --name $insightsName --resource-type "Microsoft.Insights/components" --location $location --properties '{"Application_Type":"web"}' --verbose

insightsKey=$(az resource show -g $groupName -n $insightsName --resource-type "Microsoft.Insights/components" --query properties.InstrumentationKey --output tsv) 
echo "Insights key = " $insightsKey


#----------------------------------------------------------------------------------
# Function app with consumption plan. Use KeyVault in production :)
#----------------------------------------------------------------------------------

runtime=dotnet
location=northeurope
applicationName=${groupName,,}
accountName=${groupName,,}
echo "applicationName  = " $applicationName

az functionapp create --resource-group $groupName \
--name $applicationName --storage-account $accountName --runtime $runtime \
--app-insights-key $insightsKey --consumption-plan-location $location

az functionapp update --resource-group $groupName --name $applicationName --set dailyMemoryTimeQuota=400000

az functionapp identity assign --resource-group $groupName --name $applicationName

az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings "MSDEPLOY_RENAME_LOCKED_FILES=1"

managedIdKey=$(az functionapp identity show --name $applicationName --resource-group $groupName --query principalId --output tsv)
echo "Managed Id key = " $managedIdKey

#----------------------------------------------------------------------------------
# Azure SQL Server and Serverless DB 1-4 cores and 32 Gb storage
#----------------------------------------------------------------------------------

location=northeurope
serverName=${groupName,,}
adminLogin=Admin$groupName
password=Sup3rStr0ng$groupName$postfix
databaseName=${groupName,,}
serverSku=S0
catalogCollation="SQL_Latin1_General_CP1_CI_AS"

az sql server create --name $serverName --resource-group $groupName --assign-identity \
--location $location --admin-user $adminLogin --admin-password $password

az sql db create --resource-group $groupName --server $serverName --name $databaseName \
--edition GeneralPurpose --family Gen5 --compute-model Serverless \
--auto-pause-delay 60 --capacity 4

outboundIps=$(az webapp show --resource-group $groupName --name $applicationName --query possibleOutboundIpAddresses --output tsv)
IFS=',' read -r -a ipArray <<< "$outboundIps"

for ip in "${ipArray[@]}"
do
echo "$ip add"
az sql server firewall-rule create --resource-group $groupName --server $serverName \
--name "WebApp$ip" --start-ip-address $ip --end-ip-address $ip
done

sqlClientType=ado.net

#TODO add Admin login and remove password, set to variable.
sqlConString=$(az sql db show-connection-string --name $databaseName --server $serverName --client $sqlClientType --output tsv)
sqlConString=${sqlConString/Password=<password>;}
sqlConString=${sqlConString/<username>/$adminLogin}
echo "SQL Connection string is = " $sqlConString

# on your PC run CMD as administrator, then execute following commands and reboot PC.
# just copy command output below to CMD and execute.

az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings "SqlConnectionString=$sqlConString"
az functionapp config appsettings set --resource-group $groupName --name $applicationName --settings "SqlConnectionPassword=$password"

# on your PC run CMD as administrator, then execute following commands and reboot PC.
# just copy command output below to CMD and execute.
echo "setx APPINSIGHTS_INSTRUMENTATIONKEY "$insightsKey
echo "setx StorageConnectionString \""$connString\"
echo "setx SqlConnectionString \""$sqlConString\"
echo "setx SqlConnectionPassword "$password

```
