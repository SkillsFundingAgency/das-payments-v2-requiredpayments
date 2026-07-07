Feature: PV2-4161 - SFA Funding for Levy Employers With Apprentices


Scenario Outline: Levy employer with insufficient balance - Funded from co-investment - Start date before 1st August - Learner under 25 yrs(regression)
Given a Levy employer with an Apprentice
And the Levy Employer has insufficient balance
And the learning start date is before 1 August 2026 and after 1 Apr 2024
And the learner is aged under 25
And the transaction type is a <transactionType> payment
When the ILR is submitted - Levy
Then the payment funding is split between 'SFA co-investment' (95%) and 'Employer co-investment' (5%)

Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |


Scenario Outline: Levy employer with Insufficient balance - Fully funded from co-investment - Start date on or after 1 Aug 2026 Learner aged under 25 (happy path)
Given a Levy employer with an Apprentice
And the learning start date is on or after 1 August 2026
And the learner is aged under 25 on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted - Levy
Then the payment is fully funded by SFA (100%)

Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |