// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Nexus.Shared.Neurotrauma.CCVar;

[CVarDefs]
public sealed class NeuroCVars
{
    /// <summary>
    ///     Master switch for the two-stage death model (clinical death -> brain death).
    ///     When false, bodies behave like vanilla: Dead is a single, always-revivable state.
    /// </summary>
    public static readonly CVarDef<bool> Enabled =
        CVarDef.Create("nexus.neuro.enabled", true, CVar.SERVERONLY);

    /// <summary>
    ///     Global speed multiplier for all neuro death timers (brain death delay, activity decay, debt recovery).
    ///     2.0 = everything happens twice as fast.
    /// </summary>
    public static readonly CVarDef<float> TimeScale =
        CVarDef.Create("nexus.neuro.time_scale", 1f, CVar.SERVERONLY);

    /// <summary>
    ///     Speed multiplier for the death timers while the body is being perfused by CPR.
    ///     1.0 = CPR has no effect, 0.0 = CPR fully freezes the timers.
    /// </summary>
    public static readonly CVarDef<float> PerfusionFactor =
        CVarDef.Create("nexus.neuro.cpr_factor", 0.25f, CVar.SERVERONLY);

    /// <summary>
    ///     When true, once the defib window has closed NOTHING can pull a clinically dead body out of the
    ///     Dead state (CPR resuscitation chance, healing below the death threshold, etc.): the heart cannot
    ///     be restarted any more. When false, only brain death makes Dead permanent.
    /// </summary>
    public static readonly CVarDef<bool> StrictDefibWindow =
        CVarDef.Create("nexus.neuro.strict_defib_window", true, CVar.SERVERONLY);
}
