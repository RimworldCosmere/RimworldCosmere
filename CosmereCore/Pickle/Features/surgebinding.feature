# One Rosharan world for the whole file: building it costs more than the scenarios do.
@quickstart:CosmereQuickstart
@timeout:600
Feature: surgebinding

  # Storm waits read against the season's ceiling: a flat 0.7 is out of reach in Winter.
  # Pause only freezes the gaps between steps: the waits call DoSingleTick themselves.
  Background:
    Given I enable the shard "Honor"
    And no storm is running
    And game speed is paused

  # The give step routes a Radiant order through TryAddRadiantOrder, so the Honor gate, the
  # rebond check and the bonded spren all run.
  Scenario: a bond lands through the game's own path
    Given a colonist "Teft" exists
    When I give "Teft" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    Then "Teft" is bonded to the radiant order "Windrunner"
    And "Teft" stored ideal in the radiant order "Windrunner" is 0

  # CurrentIdeal counts from 0 and the display adds one. Reading the wrong one has already
  # shipped a defect here, so both are pinned at the same moment.
  @same-world
  Scenario: a fresh bond stores ideal 0 and reads as the First Ideal
    Given a colonist "Sigzil" exists
    And I give "Sigzil" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    Then "Sigzil" stored ideal in the radiant order "Windrunner" is 0
    And "Sigzil" ideal in the radiant order "Windrunner" reads as 1

  @same-world
  Scenario: the First Ideal already carries a surge, and none of the later gifts
    Given a colonist "Skar" exists
    And I give "Skar" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    Then "Skar" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_BasicLashing"
    And "Skar" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_ToggleShardblade"
    And "Skar" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_ToggleShardplate"
    And "Skar" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_GravitationalPull"

  # The First Ideal lightens a darkeyes' eyes, and the caste swap used to run SetXenotype, whose
  # ClearXenogenes threw the bond away one line after the game granted it.
  @same-world
  Scenario: the eyes lighten and the bond lives through it
    Given a colonist "Lopen" exists
    And "Lopen" is already darkeyed
    And I give "Lopen" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    When "Lopen" reaches ideal 1 of the radiant order "Windrunner"
    Then "Lopen" eyes have lightened
    And "Lopen" is bonded to the radiant order "Windrunner"
    And "Lopen" stored ideal in the radiant order "Windrunner" is 1
    And "Lopen" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_BasicLashing"
    And "Lopen" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_ToggleShardblade"

  # Lighteyed up front: the scenario below is about what each Ideal opens, not about the caste
  # swap the first one triggers, so the caste is pinned instead of rolled.
  @same-world
  Scenario: each ideal sworn opens what the one before it withheld
    Given a colonist "Drehy" exists
    And "Drehy" is already lighteyed
    And I give "Drehy" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    When "Drehy" reaches ideal 1 of the radiant order "Windrunner"
    Then "Drehy" ideal in the radiant order "Windrunner" reads as 2
    And "Drehy" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_ToggleShardblade"
    And "Drehy" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_ToggleShardplate"
    When "Drehy" reaches ideal 2 of the radiant order "Windrunner"
    Then "Drehy" ideal in the radiant order "Windrunner" reads as 3
    And "Drehy" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_ToggleShardplate"
    And "Drehy" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_GravitationalPull"

  @same-world
  Scenario: a broken ideal takes back what it gave
    Given a colonist "Leyten" exists
    And "Leyten" is already lighteyed
    And I give "Leyten" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Leyten" reaches ideal 2 of the radiant order "Windrunner"
    And "Leyten" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_ToggleShardplate"
    When "Leyten" regresses an ideal in the radiant order "Windrunner"
    Then "Leyten" stored ideal in the radiant order "Windrunner" is 1
    And "Leyten" ideal in the radiant order "Windrunner" reads as 2
    And "Leyten" gene "Cosmere_Roshar_Gene_RadiantWindrunner" grants ability "Cosmere_Roshar_Ability_ToggleShardblade"
    And "Leyten" gene "Cosmere_Roshar_Gene_RadiantWindrunner" does not grant ability "Cosmere_Roshar_Ability_ToggleShardplate"

  # The Background puts Honor back before the next scenario runs.
  @same-world
  Scenario: no Honor, no bond
    Given a colonist "Hobber" exists
    And Honor no longer holds this cosmere
    Then the game refuses to bond "Hobber" to the radiant order "Windrunner"
    And "Hobber" is not bonded to the radiant order "Windrunner"

  # A broken bond shuts every order for sixty days, not just the one that broke.
  @same-world
  Scenario: a broken bond closes the spren off
    Given a colonist "Moash" exists
    And "Moash" has broken a bond to the radiant order "Windrunner"
    Then the game refuses to bond "Moash" to the radiant order "Windrunner"
    And the game refuses to bond "Moash" to the radiant order "Skybreaker"
    And "Moash" is not bonded to the radiant order "Windrunner"

  # A called pawn is not tested by the storm. The regard hediff only lands on that branch,
  # so its arrival is the exemption firing.

  # Healed first, because a generated colonist arrives with whatever injuries rolled for it
  # and this one stands through a storm. Ten damage rolls at the peak is plenty for the regard.
  @same-world
  Scenario: the storm spares a pawn the Stormfather has called
    Given a colonist "Dabbid" exists
    And I heal "Dabbid"
    And I draft "Dabbid"
    And "Dabbid" stands in the open
    And "Dabbid" is given hediff "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather"
    Then "Dabbid" is exposed to the storm
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    Then "Dabbid" is exposed to the storm
    When I wait 300 ticks
    Then "Dabbid" has hediff "Cosmere_Roshar_Hediff_StormfathersRegard"
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  # The long window in this file, and it stays long: the objective is surviving a whole storm.
  # Healed first so the survival turns on the storm's damage and not on a rolled bad back.
  @same-world
  Scenario: standing out the whole storm earns the Stormfather's bond
    Given a colonist "Rlain" exists
    And I heal "Rlain"
    And "Rlain" is given hediff "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather"
    And I draft "Rlain"
    And "Rlain" stands in the open
    Then "Rlain" is exposed to the storm
    When the capstone "Cosmere_Roshar_Quest_BondStormfather" is offered to "Rlain"
    Then the capstone "Cosmere_Roshar_Quest_BondStormfather" is NotYetAccepted
    When "Rlain" accepts the capstone "Cosmere_Roshar_Quest_BondStormfather"
    Then the capstone "Cosmere_Roshar_Quest_BondStormfather" is Ongoing
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 2000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    And I wait up to 3000 ticks for game condition "Cosmere_Roshar_HighstormCondition" to end
    And I wait up to 600 ticks for the capstone "Cosmere_Roshar_Quest_BondStormfather" to settle
    Then the capstone "Cosmere_Roshar_Quest_BondStormfather" is EndedSuccess
    And "Rlain" is bonded to the radiant order "Bondsmith"
    And "Rlain" has no hediff "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather"

  # No @same-world on purpose. A capstone only starts from NotFired, and the scenario above
  # leaves this one Offered for the rest of its world, so the failure path needs a fresh one.

  # Healed before he is beaten down, so the storm still blowing over a downed pawn takes him
  # to the failed objective and no further. The last two steps need him alive to read.
  Scenario: going down mid-storm fails the test without burning it
    Given a colonist "Roshone" exists
    And I heal "Roshone"
    And "Roshone" is given hediff "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather"
    And I draft "Roshone"
    And "Roshone" stands in the open
    Then "Roshone" is exposed to the storm
    When the capstone "Cosmere_Roshar_Quest_BondStormfather" is offered to "Roshone"
    And "Roshone" accepts the capstone "Cosmere_Roshar_Quest_BondStormfather"
    And game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    And "Roshone" is beaten down
    And I wait up to 600 ticks for the capstone "Cosmere_Roshar_Quest_BondStormfather" to settle
    Then the capstone "Cosmere_Roshar_Quest_BondStormfather" is EndedFailed
    And the capstone "Cosmere_Roshar_Quest_BondStormfather" is not burned for "Roshone"
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then "Roshone" has hediff "Cosmere_Roshar_Hediff_BondsmithCalling_Stormfather"
    And "Roshone" is not bonded to the radiant order "Bondsmith"
