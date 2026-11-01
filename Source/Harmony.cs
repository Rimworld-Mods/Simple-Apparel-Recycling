using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Sound;
using Verse.Noise;
using Verse.Grammar;
using RimWorld;
using RimWorld.Planet;

using System.Reflection;
using HarmonyLib;

namespace AVSR
{
    [HarmonyPatch(typeof(GenRecipe), "MakeRecipeProducts")]
    public static class RecycleRecipeProductsPatch
    {
        private static IEnumerable<Thing> Recycle(Thing apparel){
            float multiplier = Settings.isDurabilityInfluenced ? apparel.HitPoints / (float)apparel.MaxHitPoints : 1.0f;
            foreach(ThingDefCountClass t in apparel.CostListAdjusted()){
                int count = (int)Mathf.Floor(t.count * Settings.efficiency * multiplier);
                if(count == 0 || t.thingDef.intricate) continue; // intricate materials are rare items, like components
                Thing product = ThingMaker.MakeThing(t.thingDef, null);
                product.stackCount = count;
                yield return product;
            }
        }

        public static void Postfix(ref RecipeDef recipeDef, ref List<Thing> ingredients, ref IEnumerable<Thing> __result)
        {
            string name = recipeDef.defName;
            string[] recycleDefs = {
                "SimpleRecycleApparel",
                "NeolithicRecycleApparel",
                "BulkRecycleApparel",
            };
            if(recycleDefs.Contains(name)){
                foreach(Thing apparel in ingredients){
                    // if apparel is smeltable - do nothing, smelt functionionality will return materials
                    if(apparel == null || apparel.Smeltable) continue;
                    __result = __result.Concat(Recycle(apparel));
                }
            }
        }

        // TODO: Create Process.GetThingsAndContainedItems harmony patch (after) and filter smeltable items further for VFEFactory
    }
}