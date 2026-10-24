// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.ViewVariables;

namespace Content.Nexus.Shared.Neurotrauma.Death;

/// <summary>
///     Tracks the two-stage death of a body. See <see cref="NeuroDeathPhase"/>.
///     All durations here are "effective" time: they run slower while CPR perfuses the body
///     and are scaled by the nexus.neuro.* CVars.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NeuroDeathStateComponent : Component
{
    [DataField, AutoNetworkedField]
    public NeuroDeathPhase Phase = NeuroDeathPhase.Alive;

    /// <summary>
    ///     Effective time spent in clinical death, in seconds. It is NOT reset on revival: it only slowly
    ///     recovers while the person is alive (see <see cref="DebtRecoveryRate"/>), so quickly dying and
    ///     being revived over and over cannot be used to reset the clock.
    /// </summary>
    [DataField]
    public float EffectiveClinicalTime;

    /// <summary>
    ///     After this much effective clinical death time the heart can no longer be restarted by a defibrillator.
    /// </summary>
    [DataField]
    public TimeSpan DefibWindow = TimeSpan.FromMinutes(5);

    /// <summary>
    ///     After this much effective clinical death time the brain dies. Should be longer than <see cref="DefibWindow"/>.
    /// </summary>
    [DataField]
    public TimeSpan BrainDeathDelay = TimeSpan.FromMinutes(12);

    /// <summary>
    ///     Half-life of residual neuron activity after brain death: activity = 0.5 ^ (t / halfLife).
    /// </summary>
    [DataField]
    public TimeSpan ActivityHalfLife = TimeSpan.FromMinutes(20);

    /// <summary>
    ///     How many seconds of accumulated clinical death time are forgiven per second of being alive.
    /// </summary>
    [DataField]
    public float DebtRecoveryRate = 0.1f;

    /// <summary>
    ///     How long perfusion continues after a completed CPR cycle (added on top of the cycle duration).
    /// </summary>
    [DataField]
    public TimeSpan PerfusionGrace = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     Fraction (0..1) of neuron activity left after brain death. Starts at 1 on brain death.
    ///     Consumed by cloning in a later stage. Not networked: it changes continuously.
    /// </summary>
    [DataField]
    public float ResidualActivity = 1f;

    /// <summary>Server-side: until when the body counts as perfused by CPR.</summary>
    [ViewVariables]
    public TimeSpan PerfusionUntil;
}
