using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using ESCP.Core;
using UnityEngine;
using Verse;

namespace ESCP.Factions
{
    public class LoreSettlement
    {
        public string name;
        public float x;
        public float z;
    }

    public class LoreMapFaction
    {
        public FactionDef faction;
        public List<LoreSettlement> settlements = new List<LoreSettlement>();
    }

    public class LoreMapDef : Def
    {
        public float width = 72f;
        public List<LoreMapFaction> factions = new List<LoreMapFaction>();
    }

    [DefOf]
    public static class LoreMapDefOf
    {
        public static LoreMapDef ESCP_LoreMap_Tamriel;
        public static ModOptionDef ESCP_Option_LoreFactionPlacement;

        static LoreMapDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(LoreMapDefOf));
    }

    public class FactionExtension_NamedSettlement : DefModExtension
    {
        public string settlementName              = null;
        public List<string> settlementNames       = null;
        public List<Hilliness> preferredHilliness = new() { Hilliness.Mountainous, Hilliness.LargeHills };
        public int searchRadius                   = 40;
        public int maxSettlements                 = 0;
        public List<BiomeDef> allowedBiomes       = null;
        public bool clustered                     = false;
        public bool coastal                       = false;
        public FactionDef nearFaction             = null;
    }

    [HarmonyPatch(typeof(World), nameof(World.FinalizeInit))]
    static class Patch_NamedSettlements
    {
        const int ClusterRadius = 20;
        const int AnchorSamples = 400;
        const int OriginSamples = 120;
        const int MinSpacing = 8;
        const int ForeignSpacing = 12;
        const int NearSpacing = 6;
        const int FactionGap = 40;
        const int LoreSpacing = 2;

        static readonly List<PlanetTile> placed = new List<PlanetTile>();
        static readonly List<PlanetTile> foreignTiles = new List<PlanetTile>();
        static readonly List<Faction> foreignFactions = new List<Faction>();
        static readonly List<PlanetTile> neighbors = new List<PlanetTile>();
        static int spacing;
        static bool lore;
        static bool relaxed;
        static Faction current;
        static FactionExtension_NamedSettlement currentExt;

        static void Postfix()
        {
            if (Current.Game == null || Current.Game.tickManager.TicksGame > 0)
                return;

            var grid = Find.WorldGrid;
            var byFaction = Find.WorldObjects.Settlements
                .Where(s => s.Faction != null && !s.Faction.IsPlayer && s.Tile.Layer.IsRootSurface)
                .GroupBy(s => s.Faction)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreignTiles.Clear();
            foreignFactions.Clear();
            var handled = new HashSet<Faction>();
            if (LoreMapDefOf.ESCP_Option_LoreFactionPlacement.Enabled && LoreMapDefOf.ESCP_LoreMap_Tamriel != null)
                PlaceLoreMap(LoreMapDefOf.ESCP_LoreMap_Tamriel, byFaction, handled, grid);

            foreach (var (faction, settlements) in byFaction.Where(p => !handled.Contains(p.Key)).OrderBy(p => p.Key.def.GetModExtension<FactionExtension_NamedSettlement>()?.nearFaction != null))
            {
                var ext = faction.def.GetModExtension<FactionExtension_NamedSettlement>();
                if (ext == null) continue;

                PlaceFaction(faction, settlements, ext, grid);
                handled.Add(faction);
            }

            current = null;
            currentExt = null;
            spacing = MinSpacing;
            placed.Clear();
            foreach (var (faction, settlements) in byFaction)
            {
                if (handled.Contains(faction))
                    continue;
                current = faction;
                foreach (Settlement s in settlements)
                {
                    if (!ClearOfForeign(s.Tile, grid))
                    {
                        int best = FindNearbyTile(s.Tile.tileId, 40, grid);
                        if (best >= 0)
                            Move(s, best);
                    }
                    foreignTiles.Add(s.Tile);
                    foreignFactions.Add(faction);
                }
            }
        }

        static void PlaceFaction(Faction faction, List<Settlement> settlements, FactionExtension_NamedSettlement ext, WorldGrid grid)
        {
            current = faction;
            currentExt = ext;
            placed.Clear();
            PlanetTile anchor = PlanetTile.Invalid;
            if (ext.nearFaction != null)
            {
                foreach (Settlement other in Find.WorldObjects.Settlements)
                {
                    if (other.Faction?.def != ext.nearFaction)
                        continue;
                    if (!anchor.Valid)
                        anchor = other.Tile;
                    placed.Add(other.Tile);
                }
            }
            else if (ext.clustered && foreignTiles.Count > 0)
            {
                int best = DistantAnchor(settlements[0].Tile.Layer, grid);
                if (best >= 0)
                    anchor = new PlanetTile(best, settlements[0].Tile.Layer);
            }

            if (ext.maxSettlements > 0 && settlements.Count > ext.maxSettlements)
            {
                for (int i = ext.maxSettlements; i < settlements.Count; i++)
                    Find.WorldObjects.Remove(settlements[i]);
                settlements.RemoveRange(ext.maxSettlements, settlements.Count - ext.maxSettlements);
            }

            int needed = 1 + DefDatabase<FactionSplitDef>.AllDefsListForReading.Count(split => ActiveSplit(split, faction));
            while (settlements.Count < needed && (ext.maxSettlements <= 0 || settlements.Count < ext.maxSettlements))
            {
                var extra = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                extra.SetFaction(faction);
                extra.Tile = settlements[0].Tile;
                Find.WorldObjects.Add(extra);
                settlements.Add(extra);
            }

            List<string> names = PickNames(faction, ext, settlements.Count);
            spacing = MinSpacing;
            bool cluster = ext.clustered || anchor.Valid;
            for (int i = 0; i < settlements.Count; i++)
            {
                var s = settlements[i];
                if (i < names.Count)
                    s.Name = names[i];

                bool inPlace = Accepts(ext, s.Tile.Tile) && ClearOfPlaced(s.Tile, grid) && ClearOfForeign(s.Tile, grid)
                    && (!cluster || !anchor.Valid || grid.TraversalDistanceBetween(anchor, s.Tile, true, ClusterRadius + 1) <= ClusterRadius);
                if (!inPlace)
                {
                    int best = FindNearbyTile(cluster && anchor.Valid ? anchor.tileId : s.Tile.tileId, ext.searchRadius, grid);
                    if (best < 0)
                        best = FindClosestAcceptedTile(cluster && anchor.Valid ? anchor : s.Tile, grid);
                    if (best < 0)
                    {
                        ESCPLog.Warning($"no good tile for {faction.def.defName} within {ext.searchRadius} steps");
                        continue;
                    }
                    Move(s, best);
                }

                if (!anchor.Valid) anchor = s.Tile;
                placed.Add(s.Tile);
            }

            foreach (Settlement s in settlements)
            {
                foreignTiles.Add(s.Tile);
                foreignFactions.Add(faction);
            }
        }

        static List<string> PickNames(Faction faction, FactionExtension_NamedSettlement ext, int count)
        {
            var names = new List<string>();
            if (ext.settlementNames.NullOrEmpty())
            {
                if (!ext.settlementName.NullOrEmpty())
                    names.Add(ext.settlementName);
                return names;
            }

            names.Add(ext.settlementNames[0]);
            names.AddRange(ext.settlementNames.Skip(1).InRandomOrder());
            int slot = 1;
            foreach (FactionSplitDef split in DefDatabase<FactionSplitDef>.AllDefsListForReading)
            {
                if (!ActiveSplit(split, faction))
                    continue;
                if (slot >= count)
                    break;
                int seceding = names.FindIndex(slot, split.settlements.Contains);
                if (seceding < 0)
                    continue;
                (names[slot], names[seceding]) = (names[seceding], names[slot]);
                slot++;
            }
            return names;
        }

        static bool ActiveSplit(FactionSplitDef split, Faction faction)
        {
            return split.loyalFaction == faction.def && (split.option == null || split.option.Enabled) && split.AllowsEra(ESCPToolsMod.Settings.characterEra);
        }

        static void PlaceLoreMap(LoreMapDef map, Dictionary<Faction, List<Settlement>> byFaction, HashSet<Faction> handled, WorldGrid grid)
        {
            var entries = new List<(Faction faction, LoreMapFaction entry, FactionExtension_NamedSettlement ext)>();
            foreach (LoreMapFaction entry in map.factions)
            {
                Faction faction = byFaction.Keys.FirstOrDefault(f => f.def == entry.faction);
                if (faction != null)
                    entries.Add((faction, entry, faction.def.GetModExtension<FactionExtension_NamedSettlement>()));
            }
            if (entries.Count == 0)
                return;

            PlanetLayer layer = byFaction[entries[0].faction][0].Tile.Layer;
            float scale = Mathf.Clamp(1.6f * Mathf.Sqrt(layer.TilesCount / Mathf.PI) / map.width, 0.5f, 3f);

            int origin = -1;
            int bestFit = -1;
            for (int i = 0; i < OriginSamples; i++)
            {
                int candidate = Rand.Range(0, layer.TilesCount);
                if (!TileFinder.IsValidTileForNewSettlement(layer[candidate].tile))
                    continue;

                int fit = 0;
                float tileSize = TileSize(candidate, grid);
                foreach (var (_, entry, ext) in entries)
                {
                    foreach (LoreSettlement city in entry.settlements)
                    {
                        Vector3 target = Target(candidate, city.x, city.z, scale, grid);
                        int tile = WalkToward(candidate, target, grid);
                        if ((grid.GetTileCenter(new PlanetTile(tile, layer)) - target).magnitude < tileSize * 2f && (ext == null || Accepts(ext, layer[tile])))
                            fit++;
                    }
                }
                if (fit > bestFit)
                {
                    bestFit = fit;
                    origin = candidate;
                }
            }
            if (origin < 0)
                return;

            spacing = LoreSpacing;
            lore = true;
            placed.Clear();
            foreach (var (faction, entry, ext) in entries)
            {
                current = faction;
                currentExt = ext;
                foreach (Settlement s in byFaction[faction])
                    Find.WorldObjects.Remove(s);

                foreach (LoreSettlement city in entry.settlements)
                {
                    int start = WalkToward(origin, Target(origin, city.x, city.z, scale, grid), grid);
                    int tile = LoreTile(start, ext, grid);
                    if (tile < 0)
                    {
                        ESCPLog.Warning($"no tile for {city.name}");
                        continue;
                    }

                    var settlement = (Settlement)WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.Settlement);
                    settlement.SetFaction(faction);
                    settlement.Tile = new PlanetTile(tile, layer);
                    settlement.Name = city.name;
                    Find.WorldObjects.Add(settlement);
                    placed.Add(settlement.Tile);
                    foreignTiles.Add(settlement.Tile);
                    foreignFactions.Add(faction);
                }
                handled.Add(faction);
            }
            lore = false;
        }

        static int LoreTile(int start, FactionExtension_NamedSettlement ext, WorldGrid grid)
        {
            var tile = new PlanetTile(start);
            if (CanPlace(tile, grid))
                return start;
            int best = FindNearbyTile(start, 6, grid);
            if (best >= 0)
                return best;
            relaxed = true;
            best = FindNearbyTile(start, 12, grid);
            if (best < 0)
                best = FindClosestAcceptedTile(tile, grid);
            relaxed = false;
            return best;
        }

        static int DistantAnchor(PlanetLayer layer, WorldGrid grid)
        {
            int best = -1;
            float bestScore = -1f;
            for (int i = 0; i < AnchorSamples; i++)
            {
                int candidate = Rand.Range(0, layer.TilesCount);
                var tile = new PlanetTile(candidate, layer);
                if (!Accepts(currentExt, tile.Tile) || !TileFinder.IsValidTileForNewSettlement(tile))
                    continue;

                float score = FactionGap;
                for (int j = 0; j < foreignTiles.Count && score > 0f; j++)
                    score = Mathf.Min(score, grid.ApproxDistanceInTiles(tile, foreignTiles[j]));
                if (score > bestScore)
                {
                    bestScore = score;
                    best = candidate;
                    if (score >= FactionGap)
                        break;
                }
            }
            return best;
        }

        static float TileSize(int tile, WorldGrid grid)
        {
            var t = new PlanetTile(tile);
            grid.GetTileNeighbors(t, neighbors);
            return (grid.GetTileCenter(neighbors[0]) - grid.GetTileCenter(t)).magnitude;
        }

        static Vector3 Target(int anchor, float x, float z, float scale, WorldGrid grid)
        {
            var tile = new PlanetTile(anchor);
            Vector3 center = grid.GetTileCenter(tile);
            Vector3 up = center.normalized;
            Vector3 north = (Vector3.up - Vector3.Dot(Vector3.up, up) * up).normalized;
            Vector3 east = Vector3.Cross(up, north);
            return center + (east * x + north * z) * TileSize(anchor, grid) * scale;
        }

        static int WalkToward(int start, Vector3 target, WorldGrid grid)
        {
            int current = start;
            float distance = (grid.GetTileCenter(new PlanetTile(current)) - target).sqrMagnitude;
            while (true)
            {
                int next = current;
                grid.GetTileNeighbors(new PlanetTile(current), neighbors);
                foreach (PlanetTile n in neighbors)
                {
                    float d = (grid.GetTileCenter(n) - target).sqrMagnitude;
                    if (d < distance)
                    {
                        distance = d;
                        next = n.tileId;
                    }
                }
                if (next == current)
                    return current;
                current = next;
            }
        }

        static void Move(Settlement s, int tile)
        {
            Find.WorldObjects.Remove(s);
            s.Tile = new PlanetTile(tile, s.Tile.Layer);
            Find.WorldObjects.Add(s);
        }

        static bool CanPlace(PlanetTile tile, WorldGrid grid)
        {
            return (currentExt == null || (relaxed ? !currentExt.coastal || Coastal(tile) : Accepts(currentExt, tile.Tile)))
                && ClearOfPlaced(tile, grid)
                && ClearOfForeign(tile, grid)
                && TileFinder.IsValidTileForNewSettlement(tile)
                && !Find.WorldObjects.AnyWorldObjectAt(tile);
        }

        static bool ClearOfPlaced(PlanetTile tile, WorldGrid grid)
        {
            foreach (PlanetTile other in placed)
            {
                if (grid.ApproxDistanceInTiles(tile, other) < spacing)
                    return false;
            }
            return true;
        }

        static bool ClearOfForeign(PlanetTile tile, WorldGrid grid)
        {
            if (lore)
                return true;
            for (int i = 0; i < foreignTiles.Count; i++)
            {
                Faction other = foreignFactions[i];
                if (other == current)
                    continue;
                int gap = currentExt?.nearFaction == other.def || other.def.GetModExtension<FactionExtension_NamedSettlement>()?.nearFaction == current.def ? NearSpacing : ForeignSpacing;
                if (grid.ApproxDistanceInTiles(tile, foreignTiles[i]) < gap)
                    return false;
            }
            return true;
        }

        static bool Accepts(FactionExtension_NamedSettlement ext, Tile tile)
        {
            if (!ext.preferredHilliness.NullOrEmpty() && !ext.preferredHilliness.Contains(tile.hilliness))
                return false;
            if (!ext.allowedBiomes.NullOrEmpty() && !ext.allowedBiomes.Contains(tile.PrimaryBiome))
                return false;
            return !ext.coastal || Coastal(tile.tile);
        }

        static readonly List<PlanetTile> coastNeighbors = new List<PlanetTile>();

        static bool Coastal(PlanetTile tile)
        {
            Find.WorldGrid.GetTileNeighbors(tile, coastNeighbors);
            foreach (PlanetTile n in coastNeighbors)
            {
                if (n.Tile.PrimaryBiome == BiomeDefOf.Ocean)
                    return true;
            }
            return false;
        }

        static int FindClosestAcceptedTile(PlanetTile origin, WorldGrid grid)
        {
            PlanetLayer layer = origin.Layer;
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < layer.TilesCount; i++)
            {
                Tile tile = layer[i];
                if (!CanPlace(tile.tile, grid))
                    continue;

                float distance = grid.ApproxDistanceInTiles(origin, tile.tile);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = tile.tile.tileId;
                }
            }
            return best;
        }

        static int FindNearbyTile(int origin, int radius, WorldGrid grid)
        {
            var visited = new HashSet<int> { origin };
            var queue   = new Queue<int>();
            var around  = new List<PlanetTile>();
            queue.Enqueue(origin);

            while (queue.Count > 0)
            {
                if (visited.Count > radius * radius) break;

                int t = queue.Dequeue();
                grid.GetTileNeighbors(new PlanetTile(t), around);

                foreach (PlanetTile n in around)
                {
                    int nId = n.tileId;
                    if (!visited.Add(nId)) continue;

                    if (CanPlace(n, grid))
                        return nId;

                    queue.Enqueue(nId);
                }
            }

            return -1;
        }
    }
}
