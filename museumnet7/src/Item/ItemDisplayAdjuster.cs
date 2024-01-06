using System;
using System.Threading;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace museumcases
{
    public class ItemDisplayAdjuster : Item
    {
        SkillItem[] modes;
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            var capi = api as ICoreClientAPI;

            modes = new SkillItem[]
            {
                new SkillItem()
                {
                    Code = new AssetLocation("spin"),
                    Name = ("Spin")
                },
                new SkillItem()
                {
                    Code = new AssetLocation("tilt"),
                    Name = ("Tilt")
                },
                new SkillItem()
                {
                    Code = new AssetLocation("pegtoggle"),
                    Name = ("Toggle Peg")
                },

            };
            if (capi != null)
            {
                modes[0].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("game:textures/icons/scythetrim.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[0].TexturePremultipliedAlpha = false;
                modes[1].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("game:textures/icons/scytheremove.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[1].TexturePremultipliedAlpha = false;
                modes[2].WithIcon(capi, capi.Gui.LoadSvgWithPadding(new AssetLocation("game:textures/icons/scythetrim.svg"), 48, 48, 5, ColorUtil.WhiteArgb));
                modes[2].TexturePremultipliedAlpha = false;

            }
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            return base.GetHeldInteractionHelp(inSlot).Append(new WorldInteraction()
            {
                ActionLangCode = "heldhelp-settoolmode",
                HotKeyCode = "toolmodeselect"
            });
        }
        public override SkillItem[] GetToolModes(ItemSlot slot, IClientPlayer forPlayer, BlockSelection blockSel)
        {
            return modes;
        }
        public override void SetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection, int toolMode)
        {
            slot.Itemstack.Attributes.SetInt("toolMode", toolMode);
        }

        public override int GetToolMode(ItemSlot slot, IPlayer byPlayer, BlockSelection blockSelection)
        {
            return slot.Itemstack.Attributes.GetInt("toolMode", 0);
        }
        public override void OnUnloaded(ICoreAPI api)
        {
            for (int i = 0; modes != null && i < modes.Length; i++)
            {
                modes[i]?.Dispose();
            }
        }


        public override void OnHeldAttackStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, ref EnumHandHandling handling)
        {
            base.OnHeldAttackStart(slot, byEntity, blockSel, entitySel, ref handling);
            if (handling == EnumHandHandling.PreventDefault) return;
            if (blockSel == null) return;

            int toolMode = slot.Itemstack.Attributes.GetInt("toolMode");

            var pos = blockSel.Position;
            var block = api.World.BlockAccessor.GetBlock(pos);
            var be = api.World.BlockAccessor.GetBlockEntity(pos);
            //Gets the block at the selected position.

            float deg45 = GameMath.PIHALF / 2;

            var i = blockSel.SelectionBoxIndex;

            if (block is BlockMuseumCase && toolMode == 0)
            {
                BEMuseumBase bemc = (BEMuseumBase)be;
                bemc.rotValue[i] = bemc.rotValue[i] - deg45;
                bemc.updateMeshes();
                bemc.MarkDirty(true);
            }
            else if (block is BlockMuseumCase && toolMode == 1)
            {
                BEMuseumBase bemc = (BEMuseumBase)be;
                if (bemc.verrotValue[i] <= 90)
                    bemc.verrotValue[i] = bemc.verrotValue[i] + deg45;
                else bemc.verrotValue[i] = bemc.verrotValue[i];
                bemc.updateMeshes();
                bemc.MarkDirty(true);
            }
            else if (block is BlockMuseumCase && toolMode == 2)
            {
                BEMuseumBase bemc = (BEMuseumBase)be;
                if (bemc.pegValue[i] == false)
                    bemc.pegValue[i] = bemc.pegValue[i] == true;
                else if (bemc.pegValue[i] == true)
                    bemc.pegValue[i] = bemc.pegValue[i] == false;
                bemc.updateMeshes();
                bemc.MarkDirty(true);
            }

            handling = EnumHandHandling.PreventDefault;
        }
      
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            if (handling == EnumHandHandling.PreventDefault) return;
            if (blockSel == null) return;

            int toolMode = slot.Itemstack.Attributes.GetInt("toolMode");

            var pos = blockSel.Position;
            var block = api.World.BlockAccessor.GetBlock(pos);
            var be = api.World.BlockAccessor.GetBlockEntity(pos);


            //Gets the block at the selected position.

            float deg45 = GameMath.PIHALF / 2;

            var i = blockSel.SelectionBoxIndex;
            //Gets the index of the selection box you're looking at. In our case we'll likely only be using 0-7, and we only want one at a time. IT WORKS!!!

            if (block is BlockMuseumCase && toolMode == 0)
            {
                BEMuseumBase bemc = (BEMuseumBase)be;
                bemc.rotValue[i] = bemc.rotValue[i] + deg45;
                bemc.updateMeshes();
                bemc.MarkDirty(true);
            }
            else if (block is BlockMuseumCase && toolMode == 1)
            {
                BEMuseumBase bemc = (BEMuseumBase)be;
                if (bemc.verrotValue[i] <= 45)
                    bemc.verrotValue[i] = bemc.verrotValue[i] - deg45;
                else bemc.verrotValue[i] = bemc.verrotValue[i];
                bemc.updateMeshes();
                bemc.MarkDirty(true);
            }
            else if (block is BlockMuseumCase && toolMode == 2)
            {
                BEMuseumBase bemc = (BEMuseumBase)be;
                if (bemc.pegValue[i] == false)
                    bemc.pegValue[i] = bemc.pegValue[i] == true;
                else if (bemc.pegValue[i] == true)
                    bemc.pegValue[i] = bemc.pegValue[i] == false;
                bemc.updateMeshes();
                bemc.MarkDirty(true);
            }

            handling = EnumHandHandling.PreventDefault;
        }
    }

}
