Feature: PV2-4265 Process GSL Functional Skills Earnings Event for Maths and English Payments

As the Payments Service
I want to process Maths and English earnings received in the GSL Functional Skills Earnings Event
So that the service can generate the required payment events for Maths and English payments.

Scenario: Process Maths and English earnings
Given the Required Payments Service receives a GSL Functional Skills Earnings Event
And the event represents CourseType = Functional Skill
And the event represents LearningType = Maths and English
And the event contains <EarningType> earnings
When the event is processed by the Required Payments 
Then the EarningType earnings should be processed successfully
#And the incoming Maths and English earnings should be mapped to the outgoing Calculated Required Levy Amount message
#And the earning type, amount, academic year and delivery period should match the values received in the incoming event
#And the Calculated Required Levy Amount message should be published for downstream processing.