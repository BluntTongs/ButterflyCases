﻿using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace butterflycases
{
    public class BEButterflyBase : BlockEntityDisplay, IRotatable
    {
        public override string InventoryClassName => "butterflybase";
        protected InventoryGeneric inventory;
        public override InventoryBase Inventory => inventory;

        public bool haveCenterPlacement;

        public float[] rotations = new float[8];
        public float[] vertrotations = new float[8];
        public float[] rotValue
        {
            get { return rotations; }
            set { rotations = value; }
        }
             
        public float[] verrotValue
        {
            get { return vertrotations; }
            set { vertrotations = value; }
        }

        public BEButterflyBase()
        {
            inventory = new InventoryDisplayed(this, 4, "butterflybase-0", null, null);
        }

        internal bool OnInteract(IPlayer byPlayer, BlockSelection blockSel)
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
                if (colObj is ItemWrench) return false;
                if (colObj.Attributes != null && colObj.Attributes["displaycaseable"].AsBool(false) == true)
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


        public void SetBlockState(string state)
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



        public override void GetBlockInfo(IPlayer forPlayer, StringBuilder sb)
        {
            base.GetBlockInfo(forPlayer, sb);

            sb.AppendLine();

            if (forPlayer?.CurrentBlockSelection == null) return;

            int index = forPlayer.CurrentBlockSelection.SelectionBoxIndex;
            if (index >= inventory.Count) return; // Why can this happen o.O

            if (!inventory[index].Empty)
            {
                sb.AppendLine(inventory[index].Itemstack.GetName().Replace("Dead ",""));
            }
        }
        //Adds the item name to the tooltip box at the top of the screen if the display case has something in it.

        protected override float[][] genTransformationMatrices()
        {
            float[][] tfMatrices = new float[4][];

            for (int index = 0; index < 4; index++)
            {
                float x = (index % 2 == 0) ? 4.5f / 16f : 11.5f / 16f;
                float y = 1.01f / 16f;
                float z = (index > 1) ? 11.5f / 16f : 4.5f / 16;

                float degY = rotations[index] * GameMath.RAD2DEG;

                float rawdegX = vertrotations[index] * GameMath.RAD2DEG;
                float degX = GameMath.Clamp(rawdegX, 0, 30);
                //Leftover code from another project that I'm keeping around in case I want to reimplement "dynamic" rotation of items later.

                if (haveCenterPlacement)
                {
                    x = 8 / 16f;
                    y = 1 / 16f;
                    z = 8 / 16f;

                }


               
                    if (inventory[index].Itemstack != null && inventory[index].Itemstack.Collectible is ItemDeadButterfly)
                        tfMatrices[index] =
                        new Matrixf()
                        .Translate(x, y + 0.2f, z)
                        .RotateYDeg(degY)
                        .RotateXDeg(degX)
                        .RotateYDeg(42f)
                        .Scale(0.75f, 0.75f, 0.75f)
                        .Translate(-0.5f, 0, -0.5f)
                        .Values;
                    
                
            }

            return tfMatrices;
        }
        

        //public override void FromTreeAttributes(Vintagestory.API.Datastructures.ITreeAttribute tree, IWorldAccessor worldForResolving)
        //{
        //    base.FromTreeAttributes(tree, worldForResolving);

        //    haveCenterPlacement = tree.GetBool("haveCenterPlacement");
        //    rotations = new float[]
        //    {
        //        tree.GetFloat("rotation0"),
        //        tree.GetFloat("rotation1"),
        //        tree.GetFloat("rotation2"),
        //        tree.GetFloat("rotation3"),
        //    };
        //    vertrotations = new float[]
        //    {
        //        tree.GetFloat("vertrotation0"),
        //        tree.GetFloat("vertrotation1"),
        //        tree.GetFloat("vertrotation2"),
        //        tree.GetFloat("vertrotation3"),
        //    };
        //}

        //public override void ToTreeAttributes(Vintagestory.API.Datastructures.ITreeAttribute tree)
        //{
        //    base.ToTreeAttributes(tree);

        //    tree.SetBool("haveCenterPlacement", haveCenterPlacement);
        //    tree.SetFloat("rotation0", rotations[0]);
        //    tree.SetFloat("rotation1", rotations[1]);
        //    tree.SetFloat("rotation2", rotations[2]);
        //    tree.SetFloat("rotation3", rotations[3]);

        //    tree.SetFloat("vertrotation0", vertrotations[0]);
        //    tree.SetFloat("vertrotation1", vertrotations[1]);
        //    tree.SetFloat("vertrotation2", vertrotations[2]);
        //    tree.SetFloat("vertrotation3", vertrotations[3]);
        //}


        public void OnTransformed(ITreeAttribute tree, int degreeRotation, EnumAxis? flipAxis)
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
        public void OnTransformed(IWorldAccessor worldAccessor, ITreeAttribute tree, int degreeRotation,
           Dictionary<int, AssetLocation> oldBlockIdMapping, Dictionary<int, AssetLocation> oldItemIdMapping, EnumAxis? flipAxis)
        {
            
        }

    }
}
