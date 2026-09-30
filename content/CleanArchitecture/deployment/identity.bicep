param resourceNames object
param location string
param tags object

resource userAssignedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: resourceNames.appContainerApp
  location: location
  tags: tags
}

output principalId string = userAssignedIdentity.properties.principalId
output identityId string = userAssignedIdentity.id
output clientId string = userAssignedIdentity.properties.clientId
