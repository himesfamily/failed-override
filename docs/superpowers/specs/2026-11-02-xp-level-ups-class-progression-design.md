# XP, Level Ups, Roster Selection, and Class Progression Design

## Intent

Make character growth visible during battle while keeping build decisions between missions. Characters should become stronger through use, reserve crew should remain viable, and deploying a character should be required to earn their most distinctive class abilities.

## Terminology

- **XP** is earned from successful combat and useful battlefield actions.
- **Level Ups** are ordinary stat advances earned every 100 XP.
- **Training Path** is the ordinary stat category selected for future Level Ups.
- **Class Progressions** are larger class-specific abilities earned by deploying a character in a successfully completed mission.
- **Deployed character** means one of the four player-character roster slots for that mission. Summoned and reprogrammed machines never consume roster slots.

## Goals

- Show XP progress clearly throughout battle.
- Preserve the existing 25 XP damaging-hit and 50 XP kill rewards.
- Reward healing, technical work, objectives, and other useful actions.
- Credit machine combat to the character who summoned or reprogrammed that machine.
- Keep all upgrade decisions outside battle.
- Let reserve characters gain enough ordinary progression to remain usable.
- Require deployment to earn Class Progressions.
- Roll progression back completely when a mission is failed or restarted.
- Limit normal deployment to four player characters.

## Non-Goals

- No XP for movement, waiting, transformation, or failed actions.
- No independent levels, XP, or upgrade trees for summoned or reprogrammed machines.
- No mid-battle upgrade-choice dialog.
- No permanent injury or lost progression when a deployed character is defeated during an otherwise successful mission.

## XP Awards

The actor receives XP only after a successful action:

| Result | XP |
|---|---:|
| Damaging attack that does not defeat the target | 25 |
| Attack that defeats the target | 50 total |
| Heal an injured teammate | 20 |
| Repair a damaged allied machine | 20 |
| Reprogram an enemy droid | 20 |
| Summon a support robot | 20 |
| Activate a useful mission console | 20 |
| Rescue a captive | 20 |
| Complete an interaction-based objective, such as boarding the shuttle | 20 |

Moving, waiting, transforming, attempting an invalid action, or using an action that produces no useful result awards 0 XP.

### Machine ownership

Each summoned or reprogrammed machine records the character who brought it under player control:

- Akiko owns each robot she summons.
- Issac owns each droid he reprograms.
- Damage and kill XP earned by that machine is awarded to its owner.
- The machine itself never stores XP or levels.
- Ownership remains stable for the rest of the mission, even if the owner is later defeated.

## Level Ups and Training Paths

- Each level requires 100 XP.
- Excess XP carries into the next level.
- Level Ups occur as soon as the threshold is reached.
- The battle displays the level increase immediately, but it never asks the player to make a choice during battle.
- Level 2 and every later Level Up use the character's selected Training Path:
  - **Health:** +4 maximum HP and +4 current HP.
  - **Armor:** +1 armor.
  - **Movement:** +1 movement.
  - **Attack:** +1 damage with every damaging weapon.
- A Training Path can be assigned or changed between missions. Changing it affects future Level Ups only; previous advances remain.
- If a character earns a Level Up in battle, the path selected before deployment is applied immediately.

Level must no longer be treated as an automatic attack modifier. Ordinary stat bonuses are stored independently so Health, Armor, Movement, and Attack paths can diverge.

## Battle Feedback

The selected-character panel shows:

- Level.
- A clearly visible XP bar and numeric progress, such as `65 / 100 XP`.
- Current Training Path.
- A brief `+20 XP`, `+25 XP`, or `+50 XP` message after a reward.

When a threshold is reached, show a prominent but non-interactive **LEVEL UP** presentation after the current action or combat presentation finishes. It states the stat gained, then returns to battle on click. The choice was already made before deployment, so combat remains uninterrupted by build decisions.

## Pre-Mission Crew Screen

Before every battle, show a preparation screen containing all available characters. Each character card shows:

- Portrait, class, level, and XP bar.
- Health, armor, movement, and attack bonuses.
- Current Training Path, with controls to change it.
- Existing Class Progressions and any pending Class Progression choice.
- Whether the character is selected for deployment.

Normal missions allow four deployed player characters. Story-required characters are locked into a roster slot. Summoned robots and reprogrammed droids do not count toward the limit.

- Mission 1 has only Xander, Nance, and Akiko available, so all three deploy.
- Mission 2 has the original three plus Issac, so all four deploy.
- Mission 3 is the first meaningful roster choice: deploy four of the five available crew members.
- Future missions use the same four-character limit unless a mission explicitly overrides it.

The mission cannot begin until all pending Class Progression choices for deployed characters are resolved. Reserve characters may retain pending choices until they are next considered for deployment.

## Reserve Character Catch-Up

On mission victory:

1. Calculate how much XP each deployed player character earned during that mission.
2. Average those XP gains and round to the nearest whole XP.
3. Award that amount to every available character who did not deploy.
4. Apply any resulting reserve Level Ups immediately.

Reserve Level Up rules:

- If the reserve character has a Training Path, every Level Up follows it.
- If no path has ever been assigned, each later Level Up randomly selects Health, Armor, Movement, or Attack.
- Each random result is applied and saved permanently.
- Assigning a path later controls future Level Ups but does not replace prior random advances.

Reserve characters receive no Class Progression for the mission.

## Mission Transactions and Failure

At mission start, preserve a campaign-progression snapshot. Battle XP and Level Ups may be presented immediately, but they are provisional until victory.

