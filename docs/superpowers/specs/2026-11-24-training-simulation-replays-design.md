# Training Simulation Replays

## Intent

Completed campaign missions can be replayed as virtual training simulations. Replays let the player experiment with any currently unlocked named crew, earn real experience, Level Ups, and the Class Progressions already defined in the game, without rewriting the campaign story.

The simulation framing intentionally permits story contradictions, such as Issac participating in the Mission 1 rescue while the scenario contains a simulated captive Issac.

## Goals

- After a campaign victory, offer a clear choice between advancing the story and opening the training simulator.
- Allow the player to select any completed mission and then choose up to four currently unlocked named crew members.
- Require exactly four crew members for a training replay, matching Mission 3's crew-selection experience.
- Award participating named crew full battlefield XP and immediate Level Ups.
- Award each participating named crew member one pending Class Progression after a successful simulation, but only while that character has an obtainable progression in the current catalog.
- Give no XP, catch-up XP, Level Ups, Class Progressions, or bond growth to nonparticipants in either campaign missions or simulations.
- Restore all progression to its pre-mission state after a failed simulation.
- Skip intros, wrap-ups, and the Mission 1 tutorial during simulations.
- Make the simulation state obvious on mission selection, preparation, and the battle HUD.

## Non-goals

- Adding new Class Progression branches or higher tiers.
- Allowing simulation victories to unlock campaign missions or replace the canonical first completion.
- Rewriting mission maps, objectives, dialogue, or captive identities.
- Adding XP scaling, difficulty scaling, or an XP penalty for repeated play.

## Player Flow

### Campaign victory

After rewards are confirmed, a first-time campaign victory presents:

1. **Advance to Next Campaign Mission** when another authored mission is available.
2. **Replay Previous Missions in Training Simulation**.
3. **Return to Main Menu**.

After Mission 3, the advance button is omitted until a later campaign mission exists.

### Simulator

The simulator opens a dedicated **TRAINING SIMULATION** mission-selection screen. It lists only missions already present in `CampaignState.CompletedMissions`. Each card includes the mission number, title, and a short objective reminder. Selecting a mission opens the existing crew-selection experience in simulation mode.

Once at least one campaign mission has been completed, the landing screen permanently includes a **Training Simulation** button. This keeps completed missions replayable after the player leaves a victory screen and after the current final campaign mission.

The crew screen shows **SELECT YOUR SIMULATION CREW**, explains that replay rewards are permanent, and allows exactly four distinct currently unlocked named crew members. “Unlocked” is based on the furthest canonical campaign mission completed:

- After Mission 1: Xander, Nance, Akiko, and Issac.
- After Mission 2: Xander, Nance, Akiko, Issac, Chandry, and Captain Gershon.
- Mission 3 completion does not add another current crew member.

This rule uses campaign progress rather than the historical roster for the selected replay mission, so later recruits can train in earlier scenarios.

Crew preparation remains available after selection. Players may change Training Paths, review abilities, and resolve any already-pending Class Progressions before deployment.

### Battle

Simulation battles use the existing mission map and objective. The HUD adds a prominent **VIRTUAL REPLAY** or **TRAINING SIMULATION** label. Mission 1's captive remains the simulated Issac objective; a playable Issac is a separate crew unit.

No introduction, wrap-up, or tutorial is shown. The battle begins after crew preparation.

### Simulation result

After a victory, normal XP presentations and reward confirmation run. The result screen offers:

