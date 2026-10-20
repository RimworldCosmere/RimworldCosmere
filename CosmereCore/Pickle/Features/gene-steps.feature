# Pickle's own test-colony fixture is a vanilla save, and every pawn in it throws from
# DormantConnection.CompTickInterval every tick. A Cosmere quickstart builds the world instead.
@quickstart:CosmereQuickstart
@timeout:600
Feature: cosmere gene steps

  # The colony settles unpaused and a pawn ticks its own gene reserves; frozen, only steps move them.
  # No name here is in a scenario def, so each generates a fresh pawn instead of binding a crew member.
  Background:
    Given game speed is paused

  Scenario: a gene can be granted, checked and taken back
    Given a colonist "Tepper" exists
    And "Tepper" has no gene "Cosmere_Scadrial_Gene_MistingPewter"
    When I give "Tepper" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    Then "Tepper" has gene "Cosmere_Scadrial_Gene_MistingPewter"
    When I take the gene "Cosmere_Scadrial_Gene_MistingPewter" from "Tepper"
    Then "Tepper" has no gene "Cosmere_Scadrial_Gene_MistingPewter"

  @same-world
  Scenario: an invested gene's reserve can be set and read
    Given a colonist "Dedra" exists
    And I give "Dedra" the gene "Cosmere_Scadrial_Gene_MistingTin"
    When "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is set to 0 percent
    Then "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is 0.0
    When "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is set to 100 percent
    Then "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is above 0.0
    When "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is set to 0.25
    Then "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is 0.25
    And "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is above 0.2
    And "Dedra" gene "Cosmere_Scadrial_Gene_MistingTin" is below 0.3

  @same-world
  Scenario: a gene reports the abilities it grants right now
    Given a colonist "Goshel" exists
    And I give "Goshel" the gene "Cosmere_Scadrial_Gene_MistingTin"
    Then "Goshel" gene "Cosmere_Scadrial_Gene_MistingTin" grants ability "Cosmere_Scadrial_Ability_Tin"
    And "Goshel" gene "Cosmere_Scadrial_Gene_MistingTin" does not grant ability "Cosmere_Scadrial_Ability_Pewter"

  # The give step routes a Radiant order through TryAddRadiantOrder, so the Honor gate, the
  # rebond check and the bonded spren all run. A raw EnsureGene would skip all three.
  @same-world
  Scenario: a radiant order is granted through the game's own bond path
    Given a colonist "Punio" exists
    When I give "Punio" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    Then "Punio" has gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Punio" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_ToggleShardblade"
