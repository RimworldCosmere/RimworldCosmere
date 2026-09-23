Feature: cosmere pickle smoke

  # No save is loaded. Def and mod steps run at the main menu, so this stays fast.
  Scenario: the suite and its steps are discovered
    Given mod "Cosmere.Core" is loaded
    Then def "Atium" of type "MetalDef" exists
    And the Cosmere metal "Atium" is loaded
