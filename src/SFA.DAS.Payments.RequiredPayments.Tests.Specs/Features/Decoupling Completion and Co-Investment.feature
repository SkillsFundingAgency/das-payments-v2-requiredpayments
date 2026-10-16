Feature: PV2-4274 Decouple Completion Payments from Co?Investment Collection for Completions on or after 1 August 2026

Feature: PV2-4274 - Decouple Completion Payments from Co-Investment Collection for Completions on or after 1 August 2026

Scenario Outline: Completion payment generated regardless of co-investment status for completions on or after 1 August 2026
    Given a <employerType> employer with an apprentice
    And the apprentice has a completion date on or after 1 August 2026
    And the completion payment is due for payment
    When the completion ILR is submitted
    Then the completion payment is generated
    And the completion payment does not depend on co-investment collection
    And co-investment collection continues independently of completion payment generation

Examples:
| employerType |
| Levy         |
| Non-Levy     |
