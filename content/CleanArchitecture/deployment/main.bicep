targetScope='resourceGroup'

@description('env added to each resource')
param environment string
@description('the execution env. This does not affect the azure infrastructure, but the future running services')
param executionEnvironment string
@description('where to deploy the resources (EU only)')
@allowed(['westeurope', 'northeurope', 'swedencentral', 'uksouth', 'francecentral', 'germanywestcentral'])
param location string
@description('where to deploy the sql-server resource (EU only)')
@allowed(['westeurope', 'northeurope', 'swedencentral', 'uksouth', 'francecentral', 'germanywestcentral'])
param sqlServerLocation string

@description('the EntraID Group name (just for display purposes - may be anything)')
param remoteAccessEntraGroupName string
@description('the EntraID Group identifier to grant access to the SQL Server')
@minLength(36)
@maxLength(36)
param remoteAccessEntraGroupSID string
@description('the IP address to whitelist to access the sql-server remotely. Empty if none')
param remoteAccessFrom string
@description('ppl. who receive any alerts from the app-insights')
param alertReceivers array

@description('Shared container registry name (e.g., acrrstxxxxxx)')
param sharedAcrName string
@description('Shared container registry resource group name')
param sharedAcrResourceGroup string = 'RG_ProjFinder_SHARED'
@description('Container image tag to deploy (e.g., dev, test, prod, or build ID)')
param imageTag string = environment

@description('A fully qualified domain name (FQDN) for the container app (e.g., dev.example.com)')
param fqdn string = '${environment}.projectfinder.alukoenigstahl.com'

@description('additional tags added to each resource')
param tags object = {
  environment: environment
  project: 'cubido-template'
}

// Arbitrary but fixed random identifier for the resources
var uid = substring(replace(guid('cubido', tags.project, environment), '-', ''), 0, 24)
// uid for postfixes
var suid = substring(uid, 0, 6)

var resourceNames = {
    resourceGroup: 'rg-${tags.project}-${environment}'
    logAnalyticsWorkspace: 'log-${environment}'
    appContainerApp: 'app-${environment}'
    containerAppEnvironment: 'cae-${environment}'
    containerAppCertificate: 'certificate-${environment}'
    appInsightsAlerts: 'apr-${environment}'
    appInsightsActionGroup: 'ag-${environment}'
    appInsights: 'appi-${environment}'
    keyVault: 'kv-${environment}-${suid}'
    sqlServer: 'sql-${environment}-${suid}'
    sqlDatabase: 'sqldb-${environment}'
    sqlRole: guid(uid, 'sql-roles', environment)
    storageAccount: 'st-${environment}-${suid}'
//#if (BlobContainerName != "")
    blobStorage: uid
    blobStorageEntraGroupId: guid(uid, 'blobStorageEntraGroupId')
    blobStorageManagedId: guid(uid, 'blobStorageManagedId')
//#endif
    containerRegistry: sharedAcrName
}

// Phase 1: Create user-assigned managed identity
module identity 'identity.bicep' = {
  params: {
    resourceNames: resourceNames
    location: location
    tags: tags
  }
}

// Phase 2: Deploy infrastructure resources
module keyVault 'keyvault.bicep' = {
  params: {
    resourceNames: resourceNames
    remoteAccessEntraGroupSID: remoteAccessEntraGroupSID
    containerAppPrincipalId: identity.outputs.principalId
    location: location
    tags: tags
  }
}

//#if (BlobContainerName != "")
module storage 'blobstorage.bicep' = {
  params: {
    resourceNames: resourceNames
    remoteAccessEntraGroupSID: remoteAccessEntraGroupSID
    containerAppPrincipalId: identity.outputs.principalId
    location: location
    tags: tags
  }
}
//#endif

