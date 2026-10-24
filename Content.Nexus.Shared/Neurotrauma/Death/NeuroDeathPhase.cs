// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Nexus.Shared.Neurotrauma.Death;

/// <summary>
///     Two-stage death. <see cref="Content.Shared.Mobs.MobState.Dead"/> covers both death phases;
///     this enum tells them apart.
/// </summary>
[Serializable, NetSerializable]
public enum NeuroDeathPhase : byte
{
    /// <summary>The body is not dead (Alive or Critical).</summary>
    Alive,

    /// <summary>
    ///     The body is dead but the brain still lives. Revival is possible (defibrillator within the defib
    ///     window, CPR). A countdown to brain death is running.
    /// </summary>
    ClinicalDeath,

    /// <summary>
    ///     The brain is dead. Revival is impossible; only cloning can bring the person back.
    ///     Residual neuron activity decays exponentially from this point.
    /// </summary>
    BrainDeath,
}