- **Replay Another Training Mission**
- **Return to Campaign** (opens the next uncompleted canonical mission's preparation flow)
- **Return to Main Menu**

After defeat, all mission progression is rolled back and the same navigation choices are available.

## Progression Rules

### Experience and Level Ups

Every participating named crew member keeps all XP earned from attacks, healing, repairs, console interactions, rescues, and other existing XP-producing actions. Existing 100-XP Level Ups continue to apply immediately using the selected Training Path.

No average-XP or reserve catch-up award is made in any run mode. Nonparticipants are unchanged in both first-play campaign missions and simulations. Participation is the universal requirement for XP and Level Ups.

### Class Progressions

On every successful simulation, each participating named crew member receives one pending Class Progression only if `AvailableClassProgressions(characterId)` is nonempty after considering acquired and already-pending rewards. Pending rewards must not exceed the number of choices that can still be legally acquired. When the current catalog is exhausted, the character receives XP normally but no additional pending progression.

Prerequisites remain enforced. The existing six choices per character are the complete reward pool for now; future campaign missions may introduce level-gated branches without changing simulation history.

### Bonds

Participating named crew continue to grow bonds together after a successful simulation. This rewards using different teams and is consistent with bonds representing time spent completing missions together. Nonparticipants receive no bond growth.

### Failure transaction

At mission launch, snapshot character progression and bonds. A defeat restores both snapshots. A victory commits participating XP, Level Ups, eligible pending Class Progressions, and participating bond growth. `CompletedMissions` is never changed by a simulation.

## Campaign and Data Model

Introduce an explicit launch context, such as `MissionRunMode.Campaign` and `MissionRunMode.TrainingSimulation`, and carry it through mission launch, `BattleState`, result presentation, and reward flow. Do not infer replay mode solely from whether a mission was previously completed; an explicit mode keeps canonical flow and simulator behavior deterministic.

Add a pure training mission catalog/presentation state that derives selectable missions and unlocked crew from canonical completion state. Existing saves require no destructive migration: `CompletedMissions` remains the source of canonical unlocks, and progression records remain compatible.

Mission completion logic branches by run mode:

- Campaign completion preserves story unlocking and first-completion semantics, but removes the old reserve catch-up XP behavior.
- Simulation completion keeps participant XP, suppresses reserve catch-up, grants capped catalog rewards, grows participant bonds, and does not modify canonical mission completion.

## UI and Copy

- Use cyan/gold visual language already established by crew selection.
- Label every simulator screen clearly; do not rely on a one-time popup.
- Explain on the mission-selection screen: “Virtual replays award full XP and available Class Progressions to deployed crew. Crew left behind receive no progress.”
- Explain Mission 1's duplicate objective through the general simulation framing rather than special-case dialogue.
- Campaign result buttons should use action-oriented names rather than raw mission numbers.

## Approaches Considered

### Full simulation mode — selected

A dedicated mission selector, simulation-aware crew preparation, persistent battle label, explicit run mode, and separate replay result flow. This adds the most implementation work but makes story contradictions and permanent rewards clear everywhere they matter.

### Simple replay notice

Show one notice before launching the existing flow. This is cheaper but too easy to forget during Mission 1 and does not adequately distinguish canonical completion from repeat progression.

### Separate training campaign

Maintain a parallel campaign/save structure for simulation progress. This offers strong isolation but duplicates data and creates unnecessary save-management complexity because simulation rewards must feed back into the real crew.

## Constraints and Safety

- Maximum deployed named crew remains four.
- Only completed canonical missions are selectable.
- Only crew unlocked by canonical progress are selectable.
- Only deployed characters progress in campaign missions and simulations; reserve catch-up XP is removed.
- Summoned and reprogrammed robots continue crediting XP to their controller.
- A failed simulation cannot persist partial XP, Level Ups, pending rewards, or bond growth.
- Existing campaign saves and completed-mission records remain valid.

## Acceptance Criteria

- A player who completes Mission 1 can open the simulator, select Mission 1, and deploy Xander, Nance, Akiko, and Issac.
- The battle is visibly labeled as virtual and plays without its intro, wrap-up, or tutorial.
- Issac can participate while the simulated captive Issac remains the rescue objective.
- All deployed characters retain earned XP and Level Ups after victory.
- Each deployed character with remaining legal catalog choices receives exactly one pending Class Progression; an exhausted character receives none.
- An undeployed unlocked character receives no XP or reward.
- An undeployed character also receives no progression from a first-time campaign victory.
- Defeat restores progression and bonds exactly.
- A simulation does not unlock or complete a campaign mission.
- The post-campaign-victory screen clearly offers the next story mission and the training simulator.
- The landing screen exposes Training Simulation whenever at least one canonical mission is complete.
