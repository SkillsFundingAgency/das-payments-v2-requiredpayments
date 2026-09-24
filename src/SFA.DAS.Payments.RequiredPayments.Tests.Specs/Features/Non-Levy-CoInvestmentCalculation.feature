Feature: PV2-4063 SFA Funding for Non‑Levy Employers With Apprentices


Scenario Outline: Non-levy employer - Fully funded from co-investment - Start date on or after 1st August - Learner under 22yrs (regression)
Given a Non-levy employer with an Apprentice
And the learning start date is on or after 1 August 2026
And the learner is aged between 16 and 21 on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted
Then the payment is fully funded by SFA (100%)

Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |


Scenario Outline: Non-levy employer - Fully funded from co-investment - Start date on or after 1st August - Learner aged under 25 (happy path)
Given a Non-levy employer with an Apprentice
And the learning start date is on or after 1 August 2026
And the learner is aged between 22 and 24 on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted
Then the payment is fully funded by SFA (100%)

Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |


Scenario Outline: Non-levy employer - Funded from co-investment - Start date before 1st August - Learner 22yrs - 24yrs(regression)
Given a Non-levy employer with an Apprentice
And the learning start date is before 1 August 2026 and after 1 Apr 2024
And the learner is aged between 22 and 24 on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted
Then the payment funding is split between 'SFA co-investment' (95%) and 'Employer co-investment' (5%)

Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |


Scenario Outline: Non-levy employer - Funded from co-investment -- Start date on or after 1st August - Learner 25yrs or over
Given a Non-levy employer with an Apprentice
And the learning start date is on or after 1 August 2026
And the learner is aged 25 or over on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted
Then the payment funding is split between 'SFA co-investment' (95%) and 'Employer co-investment' (5%)
Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |


Scenario Outline: Non-levy employer - Funded from co-investment - Start date before 1st August - Learner 25yrs or over
Given a Non-levy employer with an Apprentice
And the learning start date is before 1 August 2026
And the learner is aged 25 or over on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted
Then the payment funding is split between 'SFA co-investment' (95%) and 'Employer co-investment' (5%)
Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |

Scenario Outline: Change from Levy Employer to Non-Levy Employer  - Start date on after August 1 2026
Given an apprentice changes from a Levy to a Non-Levy employer
And the Levy Employer has insufficient balance
And the learning start date is on or after 1 August 2026
And the learner is aged 25 or over on the start date
And the transaction type is a <transactionType> payment
When the ILR is submitted
Then then two payments are generated for 'Levy' (75%) and 'Non-Levy' (95%)

Examples:
| transactionType |
| Learning        |
| Completion      |
| Balancing       |
