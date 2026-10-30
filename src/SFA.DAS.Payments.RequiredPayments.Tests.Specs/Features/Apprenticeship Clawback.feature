Feature: PV2-4358 Resolution: Apprenticeship Clawbacks Following Learner Removal

Scenario Outline: Generate clawbacks when a learner is removed
	Given earnings were submitted for an Apprenticeship learner in a previous collection period with funding platform type of <fundingPlatformType>
	And payments were generated from those earnings 
	And the learner is subsequently removed from the source data 
	And an empty earnings message is received for that learner in a later collection period 
	When the payment system handles the empty earnings message 
	Then clawback transactions should be generated to reverse the eligible payments made from the previously submitted earnings 
	And the clawback amounts should match the payments being reversed

Examples:
| fundingPlatformType |
| SubmitLearnerData   |
| DigitalApprenticeshipService |
