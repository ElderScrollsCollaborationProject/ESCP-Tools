using System.Collections.Generic;
using Verse;

namespace ESCP_Birthsigns
{
    public class BirthsignSetDef : Def
    {
        public List<List<HediffDef>> birthsignHediffs;
        public List<HediffDef> additionalSigns;
        public float additionalSignsChance = 0.08f;
        public float hasBirthsign_DefaultChance = 1f;
        public List<string> overridenQuadrumNames;
        public List<string> overridenQuadrumNamesShort;

        public override IEnumerable<string> ConfigErrors()
        {
            foreach (string text in base.ConfigErrors())
            {
                yield return text;
            }
            if (label == null || label == "")
            {
                yield return "label is null or blank";
            }
            if (description == null || description == "")
            {
                yield return "desciption is null or blank";
            }

            if (!overridenQuadrumNames.NullOrEmpty())
            {
                if (overridenQuadrumNames.Count != 4)
                {
                    yield return "overridenQuadrumNames must either contain four items, or be left null/empty";
                }
                if (overridenQuadrumNamesShort.NullOrEmpty())
                {
                    yield return "overridenQuadrumNamesShort must also be defined if overridenQuadrumNames is";
                }
            }
            if (!overridenQuadrumNamesShort.NullOrEmpty())
            {
                if (overridenQuadrumNamesShort.Count != 4)
                {
                    yield return "overridenQuadrumNamesShort must either contain four items, or be left null/empty";
                }
                if (overridenQuadrumNamesShort.NullOrEmpty())
                {
                    yield return "overridenQuadrumNames must also be defined if overridenQuadrumNamesShort is";
                }
            }

            bool flag = false;
            if (birthsignHediffs.NullOrEmpty())
            {
                yield return "birthsignHediffs is null or empty";
                flag = true;
            }
            else
            {
                if (birthsignHediffs.Count != 4)
                {
                    yield return "birthsignHediffs does not contain exactly 4 season groups";
                    flag = true;
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (birthsignHediffs[i].Count != 3)
                        {
                            yield return "birthsignHediffs does not contain exactly 3 hediffs in season group: " + (i+1) + ", starting with: " + (birthsignHediffs[i].NullOrEmpty() ? "Null" : birthsignHediffs[i][0].defName);
                            flag = true;
                        }
                    }
                }
            }
            if (flag)
            {
                yield return "errors found, this will cause out of bounds exceptions";
            }
            yield break;
        }

        public bool QuadrumNameReplacerValid()
        {
            return !overridenQuadrumNames.NullOrEmpty() && overridenQuadrumNames.Count == 4 && !overridenQuadrumNamesShort.NullOrEmpty() && overridenQuadrumNamesShort.Count == 4;
        }
    }
}
