# The metal itself rather than the act: how much it holds, which ones a transfer reaches,
# whose charge a keyed withdrawal may touch, and who a metalmind answers to.
@quickstart:CosmereQuickstart
@timeout:600
Feature: metalminds

  # Quality outweighs the choice of metalmind: a legendary band carries four awful ones.
  Scenario: quality decides how much a metalmind holds
    Given I enable the shard "Preservation"
    And a colonist "Vedlew" exists
    And "Vedlew" carries a "Copper" "Cosmere_Scadrial_Thing_MetalmindBand" of awful quality
    Then the carried "Copper" metalmind of "Vedlew" capacity is 450.0
    Given a colonist "Haddek" exists
    And "Haddek" carries a "Copper" "Cosmere_Scadrial_Thing_MetalmindBand" of legendary quality
    Then the carried "Copper" metalmind of "Haddek" capacity is 1800.0

  @same-world
  Scenario: a transfer aimed at worn metalminds leaves the implant alone
    Given a colonist "Yomen" exists
    And "Yomen" carries a "Steel" "Cosmere_Scadrial_Thing_MetalmindBand"
    And "Yomen" has a "Steel" "Cosmere_Scadrial_Thing_MetalmindImplant" implanted
    And "Yomen" aims feruchemy at external metalminds
    When "Yomen" stores 2000.0 into the "Steel" metalminds
    Then the last transfer moved 900.0
    And the carried "Steel" metalmind of "Yomen" holds 900.0
    And the implanted "Steel" metalmind of "Yomen" holds 0.0

  @same-world
  Scenario: a transfer aimed inside leaves the worn one alone
    Given a colonist "Allrianne" exists
    And "Allrianne" carries a "Steel" "Cosmere_Scadrial_Thing_MetalmindBand"
    And "Allrianne" has a "Steel" "Cosmere_Scadrial_Thing_MetalmindImplant" implanted
    And "Allrianne" aims feruchemy at internal metalminds
    When "Allrianne" stores 2000.0 into the "Steel" metalminds
    Then the last transfer moved 450.0
    And the carried "Steel" metalmind of "Allrianne" holds 0.0
    And the implanted "Steel" metalmind of "Allrianne" holds 450.0

  # Dumping the whole offer into the first metalmind let its own clamp swallow the overflow,
  # so the remainder is carried to the next one instead.
  @same-world
  Scenario: one transfer spills from a full metalmind into the next
    Given a colonist "Straff" exists
    And "Straff" carries a "Bronze" "Cosmere_Scadrial_Thing_MetalmindEarring"
    And "Straff" has a "Bronze" "Cosmere_Scadrial_Thing_MetalmindImplant" implanted
    When "Straff" stores 400.0 into the "Bronze" metalminds
    Then the last transfer moved 400.0
    And the carried "Bronze" metalmind of "Straff" holds 150.0
    And the implanted "Bronze" metalmind of "Straff" holds 250.0

  # A tie to one thing can never be handed back as a tie to another, however much charge
  # the metal holds in total.
  @same-world
  Scenario: a keyed withdrawal cannot reach another key's charge
    Given a colonist "Elariel" exists
    And "Elariel" carries a "Duralumin" "Cosmere_Scadrial_Thing_MetalmindBand"
    When "Elariel" stores 90.0 into the "Duralumin" metalminds under Residence
    And "Elariel" stores 90.0 into the "Duralumin" metalminds under Social
    Then the carried "Duralumin" metalmind of "Elariel" holds 180.0
    And the carried "Duralumin" metalmind of "Elariel" holds 90.0 under Residence
    And the carried "Duralumin" metalmind of "Elariel" holds 90.0 under Social
    When "Elariel" taps 180.0 from the "Duralumin" metalminds under Residence
    Then the last transfer moved 90.0
    And the carried "Duralumin" metalmind of "Elariel" holds 90.0
    And the carried "Duralumin" metalmind of "Elariel" holds 0.0 under Residence
    And the carried "Duralumin" metalmind of "Elariel" holds 90.0 under Social

  # A coppermind full of a childhood has no room left for mental speed.
  @same-world
  Scenario: memories and attribute charge share the same metal
    Given a colonist "Telden" exists
    And "Telden" carries a "Copper" "Cosmere_Scadrial_Thing_MetalmindBracelet"
    When "Telden" files a memory worth -200.0 mood in their "Copper" metalmind
    Then the carried "Copper" metalmind of "Telden" has 250.0 free space
    When "Telden" stores 450.0 into the "Copper" metalminds
    Then the last transfer moved 250.0
    And the carried "Copper" metalmind of "Telden" holds 250.0
    And the carried "Copper" metalmind of "Telden" has 0.0 free space

  # A band answers to whoever filled it. A second pawn may carry it and still draw nothing.
  @same-world
  Scenario: a metalmind keyed to one pawn still offers itself to a second
    Given a colonist "Kelsier" exists
    And a colonist "Dockson" exists
    And "Kelsier" carries a "Pewter" "Cosmere_Scadrial_Thing_MetalmindBand"
    When "Kelsier" stores 400.0 into the "Pewter" metalminds
    And the "Pewter" metalmind of "Kelsier" moves to "Dockson"
    Then the "Pewter" metalmind of "Dockson" is owned by "Kelsier"
    When "Dockson" taps 200.0 from the "Pewter" metalminds
    Then the last transfer moved 0.0
    And the carried "Pewter" metalmind of "Dockson" holds 400.0
    And the carried "Pewter" metalmind of "Dockson" cannot be tapped
