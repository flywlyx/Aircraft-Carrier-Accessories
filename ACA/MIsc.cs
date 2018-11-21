using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;
using System.Linq;

namespace SPS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class ACAEditorCategory : MonoBehaviour
    {
        private static readonly List<AvailablePart> availableParts = new List<AvailablePart>();

        void Awake()
        {
            GameEvents.onGUIEditorToolbarReady.Add(ACAWeaponsCategory);

        }


        void ACAWeaponsCategory()
        {
            const string customCategoryName = "ACA";
            const string customDisplayCategoryName = "Aircraft Carrier Accessories";

            availableParts.Clear();
            availableParts.AddRange(PartLoader.LoadedPartsList.ACAParts());

            Debug.Log(availableParts.Count);

            Texture2D iconTex = GameDatabase.Instance.GetTexture("KFC/Textures/icon", false);

            RUI.Icons.Selectable.Icon icon = new RUI.Icons.Selectable.Icon("ACA", iconTex, iconTex, false);

            PartCategorizer.Category filter = PartCategorizer.Instance.filters.Find(f => f.button.categorydisplayName == "#autoLOC_453547");

            PartCategorizer.AddCustomSubcategoryFilter(filter, customCategoryName, customDisplayCategoryName, icon,
                p => availableParts.Contains(p));

        }

    }

    public class ACACategoryModule : PartModule
    {
        //dummy
    }

    public static class ACAExtensions
    {
        public static IEnumerable<AvailablePart> ACAParts(this List<AvailablePart> parts)
        {
            return (from avPart in parts.Where(p => p.partPrefab)
                    let Tiedown = avPart.partPrefab.GetComponent<Tiedown>()
                    let FuelCap = avPart.partPrefab.GetComponent<FuelCap>()
                    let FuelNozzle = avPart.partPrefab.GetComponent<FuelNozzle>()
                    let CLLS = avPart.partPrefab.GetComponent<CLLS>()
                    let FixPoint = avPart.partPrefab.GetComponent<FixPoint>()
                    let ACE = avPart.partPrefab.GetComponent<ACE>()
                    let Catapult = avPart.partPrefab.GetComponent<Catapult>()
                    let Arrestwire = avPart.partPrefab.GetComponent<Arrestwire>()
                    let ReloadableRail = avPart.partPrefab.GetComponent<ReloadableRail>()
                    let Tailhook = avPart.partPrefab.GetComponent<Tailhook>()
                    let OLS = avPart.partPrefab.GetComponent<OLS>()
                    let otherModule = avPart.partPrefab.GetComponent<ACACategoryModule>()
                    where Tiedown || FuelCap || FuelNozzle || CLLS || FixPoint || ACE || Catapult || Arrestwire || ReloadableRail|| Tailhook|| OLS|| otherModule
                    select avPart).AsEnumerable();
        }
    }
}
