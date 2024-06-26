﻿Feature: No history - ALL TT1 TT2 and TT3
Provider earnings and payments where learner completes earlier than planned (3 months early) but missing previous submissions

Background:

	Given the current processing period is 10
	And a learner is undertaking a training with a training provider
	And the payments are for the current collection year
	And the SFA contribution percentage is 90%
	And the earning events component generates the following contract type 2 earnings:	
	| PriceEpisodeIdentifier | Delivery Period | TransactionType  | Amount |
	| p1                     | 1               | Learning (TT1)   | 1000   |
	| p1                     | 2               | Learning (TT1)   | 1000   |
	| p1                     | 3               | Learning (TT1)   | 1000   |
	| p1                     | 4               | Learning (TT1)   | 1000   |
	| p1                     | 5               | Learning (TT1)   | 1000   |
	| p1                     | 6               | Learning (TT1)   | 1000   |
	| p1                     | 7               | Learning (TT1)   | 1000   |
	| p1                     | 8               | Learning (TT1)   | 1000   |
	| p1                     | 9               | Learning (TT1)   | 1000   |
	| p1                     | 10              | Completion (TT2) | 3000   |
	| p1                     | 10              | Balancing (TT3)  | 3000   |
	
@Non-DAS
@Learning(TT1)
@Completion(TT2)
@Balancing(TT3)
@NoHistory

Scenario: Contract Type 2 On Programme Learning payments
	When an earning event is received
	Then the required payments component will only generate contract type 2 required payments
	| PriceEpisodeIdentifier | Delivery Period | TransactionType  | Amount |
	| p1                     | 1               | Learning (TT1)   | 1000   |
	| p1                     | 2               | Learning (TT1)   | 1000   |
	| p1                     | 3               | Learning (TT1)   | 1000   |
	| p1                     | 4               | Learning (TT1)   | 1000   |
	| p1                     | 5               | Learning (TT1)   | 1000   |
	| p1                     | 6               | Learning (TT1)   | 1000   |
	| p1                     | 7               | Learning (TT1)   | 1000   |
	| p1                     | 8               | Learning (TT1)   | 1000   |
	| p1                     | 9               | Learning (TT1)   | 1000   |
	| p1                     | 10              | Completion (TT2) | 3000   |
	| p1                     | 10              | Balancing (TT3)  | 3000   |
