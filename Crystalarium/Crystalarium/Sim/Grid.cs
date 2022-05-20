﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Crystalarium.Util;

namespace Crystalarium.Sim
{
    public class Grid
    {
        /* the grid class represents a grid.
        * In Crystalarium, a grid is a 2d plane where devices can be built using a number of systems,
        * with the primary system called Crysm.
        */


        SimulationManager sim;

        private List<List<Chunk>> _chunks; // a 2d array where the outer array represents rows and the inner array represents columns. [x][y]
        private Point chunksOrigin; // the chunk coords where the chunk array, chunks, starts.
        private Point chunksSize; // the size, in chunks, of the grid.

        public List<List<Chunk>> Chunks
        {
            get => _chunks;
        }

        public Rectangle Bounds
        { 
            get
            {
                return new Rectangle
                  ( chunksOrigin.X * Chunk.SIZE,
                    chunksOrigin.Y * Chunk.SIZE,
                    chunksSize.X   * Chunk.SIZE,
                    chunksSize.Y   * Chunk.SIZE );
            }
        
        }

        public Point gridSize
        {
            get => chunksSize;
        }



        public Grid(SimulationManager sim)
        {
            this.sim = sim;
            sim.addGrid(this);

            // perform first time setup.
            Reset();


        }

        // Probably temporary.
        public void DebugReport()
        {
            Console.WriteLine("Size: " + chunksSize + "\nOrigin:" + chunksOrigin+"\n"); 

            foreach (List<Chunk> list in _chunks)
            {
                String s = "{";
                foreach(Chunk ch in list)
                {
                    // this is silly! why do I have to write all of this?
                    s += ((ch!=null)?ch.ToString():"null")+", ";
                }

                s= s.Substring(0, s.Length - 2)+"},";
                Console.WriteLine(s);
            }
        }

        public void Destroy()
        {
            sim.removeGrid(this);
        }

        public void Reset()
        {
            // remove any existing chunks.
            if(_chunks!=null)
            {
                foreach(List<Chunk> list in _chunks)
                {
                    foreach(Chunk ch in list)
                    {
                        ch.Destroy();
                    }
                }

            }

            // initialize the chunk array.
            _chunks = new List<List<Chunk>>();
            _chunks.Add(new List<Chunk>());

            // create initial chunk.
            _chunks[0].Add(new Chunk(this, new Point(0, 0)));

            // set the chunk origin.
            chunksOrigin = new Point(0, 0);
            chunksSize = new Point(1, 1);
        }

        public void Add( GridObject o)
        {
            // what we do with the gridobject depends on what kind of object it is.

            // note that you are not meant to add chunks to the grid using the add method.
            // use expandChunkGrid instead.
            if(o is Chunk)
            {
           
                return;
            }

            // This girdObject is not of any known instance, so we throw an expection; we aren't prepared to 
            // handle it
            throw new ArgumentException("Unknown or Invalid type of GridObject to Add to this grid.");
        }

        public void Remove(GridObject o)
        {

            // Remove a grid object from it's appropriate containers

            if( o is Chunk) // chunks can't be removed once added.
            {
                o = null; // Doesn't change the size of the grid. This should be used sparingly.
                return; 
            }

            throw new ArgumentException("Unknown or Invalid type of GridObject to remove from this grid.");

        }

        public void ExpandGrid( Direction d)
        {
            if (d.IsHorizontal())
            {
                ExpandHorizontal(d); 
            }
            else
            {
                ExpandVertical(d);
            }
        }

        private void ExpandHorizontal(Direction d)
        {
            // we are adding a new list<Chunk> to _chunks.
            List<Chunk> newList = new List<Chunk>();
            chunksSize.X++;
            int x;


            if (d == Direction.left)
            {
                x = 0;
                _chunks.Insert(x, new List<Chunk>());
                chunksOrigin.X--; 
            }
            else
            {
                x = _chunks.Count;
                _chunks.Add(newList);
            }


            // generate the new chunks
            for(int y = 0; y<chunksSize.Y; y++)
            {
                Point gridLoc = new Point(x, y) + chunksOrigin;
                Chunk ch = new Chunk(this, gridLoc);
                _chunks[x].Add(ch);

            }

        }

        private void ExpandVertical(Direction d)
        {
            // we are adding a new Chunk to every list<Chunk> in _chunk.
            chunksSize.Y++;
           

            if (d == Direction.up)
                chunksOrigin.Y--;
          
            // create the new chunks.

            for (int x = 0; x < _chunks.Count; x++)
            {

                int y;

                if (d == Direction.up)
                {
                    y = 0;
                    _chunks[x].Insert(0, null);
                }
                else
                {
                    y = _chunks[x].Count;
                    _chunks[x].Add(null);
                    
                }

                // including chunk origins is important to get the correct coords for this chunk.
                Chunk ch = new Chunk(this, new Point(x + chunksOrigin.X, y+ chunksOrigin.Y));

                _chunks[x][y] = ch;

            }
        }


      


    }
}
