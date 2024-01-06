using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;

namespace museumcases
{
    public class BlockMuseumCase : Block
    {
        WorldInteraction[] interactions;
        public float height;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            height = Attributes["height"].AsFloat(0.5f);

            if (api.Side != EnumAppSide.Client) return;
            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "displayCaseInteractions", () =>

            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        MouseButton = EnumMouseButton.Right,
                        ActionLangCode = "blockhelp-displaycase-place",
                    },
                    new WorldInteraction()
                    {
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true,
                        ActionLangCode = "blockhelp-displaycase-remove",
                    }
                };
            });
        }
        public override bool DoParticalSelection(IWorldAccessor world, BlockPos pos)
        {
            return true;
        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            BEMuseumCase bemc = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMuseumCase;
            BEMuseumCaseSmol bemcs = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMuseumCaseSmol;
            BEMuseumCaseWall bemcw = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMuseumCaseWall;
            BEMuseumCaseButterfly bemcb = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMuseumCaseButterfly;
            BEMuseumCaseTall bemct = world.BlockAccessor.GetBlockEntity(blockSel.Position) as BEMuseumCaseTall;
            if (bemc != null) return bemc.OnInteract(byPlayer, blockSel);
            if (bemcs != null) return bemcs.OnInteract(byPlayer, blockSel);
            if (bemcw != null) return bemcw.OnInteract(byPlayer, blockSel);
            if (bemcb != null) return bemcb.OnInteract(byPlayer, blockSel);
            if (bemct != null) return bemct.OnInteract(byPlayer, blockSel);

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }



        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            return interactions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}
