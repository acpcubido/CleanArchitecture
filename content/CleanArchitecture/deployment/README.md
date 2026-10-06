# Setup

1. alter `parameters.json` and `parameters-{env}.json` to your needs
2. enter powershell and login into az with `az login`
3. execute the following commands while in the `Infrastructure` folder

   - For shared resources (container registry):
     ```sh
     az deployment group create \
         --resource-group 'rg-cubido-template-shared' \
         --template-file '.\main-shared.bicep' \
         --parameters .\parameters-shared.json \
         --debug
     ```

   - For environment resources (container app, keyvault, storage, ...):
     ```sh
     az deployment group create \
         --resource-group 'rg-cubido-template-{env}' \
         --template-file '.\main.bicep' \
         --parameters .\parameters.json \
         --parameters .\parameters-{env}.json \
         --debug
     ```

# Troubleshooting

Issues you might run into:

## Issue:       SQL-Server region not available
Resolution:  Choose (only for the SQL-Server) a different region

## Issue:       'Failed to register resource provider operationalInsights'
Resolution:  Execute `az provider register --namespace Microsoft.OperationalInsights`

## Issue:       'The subscription is not registered for the resource type `smartDetectorAlertRules`
Resolution:  Execute `az provider register --namespace Microsoft.AlertsManagement`

# Things you need to know:
All resources grant permission to a managed identity (user managed identity).

In addition, for convenience and development reasons, we also grant the very same
permissions to an entire Entra-Group. (remoteAccessEntraGroupSID)
