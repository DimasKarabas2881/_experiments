// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Nexus.Shared.Neurotrauma.Death;

/// <summary>
///     Raised (directed and broadcast) after <see cref="NeuroDeathStateComponent.Phase"/> changes.
///     Hook point for cloning, scanners, mood and other later systems.
/// </summary>
[ByRefEvent]
public readonly record struct NeuroDeathPhaseChangedEvent(
    EntityUid Target,
    NeuroDeathPhase OldPhase,
    NeuroDeathPhase NewPhase);
