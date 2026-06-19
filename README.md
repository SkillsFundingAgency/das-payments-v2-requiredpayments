# Payments V2 Required Payments

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-requiredpayments?branchName=main)](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-requiredpayments?branchName=main)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/secure/RapidBoard.jspa?rapidView=782&projectKey=PV2)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3700621400/Provider+and+Employer+Payments+Payments+BAU)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)


## How It Works

The required payments service is responsible for calculating how much should be paid for the earnings in each period.  It will use the output from the DC earnings calc and also look at previous payments for the same period and determine how much should be paid.  This service will publish calculated levied payments, calculated co-invested payments, and calculated incentive payments

More information here: 
- https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/400130049/4.+Payments+v2+-+Components+DAS+Space
- https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/533856326/a.+Apprenticeship+Earning+Event+DAS+Space

## 🚀 Installation

### Pre-Requisites

Setup instructions can be found at the following link, which will help you set up your environment and access the correct repositories: https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/4948754681/DAS+Payments+-+Developer+Onboarding+2025

Select the configuration for the Required Payments application


## How to run the specs project

In order to run SFA.DAS.Payments.RequiredPayments.Tests.Specs successfully you will need the following:

Within the project:
- Within SFA.DAS.Payments.RequiredPayments.Tests.Specs create a file called appSettings.local.Json. Copy and paste the contents of appSettings.json into this file and populate the following values: 
StorageConnectionString, ServiceBusConnectionString and PaymentsConnectionString.

On the Azure Portal:
- Go to https://portal.azure.com/#home or https://portal.azure.com/#browse/Microsoft.ServiceBus%2Fnamespaces, search das-pv2-dev-{your initials} , should be a service bus namespace.
- Towards the bottom of the page, beneath requests, there should be a table containing Queues and Topics, select Topics and then select bundle-1
- Towards the bottom of the page, beneath metrics, there should be a table titled subscriptions, type in sfa-das-payments-requiredpayments-tests-specs and select it. 
- Delete the default filter. 
- Create a new filter with the following information:
	Name: specs
	Filter Type: SQL Filter
	SQL: [NServiceBus.EnclosedMessageTypes] LIKE 'SFA.DAS.Payments.RequiredPayments.Messages%'

After the following have been done, within the IDE, run (without debugging) SFA.DAS.Payments.RequiredPayments.ServiceFabric


## 🔗 External Dependencies

N/A

## Technologies

* .NetCore 6
* Azure SQL Server
* Azure Functions
* Azure Service Bus
* ServiceFabric

## 🐛 Known Issues

N/A
 
