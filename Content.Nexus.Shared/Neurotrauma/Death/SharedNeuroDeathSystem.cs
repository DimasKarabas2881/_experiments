// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Nexus.Shared.Neurotrauma.CCVar;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Rejuvenate;
using Robust.Shared.Configuration;

namespace Content.Nexus.Shared.Neurotrauma.Death;

/// <summary>
///     Splits vanilla's single "Dead" state into clinical death and brain death.
///     MobState.Dead stays as is (so everything that checks it keeps working); the phase lives in
///     <see cref="NeuroDeathStateComponent"/>.
/// </summary>
public abstract class SharedNeuroDeathSystem : EntitySystem
{
    [Dependency] protected readonly IConfigurationManager Cfg = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NeuroDeathStateComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<NeuroDeathStateComponent, MobStateChangeAttemptEvent>(OnChangeAttempt);
        SubscribeLocalEvent<NeuroDeathStateComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<NeuroDeathStateComponent, RejuvenateEvent>(OnRejuvenate);
    }

    protected bool IsEnabled => Cfg.GetCVar(NeuroCVars.Enabled);

    /// <summary>
    ///     True if a defibrillator can still restart this body: it is either not dead,
    ///     or clinically dead with the defib window still open.
    /// </summary>
    public bool CanDefib(Entity<NeuroDeathStateComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false) || !IsEnabled)
            return true;

        return ent.Comp.Phase switch
        {
            NeuroDeathPhase.Alive => true,
            NeuroDeathPhase.ClinicalDeath => ent.Comp.EffectiveClinicalTime < ent.Comp.DefibWindow.TotalSeconds,
            _ => false,
        };
    }

    public bool IsBrainDead(Entity<NeuroDeathStateComponent?> ent)
    {
        return Resolve(ent, ref ent.Comp, false) && ent.Comp.Phase == NeuroDeathPhase.BrainDeath;
    }

    protected void SetPhase(Entity<NeuroDeathStateComponent> ent, NeuroDeathPhase phase)
    {
        var old = ent.Comp.Phase;
        if (old == phase)
            return;

        ent.Comp.Phase = phase;

        if (phase == NeuroDeathPhase.BrainDeath)
            ent.Comp.ResidualActivity = 1f;

        Dirty(ent.Owner, ent.Comp);

        var ev = new NeuroDeathPhaseChangedEvent(ent.Owner, old, phase);
        RaiseLocalEvent(ent.Owner, ref ev, true);
    }

    // Bodies that spawn already dead (map corpses etc.) never get a state-change event.
    private void OnStartup(EntityUid uid, NeuroDeathStateComponent comp, ComponentStartup args)
    {
        if (IsEnabled && comp.Phase == NeuroDeathPhase.Alive && _mobState.IsDead(uid))
            SetPhase((uid, comp), NeuroDeathPhase.ClinicalDeath);
    }

    /// <summary>
    ///     Brain death is permanent: nothing may pull the body out of the Dead state.
    ///     After the defib window closes (and nexus.neuro.strict_defib_window is on) the heart cannot be
    ///     restarted either, no matter what tries to do it (vanilla CPR chance, healing, ...).
    /// </summary>
    private void OnChangeAttempt(Entity<NeuroDeathStateComponent> ent, ref MobStateChangeAttemptEvent args)
    {
        if (!IsEnabled || args.NewState == MobState.Dead)
            return;

        switch (ent.Comp.Phase)
        {
            case NeuroDeathPhase.BrainDeath:
                args.Cancelled = true;
                break;

            case NeuroDeathPhase.ClinicalDeath:
                if (Cfg.GetCVar(NeuroCVars.StrictDefibWindow) && !CanDefib((ent.Owner, ent.Comp)))
                    args.Cancelled = true;
                break;
        }
    }

    private void OnMobStateChanged(Entity<NeuroDeathStateComponent> ent, ref MobStateChangedEvent args)
    {
        if (!IsEnabled)
            return;

        if (args.NewMobState == MobState.Dead)
        {
            if (ent.Comp.Phase == NeuroDeathPhase.Alive)
                SetPhase(ent, NeuroDeathPhase.ClinicalDeath);
        }
        else if (ent.Comp.Phase == NeuroDeathPhase.ClinicalDeath)
        {
            SetPhase(ent, NeuroDeathPhase.Alive);
        }
    }

    /// <summary>
    ///     Admin/system rejuvenation overrides everything. Damage may heal before this handler runs, in which
    ///     case the revive attempt was vetoed above, so the mob state is re-evaluated explicitly.
    /// </summary>
    private void OnRejuvenate(EntityUid uid, NeuroDeathStateComponent comp, RejuvenateEvent args)
    {
        comp.EffectiveClinicalTime = 0f;
        comp.ResidualActivity = 1f;
        SetPhase((uid, comp), NeuroDeathPhase.Alive);
        _mobState.UpdateMobState(uid);
    }
}
