using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace EtchTheOwl
{
    class Tree : BasicModel
    {
        bool hasEnemy;
        new public static Model model;

        public Tree(Matrix world, bool hasEnemy)
            : base(model, world)
        {
            this.hasEnemy = hasEnemy;
        }

        public override Model getModel()
        {
            return model;
        }


        /// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are taken from the camera object.
        /// </summary>        
        public override void DrawModel(ChaseCamera camera)
        {
            if (hasEnemy)
            {
                Enemy.DrawModel(camera, world);
            }
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * world;

                    effect.FogEnabled = true;
                    effect.FogColor = Color.Black.ToVector3();
                    effect.FogStart = 100.75f;
                    effect.FogEnd = 50000.25f;

                    // Use the matrices provided by the chase camera
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }

        public override bool CollidesWith(BasicModel otherModel)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions
            foreach (ModelMesh myModelMeshes in model.Meshes)
            {
                foreach (ModelMesh hisModelMeshes in otherModel.getModel().Meshes)
                {
                    if (myModelMeshes.BoundingSphere.Transform(
                        world).Intersects(
                        hisModelMeshes.BoundingSphere.Transform(otherModel.getWorld())))
                        return true;
                }
            }

            BoundingSphere branches = new BoundingSphere(new Vector3(world.Translation.X, 2000 ,world.Translation.Z), 350.0f);
            BoundingSphere branches2 = new BoundingSphere(new Vector3(world.Translation.X, 1500, world.Translation.Z), 200.0f);
            BoundingSphere trunk = new BoundingSphere(new Vector3(world.Translation.X, 500, world.Translation.Z), 50.0f);

            if (otherModel.CollidesWith(branches))
            {
                return true;
            }

            if (otherModel.CollidesWith(branches2))
            {
                return true;
            }

            if (otherModel.CollidesWith(trunk))
            {
                return true;
            }

            return false;
        }
    }
}
