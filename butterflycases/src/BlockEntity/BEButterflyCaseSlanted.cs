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
    public class BEButterflyCaseSlanted : BEButterflyBase, IRotatable
    {
        public override string InventoryClassName => "butterflycaseslanted";

        public BEButterflyCaseSlanted()
        {
            inventory = new InventoryDisplayed(this, 4, "butterflycaseslanted-0", null, null);
        }

        new public void SetBlockState(string state)
        {
            AssetLocation loc = Block.CodeWithVariant("type", state);
            Block block = Api.World.GetBlock(loc);
            if (block == null) return;

            Api.World.BlockAccessor.ExchangeBlock(block.Id, Pos);
            this.Block = block;
        }
        BlockFacing GetFacing()
        {
            Block block = Api.World.BlockAccessor.GetBlock(Pos);
            BlockFacing facing = BlockFacing.FromCode(block.LastCodePart());
            return facing == null ? BlockFacing.NORTH : facing;
        }
        public float RotAdder()
        {

            BlockFacing displayFacing = GetFacing();
            float RotAdder = 1;
            if (displayFacing == BlockFacing.NORTH) RotAdder = 0f;
            if (displayFacing == BlockFacing.EAST) RotAdder = 4.71f;
            if (displayFacing == BlockFacing.SOUTH) RotAdder = 3.14f;
            if (displayFacing == BlockFacing.WEST) RotAdder = 1.57f;

            return RotAdder;

        }
        
        public float OriginOffsetSides()
        {
            BlockFacing displayFacing = GetFacing();
            float OriginOffsetSides = 0f;
            if (displayFacing == BlockFacing.NORTH) OriginOffsetSides = 0f;
            if (displayFacing == BlockFacing.EAST) OriginOffsetSides = 0f;
            if (displayFacing == BlockFacing.SOUTH) OriginOffsetSides = -1f;
            if (displayFacing == BlockFacing.WEST) OriginOffsetSides = -1f;

            return OriginOffsetSides;
        }

        public float OriginOffsetDepths()
        {
            BlockFacing displayFacing = GetFacing();
            float OriginOffsetDepths = 0f;
            if (displayFacing == BlockFacing.NORTH) OriginOffsetDepths = 0f;
            if (displayFacing == BlockFacing.EAST) OriginOffsetDepths = -1f;
            if (displayFacing == BlockFacing.SOUTH) OriginOffsetDepths = -1f;
            if (displayFacing == BlockFacing.WEST) OriginOffsetDepths = 0f;

            return OriginOffsetDepths;
        }


        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[4][];



            for (int index = 0; index < 4; index++)
            {

                float x = (index % 2 == 0) ? 4.5f / 16f : 11.5f / 16f;
                float y = (index < 2) ? 6f / 16f : 1f / 16f;
                float z = (index > 1) ? 15f / 16f : 10f / 16f;


                float originRot = RotAdder();
                float originAdd = OriginOffsetSides();
                float originAdd2 = OriginOffsetDepths();

                float degY = rotations[index];
                float rawdegX = vertrotations[index] * GameMath.RAD2DEG;

                float degX = GameMath.Clamp(rawdegX, 45, 45);


                    if (haveCenterPlacement)
                    {
                        x = 8f / 16f;
                        y = 5.5f / 16f;
                    }

                    if (inventory[index].Itemstack != null && inventory[index].Itemstack.Collectible is ItemDeadButterfly)
                        tfMatrices[index] =
                        new Matrixf()
                        .RotateY(originRot)
                        .Translate(x + originAdd + 0.01f, y + 0.17f, z + originAdd2 - 0.17f)
                        .RotateXDeg(degX)
                        .RotateYDeg(42f)
                        .Scale(0.85f, 0.85f, 0.85f)
                        .Translate(-0.5f, 0, -0.5f)
                        .Values;
                    else
                        tfMatrices[index] =
                        new Matrixf()
                        .RotateY(originRot)
                        .Translate(x + originAdd + 0.01f, y + 0.17f, z + originAdd2 - 0.17f)
                        .RotateXDeg(degX)
                        .RotateYDeg(42f)
                        .Scale(0.85f, 0.85f, 0.85f)
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

            tree.SetBool("haveCenterPlacement", haveCenterPlacement);
            tree.SetFloat("rotation0", rotations[0]);
            tree.SetFloat("rotation1", rotations[1]);
            tree.SetFloat("rotation2", rotations[2]);
            tree.SetFloat("rotation3", rotations[3]);

            tree.SetFloat("vertrotation0", vertrotations[0]);
            tree.SetFloat("vertrotation1", vertrotations[1]);
            tree.SetFloat("vertrotation2", vertrotations[2]);
            tree.SetFloat("vertrotation3", vertrotations[3]);
        }


        new public void OnTransformed(ITreeAttribute tree, int degreeRotation, EnumAxis? flipAxis)
        {
            var rot = new int[] { 0, 1, 3, 2 };
            var verrot = new int[] { 0, 1, 3, 2 };
            var rots = new float[4];
            var verrots = new float[4];
            var treeAttribute = tree.GetTreeAttribute("inventory");
            inventory.FromTreeAttributes(treeAttribute);
            var inv = new ItemSlot[4];
            var start = (degreeRotation / 90) % 4;

            for (var i = 0; i < 4; i++)
            {
                rots[i] = tree.GetFloat("rotation" + i);
                verrots[i] = tree.GetFloat("vertrotation" + i);
                inv[i] = inventory[i];
            }

            for (var i = 0; i < 4; i++)
            {
                var index = GameMath.Mod(i - start, 4);
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
