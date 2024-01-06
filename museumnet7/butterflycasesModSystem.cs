using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace butterflycases
{
    public class ButterflyCasesMod : ModSystem
    {
        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("BlockButterflyCase", typeof(BlockButterflyCase));
            api.RegisterBlockEntityClass("BEButterflyBase", typeof(BEButterflyBase));
            api.RegisterBlockEntityClass("BEButterflyCaseWall", typeof(BEButterflyCaseWall));

            api.Logger.Notification("Butterfly Cases loaded: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.Logger.Notification("Butterfly Cases loaded server side: " + Lang.Get("butterflycases:true"));
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            api.Logger.Notification("Butterfly Cases loaded client side: " + Lang.Get("butterflycases:true"));
        }
    }
}
