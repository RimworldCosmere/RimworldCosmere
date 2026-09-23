# One Rosharan world for the whole file: building it costs more than the scenarios do.
@quickstart:TrueDesolationQuickstart @same-world
Feature: game condition steps

  # Storms are the subject here, so the clock stays running. What it must not carry in is a
  # storm the storyteller started, which every scenario below would read as its own.
  Background:
    Given no storm is running

  Scenario: a condition starts, reads back, and ends early
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 2000 ticks
    Then game condition "Cosmere_Roshar_HighstormCondition" is active
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  Scenario: a storm that runs its duration out ends on its own
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 600 ticks
    And I wait up to 2000 ticks for game condition "Cosmere_Roshar_HighstormCondition" to end
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  # The storm peaks a third of the way through its life, so 0.4 lands early in a 3000 tick one.
  Scenario: the storm climbs into its dangerous phase
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach intensity 0.4
    Then the highstorm is dangerous
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  Scenario: the storyteller's own highstorm reads the same way
    When incident "Cosmere_Roshar_HighstormIncident" fires
    Then game condition "Cosmere_Roshar_HighstormCondition" is active
    And the weather is "Cosmere_Roshar_HighstormWeather"
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active
