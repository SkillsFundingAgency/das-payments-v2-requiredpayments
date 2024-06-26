﻿Feature: Provider earnings and payments where learner completes earlier than planned (3 months early) but missing previous submission

Background:

	Given the current processing period is 10

	And a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with training provider 10000

	And the SFA contribution percentage is "90%"

	And the payments due component generates the following contract type 2 payments due:	

	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period | ULN   | TransactionType    | Amount |
	| learnref1      | 10000 | p1                     | 1      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 2      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 3      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 4      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 5      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 6      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 7      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 8      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 9      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 10     | 10000 | Completion (TT2)   | 3000   |
	| learnref1      | 10000 | p1                     | 10     | 10000 | Balancing (TT3)    | 3000   |

	And the following historical contract type 2 On Programme Learning payments exist:

	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period | ULN   | TransactionType    | Amount |
	| learnref1      | 10000 | p1                     | 1      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 2      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 3      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 4      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 5      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 6      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 7      | 10000 | Learning (TT1)		| 1000   |
	| learnref1      | 10000 | p1                     | 8      | 10000 | Learning (TT1)		| 1000   |

	
@Non-DAS
@Completion(TT2)
@Balancing(TT3)
@FinishedEarly
@MissingSubmission

Scenario Outline: Contract Type 2 On Programme Learning payments
	
	When a payments due event is received

	Then the required payments component will generate the following contract type 2 payable earnings:
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period | ULN   | TransactionType    | Amount   |
	| learnref1      | 10000 | p1                     | 9      | 10000 | <transaction_type> | <amount> |

	Examples: 
	| transaction_type | amount |
	| Learning (TT1)   | 1000   |	
	
Scenario Outline: Contract Type 2 On Programme Completion payment

	When a payments due event is received

	Then the required payments component will generate the following contract type 2 Completion (TT2) payable earnings:
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period | ULN   | TransactionType    | Amount   |
	| learnref1      | 10000 | p1                     | 10     | 10000 | <transaction_type> | <amount> |
	
	Examples: 
	| transaction_type | amount |
	| Completion (TT2) | 3000   |
	
	
Scenario Outline: Contract Type 2 On Programme Balancing payment

	When a payments due event is received

	Then the required payments component will generate the following contract type 2 Balancing (TT3) payable earnings:
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period  | ULN   | TransactionType    | Amount   |
	| learnref1      | 10000 | p1                     | 10      | 10000 | <transaction_type> | <amount> |

	Examples: 
	| transaction_type | amount |
	| Balancing (TT3)  | 3000   |	

