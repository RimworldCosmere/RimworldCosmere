@quickstart:CosmereQuickstart
Feature: investiture steps

  # The holder is the primitive under all three shardworlds, so nothing here names a shard.
  # Every figure the game's own rate decides is asserted as a direction, not a number.

  Scenario: a pawn's investiture round trips through the holder
    Given a colonist "Vessel" exists
    When "Vessel" investiture is set to 0.0
    Then "Vessel" investiture is 0.0
    And "Vessel" investiture is below 0.5
    When "Vessel" investiture is set to 1.0
    Then "Vessel" investiture is 1.0
    And "Vessel" investiture is above 0.5

  @same-world
  Scenario: a recorded reading names the direction the investiture moved
    Given a colonist "Riser" exists
    When "Riser" investiture is set to 0.0
    And I record the investiture of "Riser"
    And "Riser" investiture is set to 1.0
    Then the recorded investiture has risen

  @same-world @timeout:120
  Scenario: a pawn at its cap keeps its investiture while time passes
    Given a colonist "Steady" exists
    When "Steady" investiture is set to 1.0
    Then "Steady" drains 0.0 investiture per second
    When I record the investiture of "Steady"
    And I wait 1000 ticks
    Then the recorded investiture is unchanged
    And "Steady" investiture is 1.0

  @same-world @timeout:120
  Scenario: a gem bleeds investiture as the ticks pass
    When I spawn a "RawDiamond" at (37, 37)
    And the "RawDiamond" at (37, 37) investiture is set to 6.0
    Then the "RawDiamond" at (37, 37) investiture is 6.0
    And the "RawDiamond" at (37, 37) investiture is above 5.0
    When I record the investiture of the "RawDiamond" at (37, 37)
    And I wait 1000 ticks
    Then the recorded investiture has fallen
    And the "RawDiamond" at (37, 37) investiture is below 6.0
