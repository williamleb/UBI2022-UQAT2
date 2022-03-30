#if FUSION_WEAVER && FUSION_WEAVER_ILPOSTPROCESSOR
namespace Fusion.CodeGen {
  static partial class ILWeaverSettings {

    static partial void OverrideNetworkProjectConfigPath(ref string path)
    {
      path = "Assets\\Plugins\\Photon\\Fusion\\Resources\\NetworkProjectConfig.fusion";
    }
  }
}
#endif