# One world for the whole file. Building it costs more than every scenario here put together.
@quickstart:CosmereQuickstart
@timeout:600
Feature: core investiture

  # The holder is the primitive under all three shardworlds, so nothing here names a shard.
  # Every figure the def's own tuning decides is asserted as a relationship, not a number.

  # The clock runs between steps in both modes, and a diamond spends 0.5 every rare tick. Every
  # scenario asserting an exact figure puts its gems beyond the colony and the clock first.

  Scenario: a gem fills to its own ceiling and stops
    When I spawn a "RawDiamond" at (30, 30)
    And I put the "RawDiamond" at (30, 30) beyond the colony and the clock
    And the "RawDiamond" at (30, 30) investiture is set to 0.0
    And I fill the "RawDiamond" at (30, 30) with investiture
    Then the "RawDiamond" at (30, 30) is full
    And the "RawDiamond" at (30, 30) investiture is above 0.0

  # Nothing is minted and nothing is lost: what leaves one gem is what arrives in the other.
  @same-world
  Scenario: investiture moves between two gems without changing in total
    When I spawn a "RawDiamond" at (32, 30)
    And I spawn a "RawDiamond" at (34, 30)
    And I put the "RawDiamond" at (32, 30) beyond the colony and the clock
    And I put the "RawDiamond" at (34, 30) beyond the colony and the clock
    And the "RawDiamond" at (32, 30) investiture is set to 4.0
    And the "RawDiamond" at (34, 30) investiture is set to 0.0
    And the "RawDiamond" at (34, 30) absorbs 1.5 investiture from the "RawDiamond" at (32, 30)
    Then the "RawDiamond" at (34, 30) investiture is 1.5
    And the "RawDiamond" at (32, 30) investiture is 2.5

  # The ceiling binds the draw, not the ask. A full gem has nowhere to put what it asks for,
  # and a rare tick would open room for the draw this scenario says cannot happen.
  @same-world
  Scenario: a full gem draws nothing and leaves the source alone
    When I spawn a "RawDiamond" at (36, 30)
    And I spawn a "RawDiamond" at (38, 30)
    And I put the "RawDiamond" at (36, 30) beyond the colony and the clock
    And I put the "RawDiamond" at (38, 30) beyond the colony and the clock
    And I fill the "RawDiamond" at (36, 30) with investiture
    And the "RawDiamond" at (38, 30) investiture is set to 3.0
    And the "RawDiamond" at (36, 30) absorbs 2.0 investiture from the "RawDiamond" at (38, 30)
    Then the "RawDiamond" at (36, 30) is full
    And the "RawDiamond" at (38, 30) investiture is 3.0

  # The mirror of a full target: a draw is clamped to what the source actually holds.
  @same-world
  Scenario: an empty gem has nothing to give
    When I spawn a "RawDiamond" at (40, 30)
    And I spawn a "RawDiamond" at (42, 30)
    And I put the "RawDiamond" at (40, 30) beyond the colony and the clock
    And I put the "RawDiamond" at (42, 30) beyond the colony and the clock
    And the "RawDiamond" at (40, 30) investiture is set to 2.0
    And the "RawDiamond" at (42, 30) investiture is set to 0.0
    And the "RawDiamond" at (40, 30) absorbs 1.0 investiture from the "RawDiamond" at (42, 30)
    Then the "RawDiamond" at (40, 30) investiture is 2.0
    And the "RawDiamond" at (42, 30) investiture is 0.0

  # Twelve Radiants will drink a charged gem dry, so it is withheld but keeps its decay: the loss
  # is measured against the rare ticks that actually passed, not the span the step asked for.
  @same-world
  Scenario: a gem bleeds at the rate its own def declares
    When I spawn a "RawDiamond" at (44, 30)
    And I put the "RawDiamond" at (44, 30) beyond the colony's reach
    And I fill the "RawDiamond" at (44, 30) with investiture
    And I record the investiture of the "RawDiamond" at (44, 30)
    Then the "RawDiamond" at (44, 30) loses its declared investiture over 500 ticks
    And the recorded investiture has fallen

  # Withheld for the same reason. Decay stops at empty, so a clock that runs past the wait only
  # spends a floor already there and zero holds however long it runs.
  @same-world
  Scenario: a gem that runs dry stays at zero
    When I spawn a "RawDiamond" at (46, 30)
    And I put the "RawDiamond" at (46, 30) beyond the colony's reach
    And the "RawDiamond" at (46, 30) investiture is set to 0.2
    And I wait 1000 ticks
    Then the "RawDiamond" at (46, 30) investiture is 0.0
