# S. P. D. Is Late: Enemies & Level Design (Dallas)

## What the team needs to agree on
These are the points where my scripts touch Jacob's and Ethan's work:

| Thing | Value | Who cares |
|---|---|---|
| Player tag | `Player` | Jacob sets it on the player object |
| Player physics | `Rigidbody2D` (Dynamic) + `Collider2D` | Jacob; stomping reads and sets `velocity.y` |
| Ground layer | `Ground` (Tilemap + platforms) | Jacob's ground check, my enemy ledge check |
| Enemy layer | `Enemies` (must **not** be Ground) | me |
| Level events | `LevelManager.OnLevelStarted`, `OnLevelCompleted`, `OnPlayerDied(int deaths)` | Ethan's timer and UI subscribe to these |

Example for Ethan's timer:
```csharp
void OnEnable()  { LevelManager.OnLevelStarted += StartTimer; LevelManager.OnLevelCompleted += StopTimer; }
void OnDisable() { LevelManager.OnLevelStarted -= StartTimer; LevelManager.OnLevelCompleted -= StopTimer; }
```

## Scripts
| Script | Put it on | Notes |
|---|---|---|
| `LevelManager` | Empty `LevelManager` object, one per level scene | Assign `Start Point`; optional `Next Scene Name` |
| `Checkpoint` | Signpost or mushroom with a trigger collider | Optional sprite swap when activated |
| `KillZone` | Wide trigger under the level; thorn bushes; water | Sends player to last checkpoint |
| `LevelGoal` | Trigger at the end of the path | Fires `OnLevelCompleted`, loads next scene |
| `PatrolEnemy` | Beetle / hedgehog | Rigidbody2D Dynamic, child `GroundCheck` at front foot, set `Ground Layer` |
| `FlyingEnemy` | Bee / bird | Rigidbody2D Kinematic, set `Travel Offset` (cyan gizmo) |

**Enemy roster**
- **Beetle**: `PatrolEnemy`, can be stomped. The basic enemy.
- **Hedgehog**: `PatrolEnemy`, **uncheck Can Be Stomped**. The player has to jump over it.
- **Bee**: `FlyingEnemy`, can be stomped. Guards gaps and the air above platforms.
- **Thorn bush**: sprite + `KillZone`. A hazard that doesn't move.

Make each one a **prefab** (drag from Hierarchy into `Assets/Prefabs/Enemies`) so levels are quick to fill.

## Tilemap setup (project uses Unity 2021.3)
1. *GameObject → 2D Object → Tilemap → Rectangular* creates a `Grid` with a `Tilemap` child.
2. Make separate tilemaps under the same Grid: `Background` (no collider), `Ground`, `Decoration`.
3. On `Ground`: add **Tilemap Collider 2D** and **Composite Collider 2D**, tick *Used By Composite* on the tilemap collider, and set the Rigidbody2D that gets added to **Static**. This keeps the player from snagging on tile seams. Set the layer to `Ground`.
4. *Window → 2D → Tile Palette*, create a palette, and drag the sliced tileset sprite onto it.
5. Use **Sorting Layers** (Background, Ground, Characters, Foreground) so the player draws in front of trees but behind foreground leaves.
6. Parallax background: 2–3 layers of forest art behind the tilemap.

Reference: https://docs.unity3d.com/2022.3/Documentation/Manual/Tilemap.html

**Art:** we're using the *2D Pixel Art Platformer Biome - American Forest* pack already in `Assets/`. Its `Tilemap/` folder has ready-made Ground, BackGround, Plant, and Fence tiles, plus `TileSpikes` for thorn hazards (put spikes on their own `Hazards` tilemap with a trigger Tilemap Collider 2D and `KillZone`). Its `Demo.unity` scene is a good reference.

## Scene builder (Start, Game Over, Level 0)
`Assets/Scripts/Editor/SceneBuilder.cs` adds a **S.P.Dee → Build Start, Game Over and Level 0** menu in Unity. It:
- builds `_Start_Scene` (title, Play/Quit) and `_GameOver_Scene` (total time and falls, Play Again/Main Menu)
- builds `_Scene_Lvl_0` from the forest tiles: start area, a gap, a beetle, a hedgehog, a spike pit, a bee, one checkpoint and the goal, with parallax background
- adds the `Ground` and `Enemies` layers and sets the Build Settings order

It **overwrites** those three scenes, so run it once, then edit the scenes by hand afterwards.
The player in Level 0 is a placeholder box (`PlaceholderPlayer`) until Jacob's player is ready.
Level 0 currently goes straight to the Game Over screen; clear **Next Scene Name** on its LevelManager once Level 1 is built.

## Level plan (matches the scenes in `Assets/Scenes/`)
The goal is a good time, so each level should have a **safe route** and a **faster, riskier route** (a stomp chain, a hard jump, a sprint gap).

| Scene | Level | Content | Checkpoints | Target time |
|---|---|---|---|---|
| `_Scene_Lvl_0` | Test / Tutorial | Flat ground, a gap, one beetle, one hedgehog, a long straight stretch to sprint on | 1 | about 30 s |
| `_Scene_Lvl_1` | Meadow's Edge | Low platforms, beetles, first spike patches | 1 | about 45 s |
| `_Scene_Lvl_2` | Mushroom Hollow | Up and down platforming, bees between platforms, bottomless pits; stomp a bee to skip a gap | 2 | about 75 s |
| `_Scene_Lvl_3` | Thornwood | Dense spikes, tight jumps, beetles and hedgehogs on the same platforms | 2 | about 90 s |
| `_Scene_Lvl_4` | Forest Exit | Treetop route (faster, more bees) vs. ground route (slower, spikes); ends with the office in view | 2–3 | about 100 s |

On `_Scene_Lvl_4`'s LevelManager, set **Next Scene Name** to `_GameOver_Scene` (or whatever results screen Ethan makes).
Build Settings order: `_Start_Scene`, `_Scene_Lvl_0` … `_Scene_Lvl_4`, `_GameOver_Scene`.

**Checklist for each level**
- [ ] KillZone under the whole level
- [ ] Start Point and LevelManager placed
- [ ] LevelGoal at the end
- [ ] Scene added to *File → Build Settings* in order
- [ ] Playtest: can it be beaten without dying? Is the fast route actually faster?

## Timeline (my part)
- **Week 1:** set up the tilemap and the test level (`_Scene_Lvl_0`) with Jacob's player in it; agree on layers and tags.
- **Week 2:** enemy prefabs, Levels 1–4 blocked out, checkpoints, and kill zones.
- **Week 3:** art pass on the levels, enemy animations, tune difficulty and target times, fix bugs.
