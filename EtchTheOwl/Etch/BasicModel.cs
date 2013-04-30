using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace EtchTheOwl
{
    class BasicModel
    {
        protected Model model;
        protected Matrix world;
        protected float ZRotation;
        protected Vector3 position;

        public BasicModel(Model model, Matrix world)
        {
            this.model = model;
            this.world = world;
        }

        public virtual Matrix getWorld()
        {
            return world;
        }

        public virtual Model getModel()
        {
            return model;
        }

        public void translate(Vector3 vec)
        {
            world.Translation += vec;
        }


        public void rotateAlongZ(float rad)
        {
            ZRotation += rad;
            world *= Matrix.CreateRotationZ(rad);
        }

        /// <summary>
        /// Simple model drawing method. The interesting part here is that
        /// the view and projection matrices are taken from the camera object.
        /// </summary>        
        public virtual void DrawModel(ChaseCamera camera)
        {
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
                    effect.FogStart = 10.75f;
                    effect.FogEnd = 100000.25f;

                    // Use the matrices provided by the chase camera
                    effect.View = camera.View;
                    effect.Projection = camera.Projection;
                }
                mesh.Draw();
            }
        }

        public virtual bool CollidesWith(BasicModel otherModel)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions
            foreach (ModelMesh myModelMeshes in model.Meshes)
            {
                foreach (ModelMesh hisModelMeshes in otherModel.getModel().Meshes)
                {
                    if (myModelMeshes.BoundingSphere.Transform(
                        world).Intersects(
                        hisModelMeshes.BoundingSphere.Transform(otherModel.world)))
                        return true;
                }
            }
            return false;
        }

        public bool CollidesWith(BoundingSphere sphere)
        {
            // Loop through each ModelMesh in both objects and compare
            // all bounding spheres for collisions
            foreach (ModelMesh myModelMeshes in model.Meshes)
            {
                if (myModelMeshes.BoundingSphere.Transform(
                    world).Intersects(
                    sphere))
                    return true;
            }
            return false;
        }
    }
}
