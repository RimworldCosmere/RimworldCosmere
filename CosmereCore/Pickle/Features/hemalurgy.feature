# Hemalurgy is Ruin's, and the quickstart brings Ruin up. The last scenario puts Harmony on
# top without taking Ruin or Preservation down, which is the Ascension and nothing else.
@quickstart:CosmereQuickstart
@timeout:900
Feature: hemalurgy

  Scenario: a steel spike takes a power, and the donor is left drab and less Invested
    Given I enable the shard "Ruin"
    And a colonist "Kelsier" exists
    And a colonist "Marsh" exists
    And I give "Marsh" the gene "Cosmere_Scadrial_Gene_MistingPewter"
    And a "Steel" hemalurgic spike is at (36, 36)
    Then the spike at (36, 36) is not charged
    When "Marsh" investiture is set to 1.0
    And I record the investiture of "Marsh"
    And "Kelsier" drives the spike at (36, 36) through "Marsh", taking the gene "Cosmere_Scadrial_Gene_MistingPewter"
    Then the spike at (36, 36) is charged
    And the spike at (36, 36) holds "Cosmere_Scadrial_Gene_MistingPewter"
    And "Marsh" has no gene "Cosmere_Scadrial_Gene_MistingPewter"
    And the recorded investiture has fallen
    And "Marsh" has hediff "Cosmere_Scadrial_Hediff_Drab"

  # A base metal takes an attribute rather than a power, so nothing has to be named for it.
  @same-world
  Scenario: an iron spike takes raw strength and names no power
    Given a colonist "Ham" exists
    And a "Iron" hemalurgic spike is at (36, 38)
    When "Kelsier" drives the spike at (36, 38) through "Ham"
    Then the spike at (36, 38) is charged
    And the spike at (36, 38) holds "HumanStrength"

  # A ThingFilter cannot see what a spike is made of, so both of these are special filter
  # workers, and both name the spikes a bill must refuse.
  @same-world
  Scenario: the spike filters refuse an empty spike and the wrong metal
    Given a colonist "Sazed" exists
    And a "Copper" hemalurgic spike is at (38, 36)
    Then the special filter "Cosmere_Scadrial_SpecialFilter_SpikeUncharged" rejects the spike at (38, 36)
    And the special filter "Cosmere_Scadrial_SpecialFilter_NotSpikeCopper" allows the spike at (38, 36)
    And the special filter "Cosmere_Scadrial_SpecialFilter_NotSpikeIron" rejects the spike at (38, 36)
    When "Kelsier" drives the spike at (38, 36) through "Sazed"
    Then the spike at (38, 36) is charged
    And the special filter "Cosmere_Scadrial_SpecialFilter_SpikeUncharged" allows the spike at (38, 36)

  # Honor and Ruin hold this cosmere at once here. Duralumin takes Connection, and the only
  # thing that knows what a Radiant's Connection is lives on the other shardworld.
  @same-world
  Scenario: a duralumin spike strains a Radiant's bond through the cross-shard registry
    Given I enable the shard "Honor"
    And I enable the shard "Ruin"
    Then the shard "Honor" is enabled
    And the shard "Ruin" is enabled
    Given a colonist "Shallan" exists
    When I give "Shallan" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    Then "Shallan" has gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Shallan" has no hediff "Cosmere_Roshar_Hediff_StrainedBond"
    Given a "Duralumin" hemalurgic spike is at (38, 38)
    When "Kelsier" drives the spike at (38, 38) through "Shallan"
    Then the spike at (38, 38) is charged
    And the spike at (38, 38) holds "Connection"
    And "Shallan" has hediff "Cosmere_Roshar_Hediff_StrainedBond"

  @same-world
  Scenario: Ruin needs four spikes before it asks, and somebody upright to ask it of
    Given a colonist "Zane" exists
    Then Ruin has no compulsion for 3 spikes
    And Ruin has a compulsion for 4 spikes
    And Ruin has a compulsion for 7 spikes
    And Ruin can move "Zane"
    When I kill "Zane"
    Then Ruin cannot move "Zane"

  # Harmony takes up Ruin and Preservation without either letting go. The plain enable step
  # resolves that overlap away, which is why the Ascension needs one of its own.
  @same-world
  Scenario: after the Ascension the spikes answer to Harmony
    Given I enable the shard "Ruin"
    And I enable the shard "Preservation"
    Then hemalurgy answers to the shard "Ruin"
    Given I enable the shard "Harmony" alongside its conflicts
    Then the shard "Harmony" is enabled
    And the shard "Ruin" is enabled
    And the shard "Preservation" is enabled
    And hemalurgy answers to the shard "Harmony"
