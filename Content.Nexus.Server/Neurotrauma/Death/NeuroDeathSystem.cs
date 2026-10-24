// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Nexus.Shared.Neurotrauma.CCVar;
using Content.Nexus.Shared.Neurotrauma.Death;
using Content.Server.Popups;
using Content.Shared.Medical;
using Content.Shared.Medical.CPR;
using Robust.Shared.Timing;

namespace Content.Nexus.Server.Neurotrauma.Death;

public sealed class NeuroDeathSystem : SharedNeuroDeathSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Both hook into vanilla's CPRSystem / DefibrillatorSystem via events raised on the patient.
        // NOTE: the event bus allows exactly one handler per (component, event) pair, so never subscribe
        // to a vanilla component's events here - add a seam event to vanilla instead.
        SubscribeLocalEvent<NeuroDeathStateComponent, CPRCycleCompletedEvent>(OnCprCycle);
        SubscribeLocalEvent<NeuroDeathStateComponent, TargetBeforeDefibrillatorZapsEvent>(OnBeforeDefib);
    }

    /// <summary>A completed CPR cycle keeps the body perfused for the duration of the next cycle.</summary>
    private void OnCprCycle(Entity<NeuroDeathStateComponent> ent, ref CPRCycleCompletedEvent args)
    {
        ent.Comp.PerfusionUntil = _timing.CurTime + args.CycleDuration + ent.Comp.PerfusionGrace;
    }

    private void OnBeforeDefib(EntityUid uid, NeuroDeathStateComponent comp, TargetBeforeDefibrillatorZapsEvent args)
    {
        if (CanDefib((uid, comp)))
            return;

        args.Cancel();
        _popup.PopupEntity(Loc.GetString("nexus-neuro-defib-no-rhythm"), uid, args.EntityUsingDefib);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!IsEnabled)
            return;

        var dt = frameTime * Cfg.GetCVar(NeuroCVars.TimeScale);
        var perfusionFactor = Cfg.GetCVar(NeuroCVars.PerfusionFactor);
        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<NeuroDeathStateComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var factor = now < comp.PerfusionUntil ? perfusionFactor : 1f;

            switch (comp.Phase)
            {
                case NeuroDeathPhase.Alive:
                    // Forgive accumulated clinical death time slowly; never instantly.
                    if (comp.EffectiveClinicalTime > 0f)
                        comp.EffectiveClinicalTime = MathF.Max(0f, comp.EffectiveClinicalTime - dt * comp.DebtRecoveryRate);
                    break;

                case NeuroDeathPhase.ClinicalDeath:
                    comp.EffectiveClinicalTime += dt * factor;
                    if (comp.EffectiveClinicalTime >= (float) comp.BrainDeathDelay.TotalSeconds)
                        SetPhase((uid, comp), NeuroDeathPhase.BrainDeath);
                    break;

                case NeuroDeathPhase.BrainDeath:
                    var halfLife = (float) comp.ActivityHalfLife.TotalSeconds;
                    if (halfLife > 0f)
                        comp.ResidualActivity *= MathF.Pow(0.5f, dt * factor / halfLife);
                    break;
            }
        }
    }
}
