param location string
param tags object

@description('Container App Environment Name')
param containerAppEnvironmentName string

@description('Array of DNS records to create with their certificate domains')
param dnsRecordsWithCertificates array

// Create SSL certificates for all domains
resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' existing = {
  name: containerAppEnvironmentName
}

resource certificates 'Microsoft.App/managedEnvironments/managedCertificates@2025-10-02-preview' = [for record in dnsRecordsWithCertificates: {
  parent: containerAppEnvironment
  location: location
  name: record.certificateName
  properties: {
    domainControlValidation: 'CNAME'
    subjectName: record.domainFqdn
  }
  tags: tags
}]
