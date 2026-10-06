param resourceNames object
param remoteAccessEntraGroupSID string
param location string
param tags object
param containerAppPrincipalId string

// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
var storageBlobDataContributorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
)
var storageBlobDelegatorRoleId = subscriptionResourceId(
  'Microsoft.Authorization/roleDefinitions',
  'db58b8e5-c6ad-4a2a-8342-4190687cbf4a'
)

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: resourceNames.blobStorage
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'BlobStorage'
  properties: {
    accessTier: 'Hot'
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
  tags: tags
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2024-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource blobContainerDrawings 'Microsoft.Storage/storageAccounts/blobServices/containers@2024-01-01' = {
  parent: blobService
  name: 'BlobContainerName'
  properties: {
    publicAccess: 'None'
  }
}

resource entraGroupAssignContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, remoteAccessEntraGroupSID, storageBlobDataContributorRoleId)
  scope: storageAccount
  properties: {
    principalType: 'Group'
    roleDefinitionId: storageBlobDataContributorRoleId
    principalId: remoteAccessEntraGroupSID
  }
}

resource entraGroupAssignDelegator 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, remoteAccessEntraGroupSID, storageBlobDelegatorRoleId)
  scope: storageAccount
  properties: {
    principalType: 'Group'
    roleDefinitionId: storageBlobDelegatorRoleId
    principalId: remoteAccessEntraGroupSID
  }
}

// Setup permissions for managed identity
resource managedIdentityAssignContributor 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, containerAppPrincipalId, storageBlobDataContributorRoleId)
  scope: storageAccount
  properties: {
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDataContributorRoleId
    principalId: containerAppPrincipalId
  }
}

resource managedIdentityAssignDelegator 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, containerAppPrincipalId, storageBlobDelegatorRoleId)
  scope: storageAccount
  properties: {
    principalType: 'ServicePrincipal'
    roleDefinitionId: storageBlobDelegatorRoleId
    principalId: containerAppPrincipalId
  }
}

output url string = 'https://${storageAccount.name}.blob.${az.environment().suffixes.storage}/'
output id string = storageAccount.id
@secure()
output primaryKey string = storageAccount.listKeys().keys[0].value
