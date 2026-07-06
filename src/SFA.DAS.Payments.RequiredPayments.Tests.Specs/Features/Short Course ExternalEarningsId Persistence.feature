Feature: PV2-4065 Short course external earnings id propagation

Scenario: Published required payments keep the incoming external earnings id
	Given a short course earnings event with an external earnings id
	When the short course earnings event is processed
	Then the published required payment event should have the same external earnings id
