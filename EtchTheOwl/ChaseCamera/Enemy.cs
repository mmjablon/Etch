using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EtchTheOwl
{
     class Enemy : BasicModel
    {
        new public static Model model;

        public Enemy(Matrix world)
            : base(model, world)
        {

        }

        public override Model getModel()
        {
            return model;
        }

        public static void DrawModel(ChaseCamera camera, Matrix world)
        {
            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            world *= Matrix.CreateTranslation(new Vector3(600, 1550, 0));

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = new Vector3(0.4f, 0.4f, 0.4f);
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
