Feature: PV2-4158 GSO withdrawal within the same collection period

Scenario Outline: Short course training delivery that was withdrawn
    Given the following earnings have been received in collection period <collection_period>
    | Delivery Period | Amount | Earning Type |
    | 1               | 300    | Milestone1   |
    | 1               | 700    | Completion   |
    But the provider now reports that the learner actually withdrew from the course prior to completing
	When the following earnings are now generated with a new Earnings Identifier <collection_period>
    | Delivery Period | Amount | Earning Type |
    | 1               | 300    | Milestone1   |
    And the funding transactions are processed
    Then the following required payments should be generated with the new Earnings identifier in collection period <collection_period>
    | Delivery Period | Amount | Earning Type  |
    | 1               | 300    | Milestone1    |


Examples: 
| collection_period |
| 1 |
| 2 |
| 3 |
 
 