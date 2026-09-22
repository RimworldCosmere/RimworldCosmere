@quickstart:PreCatacendreQuickstart
Feature: cosmere shard steps

  Scenario: a shard turns on and a pawn takes a connection to it
    Given I enable the shard "Honor"
    Then the shard "Honor" is enabled
    Given "Kelsier" has a connection to the shard "Honor" of 50
    Then "Kelsier" connection to the shard "Honor" is 50
    And "Kelsier" connection to the shard "Honor" is 50 within 5
    And "Kelsier" connection tier to the shard "Honor" is Bonded

  # Harmony is Ruin and Preservation held together, so the three never hold at once.
  @same-world
  Scenario: enabling harmony turns off the shards it replaces
    Given I enable the shard "Ruin"
    And I enable the shard "Preservation"
    And I enable the shard "Harmony"
    Then the shard "Harmony" is enabled
    And the shard "Ruin" is disabled
    And the shard "Preservation" is disabled
