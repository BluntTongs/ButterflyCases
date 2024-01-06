using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.ServerMods.NoObf;

namespace museumcases
{
    public class BEMuseumCaseSmol : BEMuseumBase, IRotatable
    {
        public override string InventoryClassName => "museumcasesmol";
        //protected InventoryGeneric inventory;
        //override InventoryBase Inventory => inventory;

        //new public float[] rotations = new float[1];
        //new public float[] vertrotations = new float[1];

       

        public BEMuseumCaseSmol()
        {
            inventory = new InventoryDisplayed(this, 1, "museumcasesmol-0", null, null);
        }

        new internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        {
            ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if (slot.Empty)
            {
                if (TryTake(byPlayer, blockSel))
                {
                    return true;
                }
                return false;
            }
            else
            {
                CollectibleObject colObj = slot.Itemstack.Collectible;
                if (colObj is ItemDisplayAdjuster) return false;
                if (colObj.Attributes != null && colObj.Attributes["displaycaseable"].AsBool(false) == true && (Block.Variant["type"] == "smol"))
                {
                    AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                    if (TryPut(slot, blockSel, byPlayer))
                    {
                        Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                        return true;
                    }

                    return false;
                }
                else if (colObj.Attributes != null && !colObj.WildCardMatch("*clothes*") && !colObj.WildCardMatch("*planks*") && !colObj.WildCardMatch("*armor*") && (Block.Variant["type"] == "smolnoglass")) 
                    //Armor does NOT look good, and won't without some solid tweaking. Also if you try putting planks in and using the adjuster tool they get deleted?
                {
                    AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

                    if (TryPut(slot, blockSel, byPlayer))
                    {
                        Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                        return true;
                    }

                    return false;
                }

                (Api as ICoreClientAPI)?.TriggerIngameError(this, "doesnotfit", Lang.Get("This item does not fit into the display case."));
                return true;
            }


        }


        new public void setBlockState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("type", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;

        }
        

        private bool TryPut(ItemSlot slot, BlockSelection blockSel, IPlayer player)
        {
            int index = blockSel.SelectionBoxIndex;
           
            var attr = slot.Itemstack.ItemAttributes;
            float height = attr?["museumcasesmol"]["minHeight"]?.AsFloat(0.25f) ?? 0;
            if (height > (this.Block as BlockMuseumCase)?.height)
            {
                (Api as ICoreClientAPI)?.TriggerIngameError(this, "tootall", Lang.Get("This item is too tall to fit in this display case."));
                return false;
            }


            if (inventory[index].Empty)
            {
                int moved = slot.TryPutInto(Api.World, inventory[index]);

                if (moved > 0)
                {
                    BlockPos targetPos = blockSel.DidOffset ? blockSel.Position.AddCopy(blockSel.Face.Opposite) : blockSel.Position;
                    double dx = player.Entity.Pos.X - (targetPos.X + blockSel.HitPosition.X);
                    double dy = (float)player.Entity.Pos.Y - (targetPos.Y + blockSel.HitPosition.Y);
                    double dz = (float)player.Entity.Pos.Z - (targetPos.Z + blockSel.HitPosition.Z);
                    float angleHor = (float)Math.Atan2(dx, dz);
                    float angleVer = (float)Math.Atan2(-dy, dz);
                    float deg45 = GameMath.PIHALF/2;
                    rotations[index] = (int)Math.Round(angleHor / deg45) * deg45;
                    vertrotations[index] = (int)Math.Round(angleVer * deg45) * deg45;
                     

                    updateMeshes();

                    MarkDirty(true);
                }

                return moved > 0;
            }

            return false;
        }
        //Checks whether an indexed inventory slot is empty

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;
       

