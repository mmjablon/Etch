using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace EtchTheOwl
{
    class Bush : BasicModel
    {
        new public static Model model;

        public Bush(Matrix world)
            : base(model, world)
        {
         
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
    }
}
