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
    public class BEButterflyCaseDome : BEButterflyBase, IRotatable
    {
        public override string InventoryClassName => "butterflycasedome";
        //protected InventoryGeneric inventory;
        //public override InventoryBase Inventory => inventory;

        //bool haveCenterPlacement;
        //float[] rotations = new float[4];
        //float[] vertrotations = new float[4];


        public BEButterflyCaseDome()
        {
            inventory = new InventoryDisplayed(this, 1, "butterflycasedome-0", null, null);
        }
        private bool TryPut(ItemSlot slot, BlockSelection blockSel, IPlayer player)
        {
            int index = blockSel.SelectionBoxIndex;
            bool nowCenterPlacement = inventory.Empty && Math.Abs(blockSel.HitPosition.X - 0.5f) < 0.1 && Math.Abs(blockSel.HitPosition.Z - 0.5f) < 0.1;

            var attr = slot.Itemstack.ItemAttributes;
            float height = attr?["butterflycase"]["minHeight"]?.AsFloat(0.25f) ?? 0;
            if (height > (this.Block as BlockButterflyCase)?.height)
            {
                (Api as ICoreClientAPI)?.TriggerIngameError(this, "tootall", Lang.Get("This item is too tall to fit in this display case."));
                return false;
            }
            //This stuff is mostly just inherited from the original display case code.


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
        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[1][];



            for (int index = 0; index < 1; index++)
            {

                float x = 8f / 16f;
                float y = 8f / 16f;
                float z = 8f / 16f;


                float degY = rotations[index] * GameMath.RAD2DEG;
                float rawdegX = vertrotations[index] * GameMath.RAD2DEG;

                float degX = GameMath.Clamp(rawdegX, 45, 45);

                    if (inventory[index].Itemstack != null && inventory[index].Itemstack.Collectible is ItemDeadButterfly)
                        tfMatrices[index] =
                        new Matrixf()
                        .Translate(x, y, z)
                        .RotateYDeg(degY)
                        .RotateXDeg(degX)
                        .RotateYDeg(45f)
                        .Scale(0.85f, 0.85f, 0.85f)
                        .Translate(-0.5f, 0, -0.5f)
                        .Values;
                    else
                        tfMatrices[index] =
                        new Matrixf()
                        .Translate(x, y, z)
                        .RotateYDeg(degY)
                        .RotateXDeg(degX)
                        .RotateYDeg(45f)
                        .Scale(0.80f, 0.75f, 0.75f)
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
                tree.GetFloat("rotation0")
            };
            vertrotations = new float[]
            {
                tree.GetFloat("vertrotation0")
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
