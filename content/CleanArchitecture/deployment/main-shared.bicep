targetScope = 'resourceGroup'

@description('where to deploy the resources (EU only)')
@allowed(['westeurope', 'northeurope', 'swedencentral', 'uksouth', 'francecentral', 'germanywestcentral'])
param location string = 'westeurope'

// Merge incoming tags with shared-specific tag
var sharedTags = {
  shared: 'true'
}

// Fixed UID for shared resources (same across all deployments)
var sharedUid = substring(replace(guid('aks-shared-registry'), '-', ''), 0, 6)

module acr 'containerregistry-shared.bicep' = {
  name: 'shared-acr-deployment'
  params: {
    registryName: 'acraks${sharedUid}'
    location: location
    tags: sharedTags
  }
}

output acrName string = acr.outputs.name
output acrLoginServer string = acr.outputs.loginServer
output resourceGroupName string = resourceGroup().name
