using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using Vintagestory.ServerMods.NoObf;

namespace butterflycases
{
    public class BEButterflyCaseWallSmall : BEButterflyBase, IRotatable
    {
        public override string InventoryClassName => "butterflycasewallsmall";
        //protected InventoryGeneric inventory;
        //public override InventoryBase Inventory => inventory;

        //bool haveCenterPlacement;
        //float[] rotations = new float[4];
        //float[] vertrotations = new float[4];


        public BEButterflyCaseWallSmall()
        {
            inventory = new InventoryDisplayed(this, 1, "butterflycasewallsmall-0", null, null);
        }

        //internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
        //{
        //    ItemSlot slot = byPlayer.InventoryManager.ActiveHotbarSlot;


        //    if (slot.Empty)
        //    {
        //        if (TryTake(byPlayer, blockSel))
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //    else
        //    {
        //        CollectibleObject colObj = slot.Itemstack.Collectible;
        //        if (colObj.Attributes != null && colObj.Attributes["displaycaseable"].AsBool(false) == true)
        //        {
        //            AssetLocation sound = slot.Itemstack?.Block?.Sounds?.Place;

        //            if (TryPut(slot, blockSel, byPlayer))
        //            {
        //                Api.World.PlaySoundAt(sound != null ? sound : new AssetLocation("sounds/player/build"), byPlayer.Entity, byPlayer, true, 16);
        //                return true;
        //            }

        //            return false;
        //        }

        //        (Api as ICoreClientAPI)?.TriggerIngameError(this, "doesnotfit", Lang.Get("This item does not fit into the display case."));
        //        return true;
        //    }
        //}




        new public void setBlockState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("type", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;
        }
        BlockFacing getFacing()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            BlockFacing facing = BlockFacing.FromCode(block.LastCodePart());
            return facing == null ? BlockFacing.NORTH : facing;
        }
        public float rotAdder()
        {

            BlockFacing displayFacing = getFacing();
            float rotAdder = 1;
            if (displayFacing == BlockFacing.NORTH) rotAdder = 0f;
            if (displayFacing == BlockFacing.EAST) rotAdder = 4.71f;
            if (displayFacing == BlockFacing.SOUTH) rotAdder = 3.14f;
            if (displayFacing == BlockFacing.WEST) rotAdder = 1.57f;

            return rotAdder;

        }
        
        public float originOffsetSides()
        {
            BlockFacing displayFacing = getFacing();
            float originOffsetSides = 0f;
            if (displayFacing == BlockFacing.NORTH) originOffsetSides = 0f;
            if (displayFacing == BlockFacing.EAST) originOffsetSides = 0f;
            if (displayFacing == BlockFacing.SOUTH) originOffsetSides = -1f;
            if (displayFacing == BlockFacing.WEST) originOffsetSides = -1f;

            return originOffsetSides;
        }

        public float originOffsetDepths()
        {
            BlockFacing displayFacing = getFacing();
            float originOffsetDepths = 0f;
            if (displayFacing == BlockFacing.NORTH) originOffsetDepths = 0f;
            if (displayFacing == BlockFacing.EAST) originOffsetDepths = -1f;
            if (displayFacing == BlockFacing.SOUTH) originOffsetDepths = -1f;
            if (displayFacing == BlockFacing.WEST) originOffsetDepths = 0f;

            return originOffsetDepths;
        }



        private bool TryPut(ItemSlot slot, BlockSelection blockSel, IPlayer player)
        {
            int index = blockSel.SelectionBoxIndex;
            bool nowCenterPlacement = inventory.Empty && (Math.Abs(blockSel.HitPosition.Z - 0.5f) < 0.1f && Math.Abs(blockSel.HitPosition.Y - 0.5f) < 0.1f)
                || inventory.Empty && (Math.Abs(blockSel.HitPosition.X - 0.5f) < 0.1f && Math.Abs(blockSel.HitPosition.Y - 0.5f) < 0.1f);

            var attr = slot.Itemstack.ItemAttributes;
            float height = attr?["butterflycase"]["minHeight"]?.AsFloat(0.25f) ?? 0;
            if (height > (this.Block as BlockButterflyCase)?.height)
            {
                (Api as ICoreClientAPI)?.TriggerIngameError(this, "tootall", Lang.Get("This item is too tall to fit in this display case."));
                return false;
            }


            haveCenterPlacement = nowCenterPlacement;

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
                    //float angleVer = (float)Math.Atan2(-dy, dz);
                    //float deg90 = GameMath.PIHALF;

                    //rotations[index] = (int)Math.Round(angleHor / deg90) * deg90;
                    //vertrotations[index] = (int)Math.Round(angleVer * deg90) * deg90;


                    updateMeshes();

                    MarkDirty(true);
                }

                return moved > 0;
            }

            return false;
        }

        private bool TryTake(IPlayer byPlayer, BlockSelection blockSel)
        {
            int index = blockSel.SelectionBoxIndex;
            if (haveCenterPlacement)
            {
                for (int i = 0; i < inventory.Count; i++)
                {
                    if (!inventory[i].Empty) index = i;
                }
            }

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


        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[1][];



            for (int index = 0; index < 1; index++)
            {

                float x = 8.3f / 16f;
                float y = 5 / 16f;
                float z = 4 / 16f;


                float originRot = rotAdder();
                //float originMult = 4f;
                float originAdd = originOffsetSides();
                float originAdd2 = originOffsetDepths();

                float degY = rotations[index];// * GameMath.RAD2DEG;
                float rawdegX = vertrotations[index] * GameMath.RAD2DEG;

                float degX = GameMath.Clamp(rawdegX, 90, 90);


                    if (haveCenterPlacement)
                    {
                        x = 8f / 16f;
                        y = 5.5f / 16f;
                    }

                    if (inventory[index].Itemstack != null && inventory[index].Itemstack.Collectible is ItemDeadButterfly)
                        tfMatrices[index] =
                        new Matrixf()
                        .RotateY(originRot)
                        .Translate(x + originAdd, y + 0.17f, z + originAdd2 - 0.17f)
                        //.RotateYDeg(degY)
                        .RotateXDeg(degX)
                        .RotateYDeg(42f)
                        .Scale(0.85f, 0.85f, 0.85f)
                        .Translate(-0.5f, 0, -0.5f)
                        .Values;
                    else
                        tfMatrices[index] =
                        new Matrixf()
                        .RotateY(originRot)
                        .Translate(x + originAdd - 0.01f, y + 0.17f, z + originAdd2 - 0.17f)
                        //.RotateYDeg(degY)
                        .RotateXDeg(degX - 4f)
                        .RotateYDeg(42f)
                        .Scale(0.80f, 0.75f, 0.75f)
                        .Translate(-0.5f, 0, -0.5f)
                        .Values;

            }
            return tfMatrices;
        }


        public override void FromTreeAttributes(Vintagestory.API.Datastructures.ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);

            haveCenterPlacement = tree.GetBool("haveCenterPlacement");
            rotations = new float[]
            {
                tree.GetFloat("rotation0"),
                tree.GetFloat("rotation1"),
                tree.GetFloat("rotation2"),
                tree.GetFloat("rotation3"),
            };
            vertrotations = new float[]
            {
                tree.GetFloat("vertrotation0"),
                tree.GetFloat("vertrotation1"),
                tree.GetFloat("vertrotation2"),
                tree.GetFloat("vertrotation3"),
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
