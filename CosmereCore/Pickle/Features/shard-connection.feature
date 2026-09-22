# One world for the whole file, and the quickstart already holds Honor, Ruin and Preservation.
# Nothing re-enables a Shard: enabling one mints a new one and orphans every tie to the old.
@quickstart:CosmereQuickstart
@timeout:600
Feature: core shard connection

  # A generated colonist rolls a shardworld xenotype, so nobody starts at nothing. Every
  # scenario pins the xenotype it needs and asserts a direction against a recorded baseline.

  # Pinned Scadrian, so Honor carries no ancestry and the total is the grant and nothing else.
  Scenario: the tiers land on the thresholds the math declares
    Given a colonist "Nomad" exists
    And "Nomad" xenotype is "Cosmere_Scadrial_Xenotype_Scadrian"
    Then "Nomad" connection to the shard "Honor" is 0
    And "Nomad" connection tier to the shard "Honor" is None
    When "Nomad" has a connection to the shard "Honor" of 1
    Then "Nomad" connection tier to the shard "Honor" is Touched
    When "Nomad" has a connection to the shard "Honor" of 30
    Then "Nomad" connection tier to the shard "Honor" is Touched
    When "Nomad" has a connection to the shard "Honor" of 31
    Then "Nomad" connection tier to the shard "Honor" is Bonded
    When "Nomad" has a connection to the shard "Honor" of 70
    Then "Nomad" connection tier to the shard "Honor" is Bonded
    When "Nomad" has a connection to the shard "Honor" of 71
    Then "Nomad" connection tier to the shard "Honor" is Invested
    When "Nomad" has a connection to the shard "Honor" of 99
    Then "Nomad" connection tier to the shard "Honor" is Invested
    When "Nomad" has a connection to the shard "Honor" of 100
    Then "Nomad" connection tier to the shard "Honor" is Ascendant

  # A grant tops the total up, so a pawn already there is left alone rather than pushed past it.
  @same-world
  Scenario: a tie never runs past the top of the scale
    Given a colonist "Ceiling" exists
    And "Ceiling" xenotype is "Cosmere_Scadrial_Xenotype_Scadrian"
    When "Ceiling" has a connection to the shard "Honor" of 100
    And "Ceiling" has a connection to the shard "Honor" of 100
    Then "Ceiling" connection to the shard "Honor" is 100
    And "Ceiling" connection tier to the shard "Honor" is Ascendant

  # The breakdown is what the player reads. A source it leaves out is a number nobody can explain.
  @same-world
  Scenario: the parts of a tie rebuild the total the game acts on
    Given a colonist "Ledger" exists
    And "Ledger" xenotype is "Cosmere_Scadrial_Xenotype_Scadrian"
    Then "Ledger" connection breakdown to the shard "Preservation" adds up
    When "Ledger" has a connection to the shard "Preservation" of 80
    And I give "Ledger" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    Then "Ledger" connection breakdown to the shard "Preservation" adds up
    And "Ledger" connection to the shard "Preservation" is above 40

  # Allomancy is Scadrial's. Carrying it teaches a pawn nothing about Honor.
  @same-world
  Scenario: investiture stays in its own shard's lane
    Given a colonist "Tin" exists
    And "Tin" xenotype is "Cosmere_Scadrial_Xenotype_Scadrian"
    Then "Tin" connection to the shard "Honor" is 0
    When I record "Tin" connection to the shard "Honor"
    And I give "Tin" the gene "Cosmere_Scadrial_Gene_MistingTin"
    Then "Tin" connection to the shard "Honor" has not moved
    And "Tin" connection to the shard "Preservation" is above 0
    And "Tin" connection breakdown to the shard "Preservation" adds up

  # Lerasium is the exception on purpose: it is how an unconnected person becomes Connected.
  @same-world
  Scenario: a god metal refuses anyone not tied deeply enough to its shard
    Given a colonist "Demoux" exists
    And "Demoux" xenotype is "Cosmere_Roshar_Xenotype_Darkeyes"
    Then "Demoux" connection to the shard "Ruin" is below 30
    And "Demoux" may not burn the god metal "Atium"
    And "Demoux" may burn the god metal "Lerasium"
    When "Demoux" has a connection to the shard "Ruin" of 29
    Then "Demoux" may not burn the god metal "Atium"
    When "Demoux" has a connection to the shard "Ruin" of 30
    Then "Demoux" may burn the god metal "Atium"

  # Ancestry is recomputed, never stored, so changing who a pawn is changes both ties at once.
  @same-world
  Scenario: ancestry follows the world a pawn was born to
    Given a colonist "Elend" exists
    And "Elend" xenotype is "Cosmere_Roshar_Xenotype_Darkeyes"
    Then "Elend" may not burn the god metal "Atium"
    When I record "Elend" connection to the shard "Ruin"
    And I record "Elend" connection to the shard "Honor"
    And "Elend" xenotype is "Cosmere_Scadrial_Xenotype_Scadrian"
    Then "Elend" connection to the shard "Ruin" has risen
    And "Elend" connection to the shard "Honor" has fallen
    And "Elend" may burn the god metal "Atium"
    And "Elend" connection breakdown to the shard "Ruin" adds up

  # Half of Harmony is not Harmony: it reads as the weaker of Ruin and Preservation.
  @same-world
  Scenario: a tie to harmony is only as deep as the shallower half
    Given a colonist "Sazed" exists
    And "Sazed" xenotype is "Cosmere_Roshar_Xenotype_Darkeyes"
    When I record "Sazed" connection to the shard "Harmony"
    And "Sazed" has a connection to the shard "Ruin" of 60
    Then "Sazed" connection to the shard "Harmony" has not moved
    When "Sazed" has a connection to the shard "Preservation" of 60
    Then "Sazed" connection to the shard "Harmony" has risen
    And "Sazed" connection breakdown to the shard "Harmony" adds up

  # Moves the save onto Scadrial, because only a real shardworld naturalises anybody. Last in
  # the file for that reason: every scenario above wants the cross-world sentinel.
  @same-world
  Scenario: living somewhere long enough earns what being born there would have given
    Given the save's world is "Scadrial"
    And a colonist "Sarene" exists
    And "Sarene" xenotype is "Cosmere_Roshar_Xenotype_Darkeyes"
    Then "Sarene" may not burn the god metal "Atium"
    When I record "Sarene" connection to the shard "Preservation"
    And I record "Sarene" connection to the shard "Honor"
    And "Sarene" has lived on this world for 10 years
    Then "Sarene" connection to the shard "Preservation" has risen
    And "Sarene" connection to the shard "Honor" has not moved
    And "Sarene" may burn the god metal "Atium"
    And "Sarene" connection breakdown to the shard "Preservation" adds up
    When I record "Sarene" connection to the shard "Preservation"
    And "Sarene" has lived on this world for 0 years
    Then "Sarene" connection to the shard "Preservation" has fallen
