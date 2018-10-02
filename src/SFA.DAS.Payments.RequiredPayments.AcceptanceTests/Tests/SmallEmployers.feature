﻿Feature: non-DAS learner employed with a small employer, is fully funded for on programme and completion payments
#Non-DAS learner employed with a small employer, is fully funded for on programme and completion payments

Background:
	Given the current collection period is R13
	And a learner with LearnRefNumber learnref1 and Uln 10000 undertaking training with training provider 10000
	And the SFA contribution percentage is 100%
	And the payments due component generates the following contract type 2 payments due:	
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Delivery Period | ULN   | TransactionType   | Amount	|
	| learnref1      | 10000 | p1                     | 1				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 2				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 3				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 4				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 5				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 6				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 7				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 8				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 9				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 10				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 11				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 12				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 12				| 10000 | Completion (TT2)	| 1500		|

@SmallEmployerNonDas

Scenario: AC1-Payment for a 16-18 non-DAS learner, small employer at start
	When a payments due event is received
	Then the required payments component will generate the following contract type 2 payable earnings:
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Delivery Period | ULN   | TransactionType   | Amount	|
	| learnref1      | 10000 | p1                     | 1				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 2				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 3				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 4				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 5				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 6				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 7				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 8				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 9				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 10				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 11				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 12				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 12				| 10000 | Completion (TT2)	| 1500		|

@SmallEmployerNonDas

Scenario: AC5- Payment for a 16-18 non-DAS learner, employer is not small
	When a payments due event is received
	Then the required payments component will generate the following contract type 2 payable earnings:
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Delivery Period | ULN   | TransactionType   | Amount	|
	| learnref1      | 10000 | p1                     | 1				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 2				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 3				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 4				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 5				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 6				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 7				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 8				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 9				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 10				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 11				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 12				| 10000 | Learning (TT1)	| 500		|
	| learnref1      | 10000 | p1                     | 12				| 10000 | Completion (TT2)	| 1500		|