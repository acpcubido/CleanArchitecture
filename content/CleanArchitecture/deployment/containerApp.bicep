param location string
param tags object

@description('Name of the container app')
param containerAppName string

@description('Custom domain FQDN for the app')
param customDomainFqdn string

@description('Container image to deploy')
param containerImage string

@description('Container app name for display purposes')
param containerName string

@description('Container App Environment ID')
param containerAppEnvironmentId string

@description('User-assigned managed identity resource ID')
param identityId string

@description('Container registry server URL')
param containerRegistryServer string

@description('Environment variables for the container')
param environmentVariables array

resource containerApp 'Microsoft.App/containerApps@2025-07-01' = {
  name: containerAppName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    environmentId: containerAppEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
        customDomains: [
          {
            name: customDomainFqdn
            bindingType: 'Auto'
          }
        ]
      }
      registries: [
        {
          server: containerRegistryServer
          identity: identityId
        }
      ]
    }
    template: {
      containers: [
        {
          name: containerName
          image: containerImage
          resources: {
            cpu: json('1.0')
            memory: '2Gi'
          }
          env: environmentVariables
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
  tags: tags
}

output containerAppFqdn string = containerApp.properties.configuration.ingress.fqdn
