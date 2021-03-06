using CrystalCore.Input;
using CrystalCore.Rulesets;
using CrystalCore.Model;
using CrystalCore.View;
using CrystalCore.View.ChunkRender;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalCore
{
    // A central access point for setting up, drawing, and updating.
    public class Engine
    {

        private SimulationManager _sim;
        private Controller _controller;
       
      
        private List<GridView> _viewports;
        private List<Grid> _grids;
      
        public SimulationManager Sim
        {
            get => _sim;
        }

        public Controller Controller
        {
            get => _controller;
        }




        public Engine(TimeSpan timeBetweenFrames)
        {
            _sim = new SimulationManager(timeBetweenFrames.TotalSeconds);
            _controller = new Controller();

            _viewports = new List<GridView>();
            _grids = new List<Grid>();
            
        }


        // manual ways to create gridviews
        public GridView addView(Grid g, int x, int y, int width, int height, ChunkViewTemplate renderConfig )
        {
            return new GridView(_viewports, g, x, y, width, height, renderConfig);
        }

        public GridView addView(Grid g, Point location, Point size, ChunkViewTemplate renderConfig)
        {
            return new GridView(_viewports, g, location, size, renderConfig);
        }

        public Grid addGrid( Ruleset r)
        {
            
            _grids.Add(new Grid(Sim, r));

            return _grids[_grids.Count - 1];
        }

        public void Update(GameTime gameTime)
        {
            _sim.Update(gameTime);


            // update viewports. (Camera Controls, mostly)
            foreach (GridView v in _viewports)
            {
                v.Update();
            }

            _controller.Update();

        }


        public void Draw(SpriteBatch sb)
        {
            // draw viewports
            foreach (GridView v in _viewports)
            {
                v.Draw(sb);
            }

        }
    }
}
