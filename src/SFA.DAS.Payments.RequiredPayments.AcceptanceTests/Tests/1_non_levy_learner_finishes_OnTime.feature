﻿Feature: Non-Levy learner - Basic Day
@NonDas_BasicDay
Background:
	Given the current collection period is R03
	And a learner is undertaking a training with a training provider
	And the SFA contribution percentage is 90%

	And the payments due component generates the following contract type 2 payments due:	
	| PriceEpisodeIdentifier | Delivery Period	| TransactionType   | Amount	|
	| p2                     | 1 				| Learning (TT1)	| 1000		|
	| p2                     | 2 				| Completion (TT2)	| 3000		|


Scenario: 1_non_levy_learner_finishes_OnTime - with history
	Given the following historical contract type 2 payments exist:
	| PriceEpisodeIdentifier | Delivery Period	| TransactionType   | Amount	|
	| p2                     | 1				| Learning (TT1)	| 1000		|

	When a payments due event is received
	Then the required payments component will generate the following contract type 2 payable earnings:
	| PriceEpisodeIdentifier | Delivery Period	| TransactionType   | Amount	|
	| p2                     | 2 				| Completion (TT2)	| 3000		|


Scenario: 1_non_levy_learner_finishes_OnTime - without history
	When a payments due event is received
	Then the required payments component will generate the following contract type 2 payable earnings:
	| PriceEpisodeIdentifier | Delivery Period	| TransactionType   | Amount	|
	| p2                     | 1 				| Learning (TT1)	| 1000		|
	| p2                     | 2 				| Completion (TT2)	| 3000		|
