using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ESCP.Core.Thuum
{
	public class WordWallExtension : DefModExtension
	{
		public List<ShoutEntry> shouts = new();
	}

	public class ShoutEntry
	{
		public AbilityDef abilityDef;
		public int dragonSoulCost = 1;
		public int learnTicks = 3000;
	}

	public class CompProperties_WordWall : CompProperties
	{
		public CompProperties_WordWall() => compClass = typeof(CompWordWall);
	}

	public class CompWordWall : ThingComp
	{
		private WordWallExtension _ext;
		private WordWallExtension Ext => _ext ??= parent.def.GetModExtension<WordWallExtension>();

		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			var ext = Ext;
			if (ext == null) yield break;

			for (int i = 0; i < ext.shouts.Count; i++)
			{
				var entry = ext.shouts[i];
				string label = "ESCP_LearnShout".Translate(entry.abilityDef.LabelCap);

				AcceptanceReport report = CanLearn(selPawn, entry);
				if (!report.Accepted)
				{
					if (!report.Reason.NullOrEmpty())
						yield return new FloatMenuOption($"{label}: {report.Reason.CapitalizeFirst()}", null);
					continue;
				}

				int idx = i;
				yield return FloatMenuUtility.DecoratePrioritizedTask(
					new FloatMenuOption(label, () =>
					{
						var job = JobMaker.MakeJob(ThuumInternalDefs.LearnShout, parent);
						job.count = idx;
						selPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
					}),
					selPawn, parent);
			}
		}

		public AcceptanceReport CanLearn(Pawn pawn, ShoutEntry entry)
		{
			if (pawn.abilities?.GetAbility(entry.abilityDef) != null)
				return new AcceptanceReport("ESCP_AlreadyKnowsShout".Translate(entry.abilityDef.LabelCap));

			if (entry.abilityDef.casterMustBeCapableOfViolence && pawn.WorkTagIsDisabled(WorkTags.Violent))
				return new AcceptanceReport("IsIncapableOfViolenceLower".Translate(pawn.LabelShort, pawn));

			int souls = pawn.Map?.listerThings
				.ThingsOfDef(ThuumInternalDefs.DragonSoul)
				.Where(t => !t.IsForbidden(pawn))
				.Sum(t => t.stackCount) ?? 0;

			if (souls < entry.dragonSoulCost)
				return new AcceptanceReport("ESCP_NotEnoughDragonSouls".Translate(entry.dragonSoulCost));

			return true;
		}

		public bool TryGrantShout(Pawn pawn, int shoutIndex)
		{
			var ext = Ext;
			if (ext == null || shoutIndex < 0 || shoutIndex >= ext.shouts.Count)
				return false;

			var entry = ext.shouts[shoutIndex];

			AcceptanceReport report = CanLearn(pawn, entry);
			if (!report.Accepted)
			{
				Messages.Message(report.Reason.CapitalizeFirst(), pawn, MessageTypeDefOf.RejectInput, false);
				return false;
			}

			int remaining = entry.dragonSoulCost;
			foreach (Thing soul in pawn.Map.listerThings
				.ThingsOfDef(ThuumInternalDefs.DragonSoul)
				.Where(t => !t.IsForbidden(pawn))
				.ToList())
			{
				int take = Mathf.Min(remaining, soul.stackCount);
				soul.SplitOff(take).Destroy();
				remaining -= take;
				if (remaining <= 0) break;
			}

			pawn.abilities.GainAbility(entry.abilityDef);
			Messages.Message("ESCP_ShoutLearned".Translate(pawn.LabelShort.CapitalizeFirst(), entry.abilityDef.LabelCap),
				pawn, MessageTypeDefOf.PositiveEvent);
			return true;
		}
	}

	public class JobDriver_LearnShout : JobDriver
	{
		private int ShoutIndex => job.count;

		private ShoutEntry Entry
		{
			get
			{
				var ext = TargetA.Thing?.def.GetModExtension<WordWallExtension>();
				if (ext == null || ShoutIndex < 0 || ShoutIndex >= ext.shouts.Count)
					return null;
				return ext.shouts[ShoutIndex];
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed) =>
			pawn.Reserve(TargetA, job, 1, -1, null, errorOnFailed);

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnBurningImmobile(TargetIndex.A);
			this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			this.FailOn(() => Entry == null);

			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell)
				.FailOnDespawnedNullOrForbidden(TargetIndex.A);

			Toil meditate = ToilMaker.MakeToil("Meditate");
			meditate.WithProgressBarToilDelay(TargetIndex.A);
			meditate.FailOnDespawnedNullOrForbidden(TargetIndex.A);
			meditate.defaultCompleteMode = ToilCompleteMode.Delay;
			meditate.defaultDuration = Entry?.learnTicks ?? 3000;
			meditate.handlingFacing = true;
			meditate.tickAction = () =>
			{
				pawn.rotationTracker.FaceCell(TargetA.Thing.Position);
				pawn.GainComfortFromCellIfPossible(1, false);
				JoyUtility.JoyTickCheckEnd(pawn, 1);
			};
			yield return meditate;

			yield return Toils_General.Do(() =>
			{
				TargetA.Thing.TryGetComp<CompWordWall>()?.TryGrantShout(pawn, ShoutIndex);
			});
		}
	}

	public class Verb_FusRoDah : Verb_CastAbility
	{
		private FusRoDahExtension _props;
		private FusRoDahExtension Props => _props ??= ability?.def.GetModExtension<FusRoDahExtension>() ?? new FusRoDahExtension();

		protected override bool TryCastShot()
		{
			if (CasterPawn?.Map == null) return false;

			var props   = Props;
			var origin  = CasterPawn.Position;
			var map     = CasterPawn.Map;
			var originV = origin.ToVector3Shifted();

			Vector3 forward = (currentTarget.Cell.ToVector3Shifted() - originV).normalized;

			var targets = map.mapPawns.AllPawnsSpawned
				.Where(p =>
				{
					if (p == CasterPawn || p.Position == origin) return false;
					Vector3 d = p.Position.ToVector3Shifted() - originV;
					return d.magnitude <= props.coneRange
						&& Vector3.Angle(forward, d.normalized) <= props.coneHalfArc;
				})
				.ToList();

			if (targets.Count == 0) return true;

			foreach (var victim in targets)
			{
				victim.TakeDamage(new DamageInfo(DamageDefOf.Blunt, props.baseDamage,
					instigator: CasterPawn));

				if (!victim.Dead && victim.Spawned)
					LaunchPawn(victim, originV, props, map);
			}

			FleckMaker.ThrowLightningGlow(originV, map, 2.5f);
			return true;
		}

		private void LaunchPawn(Pawn victim, Vector3 casterV, FusRoDahExtension props, Map map)
		{
			Vector3 dir = (victim.Position.ToVector3Shifted() - casterV).normalized;
			if (dir == Vector3.zero)
				dir = new Vector3(0f, 0f, 1f);

			IntVec3 start   = victim.Position;
			var     path    = new List<IntVec3>();
			bool    hitWall = false;

			for (int i = 1; i <= props.knockbackCells; i++)
			{
				var c = new IntVec3(
					start.x + Mathf.RoundToInt(dir.x * i),
					0,
					start.z + Mathf.RoundToInt(dir.z * i));

				if (!c.InBounds(map))
					break;

				if (!c.Standable(map))
				{
					hitWall = true;
					break;
				}

				path.Add(c);
			}

			if (path.Count == 0) return;

			float landingDmg = props.baseDamage * 0.3f;
			if (hitWall)
				landingDmg += props.baseDamage * 0.35f;

			map.GetComponent<MapComponent_FusRoDahKnockback>()?.Enqueue(new KnockbackEntry
			{
				pawn         = victim,
				path         = path,
				ticksPerStep = props.ticksPerStep,
				landingDmg   = landingDmg,
				instigator   = CasterPawn,
			});
		}
	}

	public class FusRoDahExtension : DefModExtension
	{
		public float    coneRange      = 10f;
		public float    coneHalfArc    = 60f;
		public float    baseDamage     = 25f;
		public int      knockbackCells = 6;
		public int      ticksPerStep   = 4;
	}

	public class KnockbackEntry
	{
		public Pawn          pawn;
		public List<IntVec3> path;
		public int           pathIndex;
		public int           ticksPerStep;
		public int           tickCounter;
		public float         landingDmg;
		public Pawn          instigator;
		public bool          started;
	}

	public class MapComponent_FusRoDahKnockback : MapComponent
	{
		private readonly List<KnockbackEntry> _active = new();

		public MapComponent_FusRoDahKnockback(Map map) : base(map) { }

		public void Enqueue(KnockbackEntry e) => _active.Add(e);

		public override void MapComponentTick()
		{
			for (int i = _active.Count - 1; i >= 0; i--)
			{
				var e = _active[i];

				if (e.pawn.DestroyedOrNull() || e.pawn.Dead || !e.pawn.Spawned)
				{
					_active.RemoveAt(i);
					continue;
				}

				if (++e.tickCounter < e.ticksPerStep)
					continue;
				e.tickCounter = 0;

				var next = e.path[e.pathIndex++];

				e.pawn.Position = next;
				FleckMaker.ThrowDustPuff(next.ToVector3Shifted(), map, 0.6f);

				if (!e.started)
				{
					e.pawn.Notify_Teleported(endCurrentJob: true);
					e.started = true;
				}

				if (e.pathIndex >= e.path.Count)
				{
					e.pawn.Notify_Teleported(endCurrentJob: false);
					e.pawn.TakeDamage(new DamageInfo(DamageDefOf.Blunt, e.landingDmg, instigator: e.instigator));
					FleckMaker.ThrowDustPuff(e.pawn.Position.ToVector3Shifted(), map, 1.4f);
					_active.RemoveAt(i);
				}
			}
		}
	}

	public class PawnKindExtension_ThuumAbilities : DefModExtension
	{
		public List<AbilityDef> shouts = new();
	}

	internal static class ThuumInternalDefs
	{
		private static JobDef       _learnShout;
		private static ThingDef     _dragonSoul;
		private static GeneDef      _thuumVoiceGene;
		private static XenotypeDef  _nordXenotype;

		public static JobDef      LearnShout      => _learnShout      ??= DefDatabase<JobDef>.GetNamedSilentFail("ESCP_LearnShout");
		public static ThingDef    DragonSoul      => _dragonSoul      ??= DefDatabase<ThingDef>.GetNamedSilentFail("ESCP_DragonSoul");
		public static GeneDef     ThuumVoiceGene  => _thuumVoiceGene  ??= DefDatabase<GeneDef>.GetNamedSilentFail("ESCP_Gene_ThuumVoice");
		public static XenotypeDef NordXenotype    => _nordXenotype    ??= DefDatabase<XenotypeDef>.GetNamedSilentFail("ESCP_Xenotype_Nord");
	}

	[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
	static class Patch_NordRareThuumVoice
	{
		static void Postfix(Pawn __result)
		{
			if (__result?.genes == null) return;
			if (ThuumInternalDefs.NordXenotype == null || ThuumInternalDefs.ThuumVoiceGene == null) return;
			if (__result.genes.Xenotype != ThuumInternalDefs.NordXenotype) return;
			if (__result.genes.HasActiveGene(ThuumInternalDefs.ThuumVoiceGene)) return;
			if (Rand.Value > 0.001f) return;
			__result.genes.AddGene(ThuumInternalDefs.ThuumVoiceGene, false);
		}
	}

	[HarmonyPatch(typeof(PawnGenerator), nameof(PawnGenerator.GeneratePawn), new[] { typeof(PawnGenerationRequest) })]
	static class Patch_GrantThuumAbilities
	{
		static void Postfix(Pawn __result)
		{
			if (__result?.abilities == null || __result.kindDef == null) return;
			var ext = __result.kindDef.GetModExtension<PawnKindExtension_ThuumAbilities>();
			if (ext?.shouts == null || ext.shouts.Count == 0) return;
			foreach (var shout in ext.shouts)
			{
				if (__result.abilities.GetAbility(shout) == null)
					__result.abilities.GainAbility(shout);
			}
		}
	}
}
