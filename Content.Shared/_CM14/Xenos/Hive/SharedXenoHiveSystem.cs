﻿using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._CM14.Xenos.Hive;

public abstract class SharedXenoHiveSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _compFactory = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HiveComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<HiveComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.AnnouncedUnlocks.Clear();
        ent.Comp.Unlocks.Clear();
        ent.Comp.AnnouncementsLeft.Clear();

        foreach (var prototype in _prototypes.EnumeratePrototypes<EntityPrototype>())
        {
            if (prototype.TryGetComponent(out XenoComponent? xeno, _compFactory))
            {
                if (xeno.UnlockAt == default)
                    continue;

                ent.Comp.Unlocks.GetOrNew(xeno.UnlockAt).Add(prototype.ID);

                if (!ent.Comp.AnnouncementsLeft.Contains(xeno.UnlockAt))
                    ent.Comp.AnnouncementsLeft.Add(xeno.UnlockAt);
            }
        }

        foreach (var unlock in ent.Comp.Unlocks)
        {
            unlock.Value.Sort();
        }

        ent.Comp.AnnouncementsLeft.Sort();
    }

    public void CreateHive(string name)
    {
        if (_net.IsClient)
            return;

        var ent = Spawn(null, MapCoordinates.Nullspace);
        EnsureComp<HiveComponent>(ent);
        _metaData.SetEntityName(ent, name);
    }
}
