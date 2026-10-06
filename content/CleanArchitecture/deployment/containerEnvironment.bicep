param location string
param resourceNames object
param tags object
param logAnalyticsCustomerId string
@secure()
param logAnalyticsPrimarySharedKey string

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: resourceNames.containerAppEnvironment
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsCustomerId
        sharedKey: logAnalyticsPrimarySharedKey
      }
    }
  }
  tags: tags
}

output environmentId string = containerAppEnvironment.id
output environmentName string = containerAppEnvironment.name
output customDomainVerificationId string = containerAppEnvironment.properties.customDomainConfiguration.customDomainVerificationId
