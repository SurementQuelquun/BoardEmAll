# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- Towers should not fire while the tower's `Placement.IsPlaced` is false (ghost state); towers must only shoot after `Placement.IsPlaced == true` or when the `Placement` component is absent.
- TowerCombat should not fire while the tower's `Placement.IsPlaced` is false (ghost state); only shoot when `Placement.IsPlaced == true` or `Placement` component absent.