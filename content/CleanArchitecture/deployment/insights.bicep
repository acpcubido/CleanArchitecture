param environment string
param location string
param resourceNames object
param alertReceivers array
param tags object

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: resourceNames.logAnalyticsWorkspace
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
  tags: tags
}

resource insights 'Microsoft.Insights/components@2020-02-02' = {
  name: resourceNames.appInsights
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Bluefield'
    Request_Source: 'rest'
    WorkspaceResourceId: logAnalyticsWorkspace.id
    RetentionInDays: 90
  }
  tags: tags
}

resource insightsActionGroup 'Microsoft.Insights/actionGroups@2022-06-01' = {
  name: resourceNames.appInsightsActionGroup
  location: location
  properties: {
    groupShortName: 'Group'
    enabled: true
    emailReceivers: alertReceivers
  }
  tags: tags
}

resource insightsAlerts 'microsoft.alertsManagement/smartDetectorAlertRules@2021-04-01' = {
  name: resourceNames.appInsightsAlerts
  location: location
  properties: {
    scope: [
      insights.id
    ]
    actionGroups: {
      customEmailSubject: '[${environment}] Alert'
      groupIds: [
        resourceId('Microsoft.Insights/actionGroups', resourceNames.appInsightsActionGroup)
      ]
    }
    description: 'Failure Anomalies notifies you of an unusual rise in the rate of failed HTTP requests or dependency calls.'
    severity: 'Sev3'
    state: 'Enabled'
    frequency: 'PT1M'
    detector: {
      id: 'FailureAnomaliesDetector'
    }
  }
  tags: tags
}

output logAnalyticsWorkspaceId string = logAnalyticsWorkspace.id
output logAnalyticsCustomerId string = logAnalyticsWorkspace.properties.customerId
@secure()
output logAnalyticsPrimarySharedKey string = logAnalyticsWorkspace.listKeys().primarySharedKey
output appInsightInstrumentationKey string = insights.properties.InstrumentationKey
output appInsightConnectionString string = insights.properties.ConnectionString
