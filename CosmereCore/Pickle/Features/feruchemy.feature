# Feruchemy is Preservation's half of the Metallic Arts, so every scenario here needs a Shard
# holding it. Every transfer runs through MetalmindDistribution, the call the dials make.
@quickstart:CosmereQuickstart
@timeout:600
Feature: feruchemy

  # Nothing here needs time to pass, and a running colony ticks genes, jobs and incidents
  # over the same metalminds. Frozen, the steps are the only thing that moves charge.
  Background:
    Given game speed is paused

  # A metalmind gives back what went in. One rate, one efficiency, no loss in either direction.
  Scenario: a metalmind gives back exactly what was stored
    Given I enable the shard "Preservation"
    And a colonist "Goradel" exists
    And "Goradel" carries a "Steel" "Cosmere_Scadrial_Thing_MetalmindBand"
    When "Goradel" stores 300.0 into the "Steel" metalminds
    Then the last transfer moved 300.0
    And the carried "Steel" metalmind of "Goradel" holds 300.0
    When "Goradel" taps 300.0 from the "Steel" metalminds
    Then the last transfer moved 300.0
    And the carried "Steel" metalmind of "Goradel" holds 0.0
    And the carried "Steel" metalmind of "Goradel" capacity is 900.0

  @same-world
  Scenario: an empty metalmind has nothing to give
    Given a colonist "Milev" exists
    And "Milev" carries a "Pewter" "Cosmere_Scadrial_Thing_MetalmindBracelet"
    Then the carried "Pewter" metalmind of "Milev" cannot be tapped
    When "Milev" taps 100.0 from the "Pewter" metalminds
    Then the last transfer moved 0.0
    And the carried "Pewter" metalmind of "Milev" holds 0.0

  # The offer is clamped to the room left, and the transfer reports the smaller figure, so
  # nothing is accounted for that the metal never took.
  @same-world
  Scenario: storing past capacity fills the metalmind and stops
    Given a colonist "Mennis" exists
    And "Mennis" carries a "Tin" "Cosmere_Scadrial_Thing_MetalmindEarring"
    When "Mennis" stores 500.0 into the "Tin" metalminds
    Then the last transfer moved 150.0
    And the carried "Tin" metalmind of "Mennis" holds 150.0
    And the carried "Tin" metalmind of "Mennis" has 0.0 free space

  # The same 300 charge leaves both bracelets. Only the burn takes the metal with it.
  @same-world
  Scenario: a burn spends the metal, an ordinary tap does not
    Given a colonist "Renoux" exists
    And "Renoux" carries a "Gold" "Cosmere_Scadrial_Thing_MetalmindBracelet"
    And a colonist "Kwaan" exists
    And "Kwaan" carries a "Gold" "Cosmere_Scadrial_Thing_MetalmindBracelet"
    When "Renoux" stores 450.0 into the "Gold" metalminds
    And "Kwaan" stores 450.0 into the "Gold" metalminds
    And "Renoux" taps 300.0 from the "Gold" metalminds
    And "Kwaan" burns 300.0 from the "Gold" metalminds
    Then the carried "Gold" metalmind of "Renoux" holds 150.0
    And the carried "Gold" metalmind of "Renoux" capacity is 450.0
    And the carried "Gold" metalmind of "Kwaan" holds 150.0
    And the carried "Gold" metalmind of "Kwaan" capacity is 150.0

  @same-world
  Scenario: a metalmind burned to nothing is swept away
    Given a colonist "Tevidian" exists
    And "Tevidian" carries a "Iron" "Cosmere_Scadrial_Thing_MetalmindEarring"
    When "Tevidian" stores 150.0 into the "Iron" metalminds
    And "Tevidian" burns 150.0 from the "Iron" metalminds
    Then the last transfer moved 150.0
    And the carried "Iron" metalmind of "Tevidian" capacity is 0.0
    And the carried "Iron" metalmind of "Tevidian" is burned out
    When I sweep burned out metalminds for "Tevidian"
    Then "Tevidian" carries no "Iron" metalmind

  # A worn metalmind will carry compounded charge once it is there, but nothing can compound
  # into it. Only metal inside the body is close enough.
  @same-world
  Scenario: a worn metalmind refuses compounded charge
    Given a colonist "Jastes" exists
    And "Jastes" carries a "Zinc" "Cosmere_Scadrial_Thing_MetalmindBand"
    When "Jastes" compounds 100.0 into the "Zinc" metalminds
    Then the last transfer moved 0.0
    And the carried "Zinc" metalmind of "Jastes" holds 0.0 compounded

  @same-world
  Scenario: compounded charge eats the implant that carries it
    Given a colonist "Cett" exists
    And "Cett" has a "Brass" "Cosmere_Scadrial_Thing_MetalmindImplant" implanted
    When "Cett" compounds 200.0 into the "Brass" metalminds
    Then the last transfer moved 200.0
    And the implanted "Brass" metalmind of "Cett" holds 200.0 compounded
    And the implanted "Brass" metalmind of "Cett" holds 0.0
    And the implanted "Brass" metalmind of "Cett" capacity is 450.0
    When "Cett" burns 200.0 from the "Brass" metalminds
    Then the last transfer moved 200.0
    And the implanted "Brass" metalmind of "Cett" holds 0.0 compounded
    And the implanted "Brass" metalmind of "Cett" capacity is 250.0
    And the implanted "Brass" metalmind of "Cett" is not burned out
