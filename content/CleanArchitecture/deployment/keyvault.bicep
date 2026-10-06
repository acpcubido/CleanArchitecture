param resourceNames object
param remoteAccessEntraGroupSID string
param location string
param tags object
param containerAppPrincipalId string

resource keyVault 'Microsoft.KeyVault/vaults@2024-11-01' = {
  name: resourceNames.keyVault
  location: location
  properties: {
    enabledForDeployment: true
    enabledForTemplateDeployment: true
    enabledForDiskEncryption: true
    tenantId: subscription().tenantId
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: remoteAccessEntraGroupSID
        permissions: {
          keys: [
            'all'
          ]
          certificates: [
            'all'
          ]
          secrets: [
            'all'
          ]
          storage: [
            'all'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: containerAppPrincipalId
        permissions: {
          keys: [
            'list'
            'get'
            'wrapKey'
            'unwrapKey'
          ]
          certificates: [
            'list'
            'get'
          ]
          secrets: [
            'list'
            'get'
          ]
          storage: [
            'list'
            'get'
          ]
        }
      }
      {
        tenantId: subscription().tenantId
        objectId: deployer().objectId
        permissions: {
          keys: [
            'all'
          ]
          certificates: [
            'all'
          ]
          secrets: [
            'all'
          ]
          storage: [
            'all'
          ]
        }
      }
    ]
    sku: {
      name: 'standard'
      family: 'A'
    }
  }
  tags: tags
}

resource dataProtectionSecret 'Microsoft.KeyVault/vaults/keys@2026-02-01' = {
  parent: keyVault
  name: 'dataProtectionSecret'
  properties: {
    keyOps: [
      'encrypt'
      'decrypt'
      'sign'
      'verify'
      'wrapKey'
      'unwrapKey'
    ]
    keySize: 2048
    kty: 'RSA'
    rotationPolicy: {
        attributes: {
            expiryTime: 'P90D'
        }
        lifetimeActions: [
            {
                action: {
                    type: 'rotate'
                }
                trigger: {
                    timeBeforeExpiry: 'P7D'
                }
            }
        ]
    }
  }
}

resource keyVaultSecretUserRoleRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6'
}

resource keyVaultSecretUserRoleAssignment_ContainerAppApi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(resourceGroup().id, containerAppPrincipalId, keyVaultSecretUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultSecretUserRoleRoleDefinition.id
    principalId: containerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultCertificateUserRoleRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'db79e9a7-68ee-4b58-9aeb-b90e7c24fcba'
}

resource keyVaultCertificateUserRoleAssignment_ContainerAppApi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(resourceGroup().id, containerAppPrincipalId, keyVaultCertificateUserRoleRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultCertificateUserRoleRoleDefinition.id
    principalId: containerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

//Key Vault Crypto Service Encryption User (get, unwrap, wrap keys)
resource keyVaultCryptoServiceEncryptionUserRoleDefinition 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'e147488a-f6f5-4113-8e2d-b22465e65bf6'
}

resource keyVaultCryptoServiceEncryptionUserRoleAssignment_ContainerAppApi 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: keyVault
  name: guid(resourceGroup().id, containerAppPrincipalId, keyVaultCryptoServiceEncryptionUserRoleDefinition.id)
  properties: {
    roleDefinitionId: keyVaultCryptoServiceEncryptionUserRoleDefinition.id
    principalId: containerAppPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output resourceName string = keyVault.name
output uri string = keyVault.properties.vaultUri
output dataProtectionKeyUri string = dataProtectionSecret.properties.keyUri
