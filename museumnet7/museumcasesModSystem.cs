using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace museumcases
{
    public class MuseumMod : ModSystem
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("BlockMuseumCase", typeof(BlockMuseumCase));
            api.RegisterBlockEntityClass("BEMuseumBase", typeof(BEMuseumBase));
            api.RegisterBlockEntityClass("BEMuseumCase", typeof(BEMuseumCase));
            api.RegisterBlockEntityClass("BEMuseumCaseSmol", typeof(BEMuseumCaseSmol));
            api.RegisterBlockEntityClass("BEMuseumCaseWall", typeof(BEMuseumCaseWall));
            api.RegisterBlockEntityClass("BEMuseumCaseButterfly", typeof(BEMuseumCaseButterfly));
            api.RegisterBlockEntityClass("BEMuseumCaseTall", typeof(BEMuseumCaseTall));

            api.RegisterItemClass("ItemDisplayAdjuster", typeof(ItemDisplayAdjuster));

            api.Logger.Notification("Museum Cases loaded: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Logger.Notification("Museum Cases loaded server side: " + Lang.Get("museumcases:true"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Logger.Notification("Museum Cases loaded client side: " + Lang.Get("museumcases:true"));
        }
    }
}
