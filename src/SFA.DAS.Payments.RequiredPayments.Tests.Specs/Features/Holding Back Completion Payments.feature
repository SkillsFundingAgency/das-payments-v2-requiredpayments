Feature: Holding Back Completion Payments

As a Training Provider
Once a learner has completed their EPA and I have recorded the completion and end date
I would like to receive the final funding for the Apprenticeship

Scenario: Completion payments when history has no reporting funding line type
	Given the Co-invested payments for the apprenticeship were recorded prior to the requirement to record the Reporting Funding Line Type
	When the learner completes the apprenticeship
	Then the service should allow payment of the completion payment

Scenario: Happy Path: Completion payment generated regardless of co‑investment status

Given an employer (levy or non‑levy) with an apprentice
And the apprentice’s completion date is on or after 1 August 2026
And co‑investment may be fully, partially, or not collected
When the ILR is submitted
Then the completion payment is generated
And the completion payment does not depend on co‑investment collection
And co‑investment collection continues independently of completion payment generation

Scenario: Regression scenario: Completion payments before 1 August 2026 remain dependent on co‑investment collection
Given an employer (levy or non‑levy) with an apprentice
And the apprentice’s completion date is before 1 August 2026
And co‑investment is not fully collected
When the ILR is submitted
Then the completion payment is not generated
And the system continues to apply the pre‑August‑2026 rule linking completion payments to co‑investment collection
And completion payments generate only after co‑investment collection completes under the legacy rules

Scenario: Backdating Scenario: Completion payments retrospectively generated for eligible completions from 1 August 2026
Given an employer (levy or non‑levy) with an apprentice
And the apprentice’s completion date is on or after 1 August 2026
And the completion payment was not generated before because co-investment was incomplete
And the system change is deployed after 1 August 2026
When the completion payment process runs after deployment
Then the apprentice is eligible under the new funding rule from 1 August 2026
And the completion payment is generated regardless of co-investment status
And no co-investment proof is needed to generate the payment
And eligible payments held back due to co-investment are released retrospectively
And no duplicate payments are made if already paid