using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace EtchTheOwl
{

    class Level
    {
        public IList<Tree> trees;
        public IList<Bush> bushes;
        public IList<BasicModel> bugs;

        public int numTrees;
        public int maxX;
        public int treeSpacing;

        public Level()
        {
            trees = new List<Tree>();
            bugs = new List<BasicModel>();
            bushes = new List<Bush>();
            numTrees = 1000;
            maxX = 25000;
            treeSpacing = -500;

            Random rand = new Random();
            for (int i = 1; i <= numTrees; i++)
            {
                trees.Add(new Tree(Matrix.CreateTranslation(new Vector3(rand.Next(2 * maxX) - maxX, 0, i * treeSpacing)), true));
            }

            for (int i = 1; i <= numTrees / 5; i++)
            {
                bushes.Add(new Bush(Matrix.CreateTranslation(new Vector3(rand.Next(2 * maxX) - maxX, 0, i * 5 * treeSpacing))));
            }

            for (int i = 1; i <= numTrees / 10; i++)
            {
                bugs.Add(new Bug(Matrix.CreateTranslation(new Vector3(rand.Next(2 * maxX) - maxX, 0, i * 5 * treeSpacing))));
            }
        }
    }
}
