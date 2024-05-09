# Payments V2 Required Payments

<img src="https://avatars.githubusercontent.com/u/9841374?s=200&v=4" align="right" alt="UK Government logo">

[![Build Status](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-requiredpayments?branchName=main)](https://dev.azure.com/sfa-gov-uk/DCT/_apis/build/status/GitHub/Service%20Fabric/SkillsFundingAgency.das-payments-v2-requiredpayments?branchName=main)
[![Jira Project](https://img.shields.io/badge/Jira-Project-blue)](https://skillsfundingagency.atlassian.net/secure/RapidBoard.jspa?rapidView=782&projectKey=PV2)
[![Confluence Project](https://img.shields.io/badge/Confluence-Project-blue)](https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/3700621400/Provider+and+Employer+Payments+Payments+BAU)
[![License](https://img.shields.io/badge/license-MIT-lightgrey.svg?longCache=true&style=flat-square)](https://en.wikipedia.org/wiki/MIT_License)


## How It Works

The required payments service is responsible for calculating the how much should be paid for the earning in each period.  It will use the output from the DC earnings calc and also look at previous payments for the same period and determine how much should be paid.  This service will publish calculated levied payment, calculated co-invested payments and calculated incentive payments

More information here: 
- https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/400130049/4.+Payments+v2+-+Components+DAS+Space
- https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/533856326/a.+Apprenticeship+Earning+Event+DAS+Space

## 🚀 Installation

### Pre-Requisites

* An Azure DevBox configured for Payments V2 development

Setup instructions: https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/950927878/Development+Environment+-+Payments+V2+DAS+Space

### Config

As detailed in: https://skillsfundingagency.atlassian.net/wiki/spaces/NDL/pages/644972941/Developer+Configuration+Settings

Select the configuration for the Required Payments application

## 🔗 External Dependencies

N/A

## Technologies

* .NetCore 2.1/3.1/6
* Azure SQL Server
* Azure Functions
* Azure Service Bus
* ServiceFabric

## 🐛 Known Issues

N/A