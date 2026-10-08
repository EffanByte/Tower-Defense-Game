# Tower Defense Game

A systems-focused **Unity tower defense game** featuring procedural map generation, A* pathfinding, wave-based enemy progression, turret combat, projectiles, upgrades, and an in-game economy.

The project focuses on building interconnected gameplay systems rather than isolated mechanics, with particular emphasis on procedural content, enemy navigation, and tower-defense architecture.

---

## Features

### Procedural Map Generation
Levels are generated dynamically to produce varied playable layouts rather than relying entirely on fixed maps.

### A* Pathfinding
Enemies use **A\*** pathfinding to navigate generated environments and determine routes toward their objective.

### Wave System
Enemy encounters are organized into waves, providing structured progression and increasing gameplay pressure over time.

### Turret & Projectile Systems
Turrets handle target acquisition and attacks through projectile-based combat systems.

### Upgrade & Economy Systems
Players can spend resources on tower placement and upgrades, connecting combat performance with progression and strategic decision-making.

---

## Technical Highlights

- Procedural level generation
- A* pathfinding
- Enemy navigation
- Wave-based spawning and progression
- Turret targeting
- Projectile systems
- Tower upgrades
- Resource / economy mechanics
- Modular gameplay systems in C#

---

## System Overview

```text
         PROCEDURAL MAP
               │
               ▼
        LEVEL / PATH DATA
               │
               ▼
         A* PATHFINDING
               │
               ▼
        ENEMY NAVIGATION
               │
               ▼
           WAVE SYSTEM
               │
        ┌──────┴──────┐
        ▼             ▼
     ENEMIES      TURRET TARGETING
                       │
                       ▼
                   PROJECTILES
                       │
                       ▼
                     COMBAT


     ECONOMY
        │
        ├────► TOWER PLACEMENT
        │
        └────► TOWER UPGRADES
```

---

## Tech Stack

- **Unity 6**
- **C#**
- **Universal Render Pipeline**
- **Unity Input System**
- **Unity AI / Navigation**
- **Unity 2D Tilemap**
- **Cinemachine**

---

## What I Worked On

The project involved implementing several connected gameplay and algorithmic systems, including:

- procedural map generation
- A* pathfinding and enemy navigation
- wave progression
- turret targeting and projectile behaviour
- tower upgrade systems
- gameplay economy and resource progression

A major focus was making these systems work together cleanly inside a real-time Unity project.

---

## Getting Started

### Requirements

The project was developed using:

```text
Unity 6000.0.57f1
```

Using the same Unity version is recommended.

### Clone the Repository

```bash
git clone https://github.com/EffanByte/Tower-Defense-Game.git
```

Then:

1. Open **Unity Hub**
2. Select **Add project from disk**
3. Choose the cloned repository
4. Open it using Unity `6000.0.57f1`
5. Allow Unity to restore the required packages
6. Open the gameplay scene
7. Press **Play**

---

## Project Goals

This project was primarily an exercise in designing and implementing gameplay systems that depend on one another.

Some of the main areas explored were:

- algorithmic pathfinding
- procedural gameplay generation
- real-time enemy behaviour
- reusable gameplay architecture
- progression systems
- balancing gameplay state across multiple interacting systems

---

## Repository

[github.com/EffanByte/Tower-Defense-Game](https://github.com/EffanByte/Tower-Defense-Game)

---

## Author

**Effan Shakeel**

Gameplay Programmer focused on gameplay systems, AI, multiplayer, procedural systems, and real-time graphics.

[GitHub](https://github.com/EffanByte) · [LinkedIn](https://www.linkedin.com/in/effan-shakeel-42a58721a/)
