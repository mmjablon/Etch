﻿using System;
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
        public IList<Bug> bugs;

        public int maxX;
        //public int treeSpacing;
        public int levelEnd;
        public int xDensity;
        public float zDensity;
        public float bugDensity;


        public Level()
        {
            trees = new List<Tree>();
            bugs = new List<Bug>();
            bushes = new List<Bush>();
            maxX = 25000;
            //1000000 is about 2-3 minutes of gameplay
            levelEnd = 500000;
            //test length
            //levelEnd = 8000;

            //number of trees per x value
            xDensity = 6;

            //percent of trees from 0 to level end
            zDensity = 0.0007f;

            bugDensity = 0.0003f;

            Random rand = new Random();
            for (int i = 1; i <= (float)levelEnd * zDensity; i++)
            {
                for (int j = 0; j < xDensity; j++)
                {
                    bool enemy;
                    if (rand.Next(6) == 5)
                        enemy = true;
                    else
                        enemy = false;

                    trees.Add(new Tree(Matrix.CreateTranslation(
                        new Vector3(rand.Next(2 * maxX) - maxX, 0, -i * (1/(zDensity)))), enemy));
                }
            }

            for (int i = 1; i <= levelEnd * zDensity; i++)
            {
                bushes.Add(new Bush(Matrix.CreateTranslation(new Vector3(rand.Next(2 * maxX) - maxX, 0, -i * (1 / zDensity)))));
            }

            for (int i = 1; i <= levelEnd * bugDensity; i++)
            {
                bugs.Add(new Bug(Matrix.CreateTranslation(new Vector3(rand.Next(2 * maxX) - maxX, rand.Next(1850) + 150, -i * (1 / bugDensity)))));
            }
        }
    }
}