            if (!inventory[index].Empty)
            {
                ItemStack stack = inventory[index].TakeOut(1);
                if (byPlayer.InventoryManager.TryGiveItemstack(stack))
                {
                    AssetLocation sound = stack.Block?.Sounds?.Place;
                    Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
                }

                if (stack.StackSize > 0)
                {
                    Api.World.SpawnItemEntity(stack, Pos.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                updateMesh(index);
                MarkDirty(true);
                return true;
            }

            return false;
        }



        //public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        //{
        //    base.GetBlockInfo(forPlayer, sb);

        //    sb.AppendLine();

        //    if (forPlayer?.CurrentBlockSelection == null) return;

        //    int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex;
        //    if (index >= inventory.Count) return; // Why can this happen o.O

        //    if (!inventory[index].Empty)
        //    {
        //        sb.AppendLine(inventory[index].Itemstack.GetName());
        //    }
        //}
        //Adds the item name to the tooltip box at the top of the screen if the display case has something in it.

        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[1][];

            for (int index = 0; index < 1; index++)
            {
         
                float degY = rotations[index] * GameMath.RAD2DEG;

                float rawdegX = vertrotations[index] * GameMath.RAD2DEG;
                float degX = GameMath.Clamp(rawdegX, 0, 30);


               
                    float x = 8 / 16f;
                    float y = 1 / 16f;
                    float z = 8 / 16f;

                

                if (inventory[index].Itemstack != null && inventory[index].Itemstack.Collectible is ItemDeadButterfly)
                    tfMatrices[index] =
                    new Matrixf()
                    .Translate(x , y + 0.2f, z)
                    .RotateYDeg(degY)
                    .RotateXDeg(degX)
                    .RotateYDeg(42f)
                    .Scale(0.75f, 0.75f, 0.75f)
                    .Translate(-0.5f, 0, -0.5f)
                    .Values;
                else if (inventory[index].Itemstack != null && inventory[index].Itemstack.Collectible.WildCardMatch("*cheese*") | inventory[index].Itemstack.Collectible.WildCardMatch("*cloth*"))
                    tfMatrices[index] =
                    new Matrixf()
                    .Translate(x, y + 0.15f, z)
                    .RotateYDeg(degY)
                    .RotateXDeg(degX)
                    .Scale(0.7f, 0.7f, 0.7f)
                    .Translate(-0.5f, 0, -0.5f)
                    .Values;
                else if (inventory[index].Itemstack != null && inventory[index].Itemstack.Block is Block)
                    tfMatrices[index] =
                    new Matrixf()
                    .Translate(x, y + 0.15f, z)
                    .RotateYDeg(degY)
                    .RotateXDeg(degX)
                    .Scale(0.9f, 0.9f, 0.9f)
                    .Translate(-0.5f, 0, -0.5f)
                    .Values;
                else
                    tfMatrices[index] =
                    new Matrixf()
                    .Translate(x, y + 0.15f, z)
                    .RotateYDeg(degY)
                    .RotateXDeg(degX)
                    .Scale(0.75f, 0.75f, 0.75f)
                    .Translate(-0.5f, 0, -0.5f)
                    .Values;

            }

            return tfMatrices;
        }


        public override void FromTreeAttributes(Vintagestory.API.Datastructures.ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);


            rotations = new float[]
            {
                tree.GetFloat("rotation0"),
            };
            vertrotations = new float[]
            {
                tree.GetFloat("vertrotation0"),
            };
        }

        public override void ToTreeAttributes(Vintagestory.API.Datastructures.ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetFloat("rotation0", rotations[0]);

            tree.SetFloat("vertrotation0", vertrotations[0]);

        }


        new public void OnTransformed(ITreeAttribute tree, int degreeRotation, EnumAxis? flipAxis)
        {
            var rot = new int[] { 0 };
            var verrot = new int[] { 0 };
            var rots = new float[1];
            var verrots = new float[1];
            var treeAttribute = tree.GetTreeAttribute("inventory");
            inventory.FromTreeAttributes(treeAttribute);
            var inv = new ItemSlot[1];
            var start = (degreeRotation / 90) % 1;

            for (var i = 0; i < 1; i++)
            {
                rots[i] = tree.GetFloat("rotation" + i);
                verrots[i] = tree.GetFloat("vertrotation" + i);
                inv[i] = inventory[i];
            }

            for (var i = 0; i < 1; i++)
            {
                var index = GameMath.Mod(i - start, 1);
                // swap inventory and rotations with the new ones
                rotations[rot[i]] = rots[rot[index]] - degreeRotation * GameMath.DEG2RAD;
                vertrotations[verrot[i]] = verrots[verrot[index]] - degreeRotation * GameMath.DEG2RAD;
                inventory[rot[i]] = inv[rot[index]];
                inventory[verrot[i]] = inv[verrot[index]];
                tree.SetFloat("rotation" + rot[i], rotations[rot[i]]);
                tree.SetFloat("vertrotation" + verrot[i], vertrotations[verrot[i]]);
            }

            inventory.ToTreeAttributes(treeAttribute);
            tree["inventory"] = treeAttribute;
        }
    }
}
