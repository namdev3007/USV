
using UnityEngine;

namespace Crest
{

    [AddComponentMenu(MENU_PREFIX + "Dynamic Waves Input")]
    [HelpURL(Internal.Constants.HELP_URL_BASE_USER + "waves.html" + Internal.Constants.HELP_URL_RP + "#dynamic-waves")]
    public class RegisterDynWavesInput : RegisterLodDataInput<LodDataMgrDynWaves>
    {
 
        [SerializeField, HideInInspector]
#pragma warning disable 414
        int _version = 0;
#pragma warning restore 414

        public override float Wavelength => 0f;

        public override bool Enabled => true;

        protected override Color GizmoColor => new Color(0f, 1f, 0f, 0.5f);

        protected override string ShaderPrefix => "Crest/Inputs/Dynamic Waves";

#if UNITY_EDITOR
        protected override string FeatureToggleName => LodDataMgrDynWaves.FEATURE_TOGGLE_NAME;
        protected override string FeatureToggleLabel => LodDataMgrDynWaves.FEATURE_TOGGLE_LABEL;
        protected override bool FeatureEnabled(OceanRenderer ocean) => ocean.CreateDynamicWaveSim;
#endif
    }
}
