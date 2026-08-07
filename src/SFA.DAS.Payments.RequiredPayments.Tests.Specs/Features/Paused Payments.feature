Feature: Paused Payments

As an Employer
I want to mark payments for a learner to be paused
So that any new payments for that learner are not made until further notice

Scenario: Earnings received when payments are paused for the learner and course
Given the employer has paused payments for the course
When we receive the earnings for the course for the first time
Then no required payments should be generated

Scenario: Learner completes course but is subsequently withdrawn
Given the provider originally stated the learner started and completed the course
And the employer has subsequently paused payments
When the provider now states that the learner has withdrawn from the course and we received amended earnings
Then the required payments should be generated to process refunds for the previous payments made

Scenario: Change of delivery period and Employer pauses payments for the course
Given the milestone payment was made in a previous collection period
When the employer has paused payments for the course
And we receive the new earnings for the course with the milestone payment made in the current collection period
Then no required payments should be generated