using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using Verse;

namespace Randolph_Cherrypepper
{

    public class CompProperties_WearableExplosive : CompProperties
    {

        public float explosiveRadius = 1.9f;

        public DamageDef explosiveDamageType;

        public ThingDef postExplosionSpawnThingDef;

        public float postExplosionSpawnChance;

        public int postExplosionSpawnThingCount = 1;

        public bool applyDamageToExplosionCellsNeighbors = true;

        public ThingDef preExplosionSpawnThingDef;

        public float preExplosionSpawnChance;

        public int preExplosionSpawnThingCount = 1;

        public float explosiveExpandPerStackcount;

        public EffecterDef explosionEffect;

        /* Scales how likely this will explode when it takes damage.
         *   stability = MaxHitPoints cannot detonate until full destruction (0 hitpoints)
         *   stability = 2 cannot detonate until it has lost at least half its HP
         *   stability = 0.5 means there's at least 50/50 chance of exploding on the first hit
         *   stability = 0 could lead to an explosion without taking any damage in theory (but not in practice)
         */
        public float stability = 1.1f;

        public CompProperties_WearableExplosive()
        {
            this.compClass = typeof(CompWearableExplosive);
        }
    }

    public class CompWearableExplosive : CompWearable
    {

        public CompProperties_WearableExplosive Props => (CompProperties_WearableExplosive)props;

        // Determine who is wearing this ThingComp. Returns a Pawn or null.
        protected virtual Pawn GetWearer
        {
            get
            {
                if (ParentHolder != null && ParentHolder is Pawn_ApparelTracker)
                {
                    return (Pawn)ParentHolder.ParentHolder;
                }
                else
                {
                    return null;
                }
            }
        }

        // Determine if this ThingComp is being worn presently. Returns True/False
        protected virtual bool IsWorn => (GetWearer != null);

        // heavily based on Rimworld.CompExplosive.PostPreApplyDamage()
        public override void PostPreApplyDamage(DamageInfo dinfo, out bool absorbed)
        {
            absorbed = false;

            if (dinfo.Def.externalViolence)
            {
                if (dinfo.Amount >= parent.HitPoints)
                {
                    //Explode immediately from excessive incoming damage
                    Detonate();
                    absorbed = true;
                }
                else
                {
                    // As the hit points go down, explosion is more likely.
                    if (Rand.Value > (Props.stability * parent.HitPoints / parent.MaxHitPoints))
                    {
                        Detonate();
                        absorbed = true;
                    }
                }
            }
            // Currently ignores dinfo.Instigator
        }

        // heavily based on Rimwold.CompExplosive.Detonate()
        protected virtual void Detonate()
        {
            if (!parent.SpawnedOrAnyParentSpawned)
                return;

            // Use the wearing Pawn if there is one, otherwise use parent for determining location.
            ThingWithComps owner = IsWorn ? GetWearer : parent;
            Map map = owner.MapHeld;
            IntVec3 loc = owner.PositionHeld;

            if (!parent.Destroyed)
                parent.Kill();

            if (map == null)
            {
                Log.Warning("Tried to detonate CompWearableExplosive in a null map.");
                return;
            }
            if (loc == null)
            {
                Log.Warning("Tried to detonate CompWearableExplosive at a null position.");
                return;
            }

            var props = Props;

            //Expand radius for stackcount
            float radius = props.explosiveRadius;
            if (!IsWorn && parent.stackCount > 1 && props.explosiveExpandPerStackcount > 0)
                radius += Mathf.Sqrt((parent.stackCount - 1) * props.explosiveExpandPerStackcount);

            if (props.explosionEffect != null)
            {
                var effect = props.explosionEffect.Spawn();
                effect.Trigger(new TargetInfo(parent.PositionHeld, map), new TargetInfo(parent.PositionHeld, map));
                effect.Cleanup();
            }

            GenExplosion.DoExplosion(loc,
                map,
                radius,
                props.explosiveDamageType,
                parent,
                postExplosionSpawnThingDef: props.postExplosionSpawnThingDef,
                postExplosionSpawnChance: props.postExplosionSpawnChance,
                postExplosionSpawnThingCount: props.postExplosionSpawnThingCount,
                applyDamageToExplosionCellsNeighbors: props.applyDamageToExplosionCellsNeighbors,
                preExplosionSpawnThingDef: props.preExplosionSpawnThingDef,
                preExplosionSpawnChance: props.preExplosionSpawnChance,
                preExplosionSpawnThingCount: props.preExplosionSpawnThingCount);
        }

        public override IEnumerable<Gizmo> CompGetGizmosWorn()
        {
            yield return new Command_Action
            {
                action = Detonate,
                defaultLabel = "WearableExplosives_Label".Translate(),
                defaultDesc = "WearableExplosives_Desc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Buttons/BlastFlame", true)
            };
        }
    }
}