# Ulef is in no scenario def, so the colonist step generates a fresh pawn rather than handing
# back a crew member whose own connections the colony keeps moving.
@quickstart:PreCatacendreQuickstart
Feature: cosmere shard steps

  # This world settles unpaused, and a pawn's connections grow and decay on their own ticks.
  Background:
    Given game speed is paused

  Scenario: a shard turns on and a pawn takes a connection to it
    Given I enable the shard "Honor"
    Then the shard "Honor" is enabled
    Given a colonist "Ulef" exists
    And "Ulef" xenotype is "Cosmere_Scadrial_Xenotype_Scadrian"
    And "Ulef" has a connection to the shard "Honor" of 50
    Then "Ulef" connection to the shard "Honor" is 50
    And "Ulef" connection to the shard "Honor" is 50 within 5
    And "Ulef" connection tier to the shard "Honor" is Bonded

  # Harmony is Ruin and Preservation held together, so the three never hold at once.
  @same-world
  Scenario: enabling harmony turns off the shards it replaces
    Given I enable the shard "Ruin"
    And I enable the shard "Preservation"
    And I enable the shard "Harmony"
    Then the shard "Harmony" is enabled
    And the shard "Ruin" is disabled
    And the shard "Preservation" is disabled
