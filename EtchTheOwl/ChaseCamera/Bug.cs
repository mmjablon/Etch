using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EtchTheOwl
{
    class Bug : BasicModel
    {
        new public static Model model;
        float maxX = 2000.0f;
        float minX = 150.0f;
        float speed = 1000.0f;
        bool up = true;

        public Bug(Matrix world)
            : base(model, world)
        {

        }

        public override Model getModel()
        {
            return model;
        }

        public void Update(GameTime gameTime)
        {
            Vector3 pos = world.Translation;
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (up)
            {
                if (pos.Y >= maxX)
                {
                    up = false;
                }
                else
                {
                    Matrix translation = Matrix.CreateTranslation(new Vector3(0,speed * elapsed,0));
                    world *= translation;
                }
            }
            else
            {
                if (pos.Y <= minX)
                {
                    up = true;
                }
                else
                {
                    Matrix translation = Matrix.CreateTranslation(new Vector3(0, -speed * elapsed, 0));
                    world *= translation;
                }
            }
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
