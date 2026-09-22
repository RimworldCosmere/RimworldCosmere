# Pickle's own test-colony fixture is a vanilla save, and every pawn in it throws from
# DormantConnection.CompTickInterval every tick. A Cosmere quickstart builds the world instead.
@quickstart:CosmereQuickstart
@timeout:600
Feature: cosmere gene steps

  Scenario: a gene can be granted, checked and taken back
    Given a colonist "Tindwyl" exists
    And "Tindwyl" has no gene "Cosmere_Scadrial_Gene_MistingPewter"
    When I give "Tindwyl" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    Then "Tindwyl" has gene "Cosmere_Scadrial_Gene_MistingPewter"
    When I take the gene "Cosmere_Scadrial_Gene_MistingPewter" from "Tindwyl"
    Then "Tindwyl" has no gene "Cosmere_Scadrial_Gene_MistingPewter"

  @same-world
  Scenario: an invested gene's reserve can be set and read
    Given a colonist "Spook" exists
    And I give "Spook" the gene "Cosmere_Scadrial_Gene_MistingTin"
    When "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is set to 0 percent
    Then "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is 0.0
    When "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is set to 100 percent
    Then "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is above 0.0
    When "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is set to 0.25
    Then "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is 0.25
    And "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is above 0.2
    And "Spook" gene "Cosmere_Scadrial_Gene_MistingTin" is below 0.3

  @same-world
  Scenario: a gene reports the abilities it grants right now
    Given a colonist "Clubs" exists
    And I give "Clubs" the gene "Cosmere_Scadrial_Gene_MistingTin"
    Then "Clubs" gene "Cosmere_Scadrial_Gene_MistingTin" grants ability "Cosmere_Scadrial_Ability_Tin"
    And "Clubs" gene "Cosmere_Scadrial_Gene_MistingTin" does not grant ability "Cosmere_Scadrial_Ability_Pewter"

  # The give step routes a Radiant order through TryAddRadiantOrder, so the Honor gate, the
  # rebond check and the bonded spren all run. A raw EnsureGene would skip all three.
  @same-world
  Scenario: a radiant order is granted through the game's own bond path
    Given a colonist "Kaladin" exists
    When I give "Kaladin" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    Then "Kaladin" has gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Kaladin" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_ToggleShardblade"
