using ComponentAce.Compression.Libs.zlib;
using MonoMod.Utils;
using SIT.Core.Misc;
using SIT.Tarkov.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace SIT.Core.Core.Web
{
    //internal class TarkovTransportHttpMethodDebugPatch : ModulePatch
    //{
    //    protected override MethodBase GetTargetMethod()
    //    {
    //        return ReflectionHelpers.GetMethodForType(typeof(TarkovRequestTransportHttp), "method_5");
    //    }

    //    [PatchPrefix]
    //    public static void Prefix(TarkovRequest backRequest, BackResponse bResponse)
    //    {

    //        Logger.LogDebug(Pooled9LevelZLib.DecompressNonAlloc(backRequest.Data, backRequest.Data.Length));

    //        if (bResponse == null)
    //        {
    //            Logger.LogError("bResponse is NULL");
    //            return;
    //        }

    //        if (bResponse.responseHeaders == null)
    //            Logger.LogError("bResponse is NULL");

    //        foreach (var h in bResponse.responseHeaders)
    //        {
    //            Logger.LogDebug(h);
    //        }
    //    }
    //}

    //internal class TarkovTransportHttpMethodDebugPatch2 : ModulePatch
    //{
    //    protected override MethodBase GetTargetMethod()
    //    {
    //        var t = PatchConstants.EftTypes.FirstOrDefault(x => x.Name.StartsWith("Class244"));
    //        return ReflectionHelpers.GetMethodForType(t, "method_1");
    //    }

    //    [PatchPostfix]
    //    public static void Postfix(TarkovRequest ___backRequest, BackResponse ___responseData)
    //    {
    //        Logger.LogInfo(___backRequest);
    //        Logger.LogInfo(___responseData);
    //    }
    //}

    internal class TarkovTransportHttpMethodDebugPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            var t = PatchConstants.EftTypes.FirstOrDefault(x => x.Name.StartsWith("Class247"));
            return ReflectionHelpers.GetMethodForType(t, "method_0");
        }

        [PatchPostfix]
        public static void Postfix(string url, UnityWebRequest request, Stopwatch stopwatch)
        {
            //Logger.LogInfo(url);
            //Logger.LogInfo(request);
            //Logger.LogInfo(request.isHttpError);
            //Logger.LogInfo(request.isNetworkError);
            //Logger.LogInfo(request.isModifiable);
            //Logger.LogInfo(request.error);
            //Logger.LogInfo(request.responseCode);
        }
    }

    
}
