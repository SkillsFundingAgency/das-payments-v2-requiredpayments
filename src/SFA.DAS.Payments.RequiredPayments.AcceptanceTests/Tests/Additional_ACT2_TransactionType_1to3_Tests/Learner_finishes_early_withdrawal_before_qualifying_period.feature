﻿#WARNING: This test needs reviewing
#what is the qualified period for withdrawal, will we not pay anything if they withdraw earlier

Feature: A non-DAS learner, learner withdraws before qualifying period
#Learner withdraws before 42 days and shouldn't be paid for 1st month


Background:
	Given the current processing period is 2

	And the following learners:
	| LearnRefNumber | Ukprn | ULN   |
	| learnref1      | 10000 | 10000 |

	And the following course information:
	| AimSeqNumber | ProgrammeType | FrameworkCode | PathwayCode | StandardCode | FundingLineType                                                       | LearnAimRef | LearningStartDate | LearningPlannedEndDate | LearningActualEndDate | CompletionStatus |
	| 1            | 2             | 403           | 1           |              | 16-18 Apprenticeship (From May 2017) Non-Levy Contract (non-procured) | ZPROG001    | 01/08/2017		  | 08/08/2018             | 08/09/2017			   | withdrawn        |

	And the following contract type 2 on programme earnings for periods 1-12 are provided in the latest ILR for the academic year 1718:
	| PriceEpisodeIdentifier | EpisodeStartDate | EpisodeEffectiveTNPStartDate | TotalNegotiatedPrice | Learning_1 |
	| p1                     | 06/08/2017       | 06/08/2017                   | 15000                | 1000       |

@Non-DAS
@Learner_finishes_early
@Withdrawal
@query
@minimum_additional
@review
@refund

Scenario Outline: Contract Type 2 On programme payments

	And the following historical contract type 2 on programme payments exist:   
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period | ULN   | TransactionType    | Amount   |
	| learnref1        | 10000 | p1                   | 1      | 10000 | <transaction_type> | <amount> |

	When a TOBY is received

	Then the payments due component will generate the following contract type 2 payable earnings:
	| LearnRefNumber | Ukprn | PriceEpisodeIdentifier | Period | ULN   | TransactionType    | Amount   |
	| learnref1      | 10000 | p1                     | 1      | 10000 | <transaction_type> | <amount> |
	| learnref1      | 10000 | p1                     | 1      | 10000 | <transaction_type> | -<amount> |
		
	Examples: 
	| transaction_type | amount |
	| Learning_1       | 1000   |