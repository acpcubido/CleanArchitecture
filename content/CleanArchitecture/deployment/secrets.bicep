@secure()
param vaultName string
//#if (BlobContainerName != "")
@secure()
param storageAccountKey string
//#endif

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' existing = {
  name: vaultName
}

//#if (BlobContainerName != "")
// BlobStorage access-key
resource blobStorageSecret 'Microsoft.KeyVault/vaults/secrets@2024-11-01' = {
  parent: keyVault
  name: 'BlobStorage--AccessKey'
  properties: {
    value: storageAccountKey
  }
}
//#endif
