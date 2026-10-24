# Failed Override — Campaign v2.4

Requires Godot 4.7.2 .NET and the .NET 8 SDK. Import `project.godot`, build, and press F5. Existing campaign saves are stored separately under Godot's `user://` data and remain available through **Continue saved campaign**.

## Development

Run the dependency-free battle logic checks with `dotnet run --project Tests/FailedOverride.LogicTests.csproj`.

This project is tracked in a local Git repository. No online remote repository is configured.

## Mission 3 roster update

Otto, Odette, and Tiger remain story characters aboard the freight ship but are no longer controllable combat units. Maintenance ducts, traps, barking, and capture rules are not used by Mission 3.

Dr. Chrandy Perra joins the player squad as a level-1 Medic: 17 HP, 1 armor, and 5 movement.

- **Stun pistol:** range 3, deals zero HP damage, and stuns an ordinary target for its next turn. Ompok's mechanical proxy is immune.
- **Field treatment:** heals an adjacent injured organic teammate for 7 HP, consumes Chrandy's action, and uses one of three medical supplies. Chrandy cannot target himself. It cannot repair robots or reprogrammed droids; Akiko and Issac retain that job.

Mission 3 still requires defeating Ompok's proxy and hybrid force before the proxy completes its two-action bridge takeover. Previous XP, levels, bonds, and upgrades carry forward.

## Version 0.9 character portraits

The project includes five concept portraits under `Assets/Portraits`. Selecting Xander, Nance, Akiko, Issac, or Dr. Chrandy displays that character beside their stats and actions in the battle sidebar. Summoned robots and enemies continue using the compact text-only panel. The portrait files are RGB concept art with a baked light checker-style backdrop, so the UI treats them as rectangular portrait cards rather than transparent sprites.

## Version 1.0 board tokens

The battlefield now uses illustrated class tokens for Xander's Scout, Nance's Shapeshifter, Akiko's Tech, Issac's Science Officer, and Chrandy's Medic. Nance automatically switches to a separate armored-bear token while transformed. Tokens use distinct border colors and class emblems, dim after acting, retain the selection outline and HP bar, and are stored under `Assets/BoardTokens`. Summoned robots and enemies retain their existing procedural tokens for now.

## Version 1.1 enlarged battlefield

The 12×12 battlefield now uses 64-pixel tiles in a 1400×980 game window. Illustrated hero tokens are approximately 30% larger, with larger selection outlines and health bars. The battle sidebar is wider and taller so character portraits, actions, and mission information remain readable beside the enlarged board.

## Version 1.2 board input correction

The clickable battlefield boundary now derives from the 12×12 grid and current tile size. All enlarged tiles—including the heroes' starting row and the rightmost columns—can be selected and used normally.

## Version 1.3 mouse-over descriptions

Hover over a character portrait for a short class summary. Hover over any attack or ability in the side panel for its range, effect, targeting restrictions, resource use, and whether it ends the unit's action. Tooltips appear after 0.4 seconds.

## Version 1.4 readable hover-help console

Mouse-over descriptions now appear in a fixed information console across the bottom of the game window. The panel uses larger gold text on a dark background and remains visible while the pointer is over a character portrait, attack, or ability. Small floating tooltips have been removed so descriptions do not cover buttons or the battlefield.

## Version 1.5 Captain Jeane tutorial

Mission 1 includes contextual instruction from Captain Jeane Gerson. Her portrait and dialogue appear over the half of the battlefield opposite the selected character and ignore mouse input, so recommended spaces remain usable. The panel disappears immediately when its lesson is completed and returns only when a new teaching situation occurs. Lessons introduce movement and undo, Nance's Bear form, Akiko's robots, attacks, the disruption console, and rescuing Issac.

## Version 1.6 Jeane Gerson portrait revision

Captain Jeane Gerson's tutorial portrait now depicts her as a Black Brazilian woman while retaining her established burgundy command uniform, silver-streaked hair, and calm mentor role.

## Version 1.7 tutorial dialogue contrast

Jeane's tutorial text now appears in an opaque dark-blue dialogue box with a gold three-pixel border, rounded corners, generous padding, larger near-white lettering, and a subtle text shadow for improved readability.

## Version 1.8 flexible turns and readiness indicators

Player units may now act before moving or move before acting. A turn is complete only after both parts are used; Wait deliberately ends both. The game selects the next available unit after the current unit finishes and starts the enemy phase automatically only when every available unit is complete.

Board rings show readiness at a glance: pulsing cyan means movement and action are both available, pulsing gold means one remains, gray/dim means finished, purple means stunned, and red means captured. The phase header also displays a live CREW READY count.

## Version 1.9 landing screen and settings

The game opens on a full landing screen with Continue Campaign, New Campaign, Settings, and Quit Game. Continue displays the next mission represented by the current campaign save.

Settings are stored separately under Godot's user data and apply immediately. Master, Music, and Effects audio buses have independent 0–100% volume controls. Display choices are Compact (1120×856), Standard (1400×1070), Large (1680×1284), and Fullscreen. The existing 1400×1070 interface remains the design canvas and scales with the selected window. Captain Jeane's tutorial can also be enabled or disabled.

## Version 2.0 illustrated landing screen

The landing screen now uses a cinematic Selene outpost battle illustration featuring Xander, Nance in Bear form, Akiko, her support robot, and corrupted droids. The center remains visually calm for the menu, with an additional restrained dark tint to preserve button and title contrast at every display size.

## Version 2.1 enemy and environment artwork

All enemy classes now have illustrated board tokens: corrupted security droid, corrupted heavy droid, Ompok proxy, Vaur brute, Khelt stalker, and Synaptic engineer. Reprogrammed enemy droids retain their recognizable machine artwork while gaining player readiness rings.

Each mission now has a distinct illustrated battlefield underlay: Selene research laboratory, industrial evacuation deck, and the crew's freight-ship interior. The disruption console, evacuation shuttle, and bridge nexus are illustrated objects rather than letter markers. Semi-transparent grid shading preserves walls, movement highlights, attack ranges, and tactical readability.

## Version 2.2 local playtest feedback

The landing screen includes a Feedback page for Bug Reports, Questions, and Comments. Players can enter a short title and a detailed message. Save Feedback creates a timestamped text file under the game's local user-data feedback folder and records campaign context automatically. Copy to Clipboard makes an entry easy to paste elsewhere, and Open Feedback Folder reveals all saved reports. No account or network connection is required.

## Version 2.3 post-mission navigation

Mission results now offer Return to Main Menu, Replay Current Mission, and direct access to every other unlocked mission. Mission 3 victory is saved immediately, so the landing-screen Continue status remains accurate. Replaying Mission 1 after choosing campaign upgrades preserves those choices and does not reopen the upgrade sequence.

## Version 2.4 brighter battlefields

All three battlefields have a gentle blue-gray light lift. Floor grid shading is brighter and more transparent so the illustrated environments remain visible, walls are easier to distinguish, and movement, attack, and interaction highlights have been raised to match.
