Feature: Holding Back Completion Payments

As a Training Provider
Once a learner has completed their EPA and I have recorded the cokpletion and end date
I would like to receive the final funding for the Apprenticeship

Scenario: Completion payments when history has no reporting funding line type
	Given the Co-invested payments for the apprenticeship were recorded prior to the requirement to record the Reporting Funding Line Type
	When the learner completes the apprenticeship
	Then the service should allow payment of the completion payment
