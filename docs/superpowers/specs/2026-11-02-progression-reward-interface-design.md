# Progression and Mission Reward Interface Design

## Intent

Make advancement feel earned and understandable. Players should see experience rise at the moment a character does something useful, receive automatic Level Up benefits without interrupting battle, and then celebrate victory by choosing a separate Class Progression reward after the mission.

## Goals

- Replace the old post-Mission-1 upgrade-button sequence.
- Keep a character's XP progress visible during battle.
- Animate XP gain immediately after a successful qualifying action.
- Apply every in-battle Level Up and its stat increase immediately.
- Let players choose or change each character's Training Path between missions.
- Let deployed characters choose one earned Class Progression after mission victory.
- Show current stats and the exact effect of a choice before confirmation.
- Use the full stat names **Health**, **Armor**, **Movement**, and **Attack** throughout the progression interface.

## Non-Goals

- Training Path choices do not delay a Level Up already earned in battle.
- Class Progression choices never interrupt battle.
- Training Paths and Class Progressions are separate decisions; one does not automatically determine the other.
- This change does not redesign combat rules or XP award amounts.

## Approved Approach

Use an integrated character-card design. The compact battle card and expanded post-mission reward card share the same visual language, stat order, XP bar, portrait treatment, and terminology. After victory, a short team summary leads into one expanded reward card for each deployed character.

### Alternatives considered

1. **Integrated character card — selected.** Strong continuity between earning XP and spending rewards, with the least terminology duplication.
2. **Separate battle and reward designs.** More layout freedom, but makes the two systems feel disconnected.
3. **Full mission-results screen before rewards.** More ceremonial, but adds an unnecessary extra step. The selected approach retains only a brief team summary.

## Battle Character Card

The selected-character panel shows:

- Portrait, name, class, and level.
- Health, Armor, Movement, and Attack, written in full.
- Current Training Path.
- A prominent XP bar and numeric value such as `72 / 100 XP`.
- Existing turn-status information.

When a successful action awards XP:

1. The normal action or combat presentation finishes.
2. A short `+20 XP`, `+25 XP`, or `+50 XP` label appears beside the bar with a plain-language reason.
3. The XP bar animates from its previous value to its new value.
4. If the bar crosses 100, it fills, the Level Up applies immediately, and the bar continues from zero with carryover XP.
5. A click-to-dismiss **LEVEL UP** presentation states the full stat name and increase, for example `Health +4` or `Attack +1`.

Multiple Level Ups or awards queue in order. The player never makes a build choice during this sequence.

## Training Paths

Training Path is displayed on the battle card but can be changed only between missions. The post-mission card offers:

- **Health:** future Level Ups grant +4 maximum and current Health.
- **Armor:** future Level Ups grant +1 Armor.
- **Movement:** future Level Ups grant +1 Movement.
- **Attack:** future Level Ups grant +1 damage with damaging weapons.

Level 2 and every later Level Up use the selected Training Path. Changing a path affects only future Level Ups and never rewrites earlier gains. The current choice is visibly selected, and changing it previews the future benefit before saving.

If a character has no Training Path, selecting one is required before their post-mission reward card can be confirmed. Training Paths remain editable from the pre-mission crew-selection screen before every mission.

## Post-Mission Reward Flow

On mission victory:

1. Show a brief team summary listing XP earned and any Level Ups gained during the mission.
2. Present one expanded reward card at a time for each deployed character.
3. The card shows portrait, class, level, XP bar, full current stats, stat bonuses, current Training Path, and existing Class Progressions.
4. The player may retain or change the Training Path.
5. The player separately chooses one available Class Progression earned from the completed mission.
6. Selecting a Class Progression shows its exact gameplay effect and any before-and-after values before confirmation.
7. Confirming advances to the next deployed character.
8. Finish with a roster summary and options to continue, replay, select another mission, or return to the landing screen.

The player cannot begin the next mission while a deployed character has an unresolved Class Progression reward. Reserve characters may keep unresolved rewards until later, as already defined by the campaign progression rules.

## Pre-Mission Crew Review

When choosing the four characters for a mission, the player can open each character's integrated card to:

- Review current level, XP, full stats, Training Path, and acquired Class Progressions.
- Change the Training Path for future Level Ups.
- Resolve any previously earned Class Progression choice that is still pending.
- Review acquired Class Progressions without replacing or removing them.

Previously selected Class Progressions are permanent. The pre-mission screen does not provide respecification; it only resolves pending choices and changes Training Paths.

## Replaying Completed Missions

Permanent mission credit is tracked per character, not only per campaign:

- A character earns permanent XP and one Class Progression from a particular mission only the first time that character completes it.
- Replaying with a character who has not completed that mission grants that character normal permanent XP and a Class Progression reward.
- A character who already has credit may participate, but XP and Level Ups earned during the replay are temporary and are rolled back after the mission.
- Reserve catch-up from a replay applies only to eligible characters who have not already received credit for that mission.
- The reward summary clearly distinguishes newly rewarded characters from veterans who already completed the mission.

This prevents progression farming while allowing new crew members to catch up by joining earlier missions.

## Reward Presentation

The screen should feel celebratory rather than administrative:

- Gold-accented `Class Progression Earned` treatment.
- Character portrait and name remain the visual focus.
- Choice descriptions emphasize what becomes newly possible.
- Confirmation uses the character's name, such as `Confirm Xander's Reward`.
- Existing artwork and the game's dark blue, cyan, and gold palette remain in use.

## Save and Failure Behavior

- Battle XP and Level Ups remain provisional until mission victory under the existing mission transaction rules.
- Defeat or restart restores the pre-mission progression snapshot.
- Training Path changes and Class Progression selections are saved after confirmation between missions.
- Existing saves retain legacy upgrades and receive safe defaults for missing interface state.

## Verification

Automated checks should cover XP animation state, carryover, queued Level Up notices, Training Path changes affecting only future levels, separate Class Progression selection, unresolved-choice blocking, per-character mission credit, mixed veteran/new replay rosters, replay rollback, and save/load behavior.

Manual testing should confirm that the XP animation is noticeable but not disruptive, full stat names fit at supported window sizes, reward effects are readable, every deployed character receives a reward card, and the old upgrade-button sequence no longer appears.
