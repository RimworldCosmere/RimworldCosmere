# Preservation and Ruin both hold Scadrial before the Catacendre, and allomancy answers to
# either. The quickstart brings both up; the first scenario names one anyway so the gate is read.
@quickstart:CosmereQuickstart
@timeout:900
Feature: allomancy

  # A running colony hauls the loose metal the last scenario reads off the ground. Frozen, the
  # only ticks that pass are the ones a wait step drives itself, which is what these burns need.
  Background:
    Given game speed is paused

  Scenario: a burning metal spends its reserve and hangs the effect its source declares
    Given I enable the shard "Preservation"
    And a colonist "Spook" exists
    And I give "Spook" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    When "Spook" gene "Cosmere_Scadrial_Gene_MistingPewter" is set to 0.5
    Then "Spook" can burn "Cosmere_Scadrial_Ability_Pewter"
    When "Spook" starts burning "Cosmere_Scadrial_Ability_Pewter"
    Then "Spook" is burning "Cosmere_Scadrial_Ability_Pewter"
    And "Spook" burn power for "Cosmere_Scadrial_Ability_Pewter" is 1
    And "Spook" has hediff "Cosmere_Scadrial_Hediff_PewterBuff"
    When I wait 600 ticks
    Then "Spook" gene "Cosmere_Scadrial_Gene_MistingPewter" is below 0.5
    When "Spook" stops burning "Cosmere_Scadrial_Ability_Pewter"
    Then "Spook" is not burning "Cosmere_Scadrial_Ability_Pewter"
    And "Spook" has no hediff "Cosmere_Scadrial_Hediff_PewterBuff"

  # Autocast seeds a pewter rule that puts the burn out while the pawn is undrafted. It used to
  # release any burn it found, so a colonist who lit pewter by hand lost it inside sixty ticks.
  @same-world
  Scenario: autocast leaves a burn the colonist lit by hand alone
    Given a colonist "Allrianne" exists
    And I give "Allrianne" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    When I undraft "Allrianne"
    And "Allrianne" gene "Cosmere_Scadrial_Gene_MistingPewter" is set to 0.5
    And "Allrianne" starts burning "Cosmere_Scadrial_Ability_Pewter"
    And I wait 180 ticks
    Then autocast has passed over "Allrianne" without taking hold of "Cosmere_Scadrial_Ability_Pewter"
    And "Allrianne" is burning "Cosmere_Scadrial_Ability_Pewter"
    And "Allrianne" burn power for "Cosmere_Scadrial_Ability_Pewter" is 1
    And "Allrianne" has hediff "Cosmere_Scadrial_Hediff_PewterBuff"
    And "Allrianne" gene "Cosmere_Scadrial_Gene_MistingPewter" is below 0.5

  # Lighting a dry metal on purpose is the point. The gene has to be the thing that notices,
  # the way it does when a burn outlives its vial.
  @same-world
  Scenario: an empty reserve refuses the burn, and the gene puts out one that was forced
    Given a colonist "Clubs" exists
    And I give "Clubs" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    When "Clubs" gene "Cosmere_Scadrial_Gene_MistingPewter" is set to 0.0
    Then "Clubs" cannot burn "Cosmere_Scadrial_Ability_Pewter"
    When "Clubs" starts burning "Cosmere_Scadrial_Ability_Pewter"
    And I wait 600 ticks
    Then "Clubs" is not burning "Cosmere_Scadrial_Ability_Pewter"
    And "Clubs" has no hediff "Cosmere_Scadrial_Hediff_PewterBuff"

  @same-world
  Scenario: duralumin throws the metals beside it to full, then burns every reserve away
    Given a colonist "Vin" exists
    And I give "Vin" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    And I give "Vin" the gene "Cosmere_Scadrial_Gene_MistingDuralumin"
    When "Vin" gene "Cosmere_Scadrial_Gene_MistingPewter" is set to 0.5
    And "Vin" gene "Cosmere_Scadrial_Gene_MistingDuralumin" is set to 0.5
    And "Vin" starts burning "Cosmere_Scadrial_Ability_Pewter"
    And "Vin" starts burning "Cosmere_Scadrial_Ability_Duralumin"
    Then "Vin" has hediff "Cosmere_Scadrial_Hediff_SurgeCharge"
    When "Vin" triggers their supercharge
    Then "Vin" burn power for "Cosmere_Scadrial_Ability_Pewter" is 10
    And "Vin" burn power for "Cosmere_Scadrial_Ability_Duralumin" is 1
    When "Vin" spends their supercharge
    Then "Vin" gene "Cosmere_Scadrial_Gene_MistingPewter" is 0.0
    And "Vin" gene "Cosmere_Scadrial_Gene_MistingDuralumin" is 0.0
    And "Vin" is not burning "Cosmere_Scadrial_Ability_Duralumin"
    When I wait 600 ticks
    Then "Vin" is not burning "Cosmere_Scadrial_Ability_Pewter"

  # Same flash, other direction: nicrosil lifts somebody else's burn and the nicrosil burner is
  # the one left with nothing.
  @same-world
  Scenario: nicrosil throws somebody else's burn up and the one who lit it pays for it
    Given a colonist "Elend" exists
    And a colonist "Breeze" exists
    And I give "Elend" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    And I give "Breeze" the gene "Cosmere_Scadrial_Gene_MistingNicrosil"
    When "Elend" gene "Cosmere_Scadrial_Gene_MistingPewter" is set to 0.5
    And "Breeze" gene "Cosmere_Scadrial_Gene_MistingNicrosil" is set to 0.5
    And "Elend" starts burning "Cosmere_Scadrial_Ability_Pewter"
    And "Breeze" supercharges "Elend"
    Then "Elend" has hediff "Cosmere_Scadrial_Hediff_SurgeCharge"
    And "Breeze" has no hediff "Cosmere_Scadrial_Hediff_SurgeCharge"
    When "Elend" triggers their supercharge
    Then "Elend" burn power for "Cosmere_Scadrial_Ability_Pewter" is 10
    When "Elend" spends their supercharge
    Then "Elend" gene "Cosmere_Scadrial_Gene_MistingPewter" is 0.0
    And "Breeze" gene "Cosmere_Scadrial_Gene_MistingNicrosil" is 0.0
    And "Breeze" is not burning "Cosmere_Scadrial_Ability_Nicrosil"

  # A bolted-down building reads heavier than the Allomancer shoving off it, so the shove
  # sends the person. Wood has nothing to grasp at all.
  @same-world
  Scenario: a steelpush sends what is loose and throws the Allomancer off what is bolted down
    Given a colonist "Wax" exists
    And I give "Wax" the gene "Cosmere_Scadrial_Gene_MistingSteel"
    When "Wax" gene "Cosmere_Scadrial_Gene_MistingSteel" is set to 0.5
    And I spawn a "Steel" at (36, 36)
    And I spawn a "WoodLog" at (36, 38)
    And a "ToolCabinet" is built at (50, 50)
    Then "Wax" burning "Cosmere_Scadrial_Ability_SteelPush" can grasp the "Steel" at (36, 36)
    And "Wax" burning "Cosmere_Scadrial_Ability_SteelPush" cannot grasp the "WoodLog" at (36, 38)
    And "Wax" burning "Cosmere_Scadrial_Ability_SteelPush" shoves the "Steel" at (36, 36)
    And "Wax" burning "Cosmere_Scadrial_Ability_SteelPush" is thrown off the "ToolCabinet" at (50, 50)