On victory:

- Commit deployed-character XP and Level Ups.
- Award and commit reserve catch-up XP and resulting Level Ups.
- Grant one pending Class Progression choice to every deployed character, whether or not that character was defeated before victory.
- Save the campaign.

On defeat or restart:

- Restore XP, levels, and stat advances to the pre-mission snapshot.
- Award no reserve catch-up XP.
- Award no Class Progressions.
- Preserve Training Paths and Class Progressions that were selected before the mission began.

This prevents retry farming while allowing the battle UI to show progression as it happens.

## Class Progressions

Every successfully completed mission grants one Class Progression choice to each deployed character. A character may mix categories. Tier II is offered only after the matching Tier I is owned. Each progression can be selected once.

### Xander McRaven — Scout

- **Marksman I:** Plasma pistol range increases from 3 to 4.
- **Marksman II:** Plasma pistol ignores one point of low-cover protection.
- **Vanguard I:** Vibro-axe gains +2 damage.
- **Vanguard II:** After defeating an enemy in melee, Xander may move up to two spaces.
- **Recon Specialist I:** Xander can reveal hidden enemies within three spaces.
- **Recon Specialist II:** Enemies revealed by Xander suffer -1 armor until the end of the player phase.

### Nance Parker — Shapeshifter

- **Iron Bear I:** Bear form gains +1 armor.
- **Iron Bear II:** Nance recovers 3 HP at the beginning of each player phase while in Bear form.
- **Guardian Bear I:** Adjacent allies gain +1 armor while Nance is in Bear form.
- **Guardian Bear II:** Once per round, Nance can absorb up to 3 damage intended for an adjacent ally.
- **Primal Fury I:** Bear claws gain +2 damage.
- **Primal Fury II:** Defeating an enemy with Bear claws allows Nance to move up to two spaces.

### Akiko Ikeda — Tech

- **Hardened Robots I:** Summoned robots gain +4 HP and +1 armor.
- **Hardened Robots II:** Akiko's active robot limit increases from three to four.
- **Combat Protocols I:** Robot lasers gain +2 damage.
- **Combat Protocols II:** Newly summoned robots can act immediately.
- **Field Engineer I:** Akiko can repair allied machines from two spaces away.
- **Field Engineer II:** Her repair restores 9 HP and removes stun.

### Issac Wynter — Science Officer

- **Override Specialist I:** Issac can reprogram droids at 75% HP or below instead of 50%.
- **Override Specialist II:** A newly reprogrammed droid can act during the same player phase.
- **Master Mechanic I:** Issac's repair increases from 6 to 9 HP.
- **Master Mechanic II:** His repair also removes stun and grants one additional movement point on the target's next turn.
- **Command Protocols I:** Reprogrammed droids gain +1 armor.
- **Command Protocols II:** Reprogrammed droids gain +2 attack damage.

### Dr. Chrandy Perra — Medic

- **Trauma Specialist I:** Field Treatment increases from 7 to 10 HP.
- **Trauma Specialist II:** Healing also removes stun.
- **Prepared Medic I:** Medical supplies increase from three to five per mission.
- **Prepared Medic II:** Chrandy may heal from two spaces away.
- **Combat Anesthetist I:** Stun-pistol range increases from 3 to 4.
- **Combat Anesthetist II:** Chrandy can stun a normally immune enemy once per mission.

## Existing Save Compatibility

Existing campaign saves must continue loading. Existing upgrade fields and their current gameplay effects are preserved as grandfathered legacy rewards. They do not consume a new Class Progression selection, and the new progression collections default to empty when absent from older JSON.

Existing XP and levels remain. Because old saves used `Level - 1` as a universal attack bonus, migration records that existing attack growth as a legacy attack bonus so characters do not become weaker after loading.

New campaigns use the roster, Training Path, Level Up, and Class Progression systems described here. The old fixed post-Mission 1 upgrade screen is replaced by the pre-mission crew screen.

## Architecture

### Considered approaches

1. **Add reward logic directly to every battle action and continue using one-off upgrade strings.** This is quick but spreads progression rules across combat methods, makes rollback fragile, and cannot represent branching prerequisites cleanly.
2. **Centralize progression in campaign-owned character records and use declarative Class Progression definitions.** Battle actions report successful outcomes; the progression layer owns XP, Level Ups, paths, ownership attribution, snapshots, reserve catch-up, prerequisites, and save migration.

Use approach 2. It adds structure now but keeps combat rules, campaign persistence, and the preparation UI consistent as more missions and classes are added.

Each persistent character record should hold level, XP, ordinary stat bonuses, Training Path, selected Class Progression IDs, and pending Class Progression count. Mission-local machine ownership belongs on the machine unit and points to the persistent owner character ID.

## Verification

Automated tests should cover:

- Existing 25/50 attack rewards.
- Every 20 XP utility reward and all excluded actions.
- Machine attack attribution to Akiko or Issac.
- Multiple XP awards crossing a level threshold with carryover.
- Level 2 follows each selected Training Path.
- Each Training Path advance.
- Random reserve advancement only when no path exists.
- Reserve XP average and rounding.
- No reserve Class Progression.
- Victory commit and defeat/restart rollback.
- Four-character roster enforcement and required-character locking.
- Tier II prerequisite enforcement.
- Existing save migration and preservation of legacy power.

Manual verification should cover XP-bar readability, level-up presentation sequencing, preparation-screen usability, roster placement, and loading an existing campaign.
