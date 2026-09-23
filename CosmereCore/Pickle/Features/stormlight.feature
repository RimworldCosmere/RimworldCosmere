# One Rosharan world for the whole file: building it costs more than the scenarios do.
@quickstart:CosmereQuickstart
@timeout:600
Feature: stormlight

  # Storm waits read against the season's ceiling: a flat 0.7 is out of reach in Winter.
  # Pause only freezes the gaps between steps: the waits call DoSingleTick themselves.
  Background:
    Given I enable the shard "Honor"
    And no storm is running
    And game speed is paused

  # Every pawn here is drafted first: an idle colonist walks off to work, and the open-ground
  # step pins them, because the storm shoves whoever it reaches two cells west per thirty.

  # Reserves start at half: under a fifth a Radiant drinks the spheres around them. The storm
  # pours twenty Stormlight a charge, so the reserve caps out well inside a short window.
  Scenario: a highstorm fills a Radiant standing under open sky
    Given a colonist "Bisig" exists
    And I give "Bisig" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And I draft "Bisig"
    And "Bisig" stands in the open
    And "Bisig" gene "Cosmere_Roshar_Gene_RadiantWindrunner" is set to 50.0
    Then "Bisig" is exposed to the storm
    When I record the investiture of "Bisig"
    And game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    Then "Bisig" is exposed to the storm
    When I wait 300 ticks
    Then the recorded investiture has risen
    And "Bisig" gene "Cosmere_Roshar_Gene_RadiantWindrunner" is above 50.0
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  # Roofed and sealed to the east is what StormShelterManager counts as shelter, and a
  # sheltered pawn never enters the storm's list at all, so no Stormlight reaches them.

  # The one window here that stays long. A sheltered reserve only leaks on its own decay
  # clock, 0.1 every 250 ticks, so shortening this would leave nothing to read.
  @same-world
  Scenario: a roofed Radiant draws nothing from the same storm
    Given a colonist "Peet" exists
    And I give "Peet" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And I draft "Peet"
    And "Peet" shelters from the storm
    And "Peet" gene "Cosmere_Roshar_Gene_RadiantWindrunner" is set to 50.0
    Then "Peet" is sheltered from the storm
    When I record the investiture of "Peet"
    And game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    And I wait 600 ticks
    Then "Peet" is sheltered from the storm
    And the recorded investiture has fallen
    And "Peet" gene "Cosmere_Roshar_Gene_RadiantWindrunner" is below 50.0
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  # Health is read across the storm's peak rather than its whole life. The storm rolls for
  # damage every thirty ticks, and the shorter window is 300 fewer ticks of live colony.
  @same-world
  Scenario: the storm hurts whoever stands out in it
    Given a colonist "Mart" exists
    And I draft "Mart"
    And "Mart" stands in the open
    And I heal "Mart"
    Then "Mart" is exposed to the storm
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    Then "Mart" is exposed to the storm
    When I record the health of "Mart"
    And I wait 300 ticks
    Then the recorded health has fallen
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  # The same peak window as the scenario above, measured against the same storm. Nothing
  # should touch a sheltered pawn, so every extra tick here is only a chance for something to.
  @same-world
  Scenario: a shelter keeps the same storm off
    Given a colonist "Eth" exists
    And I draft "Eth"
    And "Eth" shelters from the storm
    And I heal "Eth"
    Then "Eth" is sheltered from the storm
    When game condition "Cosmere_Roshar_HighstormCondition" starts for 3000 ticks
    And I wait up to 2000 ticks for the highstorm to reach 0.7 of the season's peak intensity
    Then "Eth" is sheltered from the storm
    When I record the health of "Eth"
    And I wait 300 ticks
    Then the recorded health is unchanged
    And "Eth" is sheltered from the storm
    When game condition "Cosmere_Roshar_HighstormCondition" ends
    Then game condition "Cosmere_Roshar_HighstormCondition" is not active

  # BasicLashing declares 2 Stormlight per upkeep charge, and the bond is charged that whole
  # rate at the First Ideal.
  @same-world
  Scenario: a running surge charges the bond its declared rate
    Given a colonist "Beld" exists
    And I draft "Beld"
    And I give "Beld" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Beld" gene "Cosmere_Roshar_Gene_RadiantWindrunner" is set to 80.0
    Then "Beld" surge "Cosmere_Roshar_Ability_BasicLashing" charges nothing
    When "Beld" starts the surge "Cosmere_Roshar_Ability_BasicLashing"
    Then "Beld" surge "Cosmere_Roshar_Ability_BasicLashing" charges 2.0 stormlight
    When I record the investiture of "Beld"
    And I wait 300 ticks
    Then the recorded investiture has fallen
    When "Beld" stops the surge "Cosmere_Roshar_Ability_BasicLashing"
    Then "Beld" surge "Cosmere_Roshar_Ability_BasicLashing" charges nothing

  # The cost halves with every Ideal sworn, so the Second Ideal pays 1 for the same surge.
  @same-world
  Scenario: a sworn ideal halves what the surge costs
    Given a colonist "Yake" exists
    And I draft "Yake"
    And "Yake" is already lighteyed
    And I give "Yake" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    When "Yake" reaches ideal 1 of the radiant order "Windrunner"
    And "Yake" starts the surge "Cosmere_Roshar_Ability_BasicLashing"
    Then "Yake" surge "Cosmere_Roshar_Ability_BasicLashing" charges 1.0 stormlight
    When "Yake" stops the surge "Cosmere_Roshar_Ability_BasicLashing"
    Then "Yake" surge "Cosmere_Roshar_Ability_BasicLashing" charges nothing
