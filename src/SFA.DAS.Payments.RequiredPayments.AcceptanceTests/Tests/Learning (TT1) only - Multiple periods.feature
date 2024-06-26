﻿Feature: Learning (TT1) only - Multiple periods
R06 - missing submission and historical payments after R02

Background:
	Given the current processing period is 6
	And a learner is undertaking a training with a training provider
	And the payments are for the current collection year
	And the SFA contribution percentage is 90%
	And the earning events component generates the following contract type 2 earnings:	
	| PriceEpisodeIdentifier | Delivery Period | TransactionType | Amount |
	| p1                     | 1               | Learning (TT1)  | 600    |
	| p1                     | 2               | Learning (TT1)  | 600    |
	| p1                     | 3               | Learning (TT1)  | 600    |
	| p1                     | 4               | Learning (TT1)  | 600    |
	| p1                     | 5               | Learning (TT1)  | 600    |
	| p1                     | 6               | Learning (TT1)  | 600    |

	And the following historical contract type 2 payments exist:   
	| PriceEpisodeIdentifier | Delivery Period | TransactionType | Amount |
	| p1                     | 1               | Learning (TT1)  | 600    |
	| p1                     | 2               | Learning (TT1)  | 600    |

@Non-DAS
@Learning(TT1)
@Historical_Payments

Scenario: Contract Type 2 On Programme Learning payments
	When an earning event is received
	Then the required payments component will only generate contract type 2 required payments
	| PriceEpisodeIdentifier | Delivery Period | TransactionType | Amount |
	| p1                     | 3               | Learning (TT1)  | 600    |
	| p1                     | 4               | Learning (TT1)  | 600    |
	| p1                     | 5               | Learning (TT1)  | 600    |
	| p1                     | 6               | Learning (TT1)  | 600    |
