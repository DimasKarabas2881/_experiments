// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Medical.CPR;

[Serializable, NetSerializable]
public sealed partial class CPRDoAfterEvent : SimpleDoAfterEvent;

// Nexus Change Start
/// <summary>
/// Raised on the patient each time a CPR cycle completes successfully.
/// A seam so other systems can react without a duplicate subscription to <see cref="CPRDoAfterEvent"/>.
/// </summary>
/// <param name="Performer">Entity doing the CPR</param>
/// <param name="Target">The patient</param>
/// <param name="CycleDuration">Duration of the completed cycle (the next one is expected to take as long)</param>
[ByRefEvent]
public readonly record struct CPRCycleCompletedEvent(EntityUid Performer, EntityUid Target, TimeSpan CycleDuration);
// Nexus Change End
