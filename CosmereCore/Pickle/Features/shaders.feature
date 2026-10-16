# One Rosharan world for the file: building it costs more than the scenarios do.
@quickstart:CosmereQuickstart
@timeout:600
Feature: the Cosmere shaders draw

  A shader that fails to load falls back to the engine default with a warning rather than an error,
  and no build step compiles a .glsl. These put the things that use one on screen and capture it.

  Background:
    Given I enable the shard "Honor"
    And game speed is paused

  Scenario: Shardplate draws through CutoutAdvanced
    Given a colonist "Kaladin" exists
    And I give "Kaladin" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Kaladin" reaches ideal 2 of the radiant order "Windrunner"
    When "Kaladin" starts the surge "Cosmere_Roshar_Ability_ToggleShardplate"
    Then "Kaladin" is wearing "Cosmere_Roshar_Apparel_RadiantShardplate"
    And "Kaladin" is wearing "Cosmere_Roshar_Apparel_RadiantShardhelm"
    When I move the camera to "Kaladin"
    And I zoom all the way in
    And I take a screenshot "shardplate-cutoutadvanced"

  Scenario: a summoned Shardblade draws through the same shader
    Given a colonist "Szeth" exists
    And I give "Szeth" the gene "Cosmere_Roshar_Gene_RadiantWindrunner"
    And "Szeth" reaches ideal 1 of the radiant order "Windrunner"
    When "Szeth" starts the surge "Cosmere_Roshar_Ability_ToggleShardblade"
    When I move the camera to "Szeth"
    And I zoom all the way in
    And I take a screenshot "shardblade-cutoutadvanced"