module database 'database.bicep' = {
  params: {
    location: sqlServerLocation
    resourceNames: resourceNames
    remoteAccessFrom: remoteAccessFrom
    remoteAccessEntraGroupName: remoteAccessEntraGroupName
    remoteAccessEntraGroupSID: remoteAccessEntraGroupSID
    containerAppPrincipalId: identity.outputs.principalId
    tags: tags
  }
}

// Phase 3: Grant managed identity access to the ACR
module acrAccess 'containerregistry-access.bicep' = {
  name: 'acr-access-${environment}'
  scope: resourceGroup(sharedAcrResourceGroup)
  params: {
    acrName: sharedAcrName
    principalId: identity.outputs.principalId
    environment: environment
  }
}

// Phase 4: Deploy monitoring infrastructure (Log Analytics, App Insights, Alerts)
module insights 'insights.bicep' = {
  params: {
    environment: environment
    location: location
    resourceNames: resourceNames
    alertReceivers: alertReceivers
    tags: tags
  }
}

// Phase 5: Deploy Container App Environment
module containerEnv 'containerEnvironment.bicep' = {
  params: {
    location: location
    resourceNames: resourceNames
    tags: tags
    logAnalyticsCustomerId: insights.outputs.logAnalyticsCustomerId
    logAnalyticsPrimarySharedKey: insights.outputs.logAnalyticsPrimarySharedKey
  }
}

// Phase 6: Deploy container apps

// Shared environment variables builder
var baseEnvironmentVariables = [
  {
    name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
    value: insights.outputs.appInsightInstrumentationKey
  }
  {
    name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
    value: insights.outputs.appInsightConnectionString
  }
  {
    name: 'AzureMonitor__ConnectionString'
    value: insights.outputs.appInsightConnectionString
  }
  {
    name: 'KeyVault__Uri'
    value: keyVault.outputs.uri
  }
  {
    name: 'KeyVault__DataProtectionKeyUri'
    value: keyVault.outputs.dataProtectionKeyUri
  }
  {
    name: 'AZURE_CLIENT_ID'
    value: identity.outputs.clientId
  }
  {
    name: 'AZURE_TENANT_ID'
    value: subscription().tenantId
  }
  {
    name: 'ConnectionStrings__DefaultConnection'
    value: '${database.outputs.connectionString}User Id=${identity.outputs.clientId};'
  }
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: executionEnvironment
  }
]

module containerApp 'containerApp.bicep' = {
  dependsOn: [
    acrAccess
  ]
  params: {
    location: location
    tags: tags
    containerAppName: resourceNames.appContainerApp
    customDomainFqdn: fqdn
    containerImage: '${resourceNames.containerRegistry}.azurecr.io/app-web:${imageTag}'
    containerName: 'app'
    containerAppEnvironmentId: containerEnv.outputs.environmentId
    identityId: identity.outputs.identityId
    containerRegistryServer: '${resourceNames.containerRegistry}.azurecr.io'
    environmentVariables: concat(baseEnvironmentVariables, [
//#if (BlobContainerName != "")
      {
        name: 'BlobStorageOptions__Url'
        value: storage.outputs.url
      }
//#endif
      {
        name: 'KeyVaultOptions__Name'
        value: keyVault.outputs.resourceName
      }
      {
        name: 'KeyVaultOptions__Uri'
        value: keyVault.outputs.uri
      }
    ])
  }
}

// Phase 7: Populate Key Vault with secrets
module vaultSecrets 'secrets.bicep' = {
  params: {
    vaultName: keyVault.outputs.resourceName
//#if (BlobContainerName != "")
    storageAccountKey: storage.outputs.primaryKey
//#endif
  }
}

// Phase 8: Deploy SSL certificates for container apps
module envDnsAndCerts 'containerCertificates.bicep' = {
  dependsOn: [
    containerApp
  ]
  params: {
    location: location
    tags: tags
    containerAppEnvironmentName: containerEnv.outputs.environmentName
    dnsRecordsWithCertificates: [
      {
        certificateName: resourceNames.containerAppCertificate
        domainFqdn: fqdn
      }
    ]
  }
}
