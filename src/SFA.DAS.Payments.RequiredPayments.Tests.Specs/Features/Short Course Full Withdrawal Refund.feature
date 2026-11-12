Feature: PV2-4368 Short course full withdrawal refund keeps the learning dates

As a Training Provider
I want the clawback for a fully withdrawn Apprenticeship Unit to carry the same learning dates as the payment it reverses
So that the clawback is funded from the same funding source as the original payment

Scenario Outline: Fully withdrawn Apprenticeship Unit refund carries the dates from the payment history
	Given an Apprenticeship Unit learner with a <employer_type> employer who started training on 1 September 2026
	And the SFA contribution percentage for the Apprenticeship Unit is <sfa_contribution>
	And the following short course payments were made in a previous collection period
	| Delivery Period | Amount | Earning Type |
	| 2               | 300    | Milestone1   |
	When the provider reports that the learner has been withdrawn with no earnings
	Then the following <required_payment_type> refunds are generated
	| Delivery Period | Amount | Earning Type |
	| 2               | -300   | Milestone1   |
	And each refund carries the start date, planned end date and instalment details of the original payment

Examples:
	| employer_type | sfa_contribution | required_payment_type |
	| Levy          | 100%             | Levy                  |
	| Non-Levy      | 95%              | Co-Invested           |
