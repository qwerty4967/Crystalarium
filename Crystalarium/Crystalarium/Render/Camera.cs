﻿using System;
using System.Collections.Generic;
using Crystalarium.Sim;
using Crystalarium.Util;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Crystalarium.Render.ChunkRender;


namespace Crystalarium.Render
{
    public class Camera
    {
        /*
         * A viewport renders a grid.
         * 
         */


        private GridView parent;

        // 'Camera' controls
        private double _scale; // the number of pixels that currently represent one tile in gridspace
        private Vector2 _position; // the position of the top left corner of the viewport, in tiles, in grid space
        private Vector3 _velocity; // the velocity of the camera in x, y, and z dimensions. (in pixels/frame)
        private const float FRICTION = .3f;

        private int _minScale; // the minumum and maximum amount of pixels that can represent one tile.
        private int _maxScale;


        
       

        // Properties
        public Vector3 Velocity
        {
            get => _velocity;
            set => _velocity = value;
        
        }

        public float VelX
        {
            get => _velocity.X;
            set => _velocity.X = value;
        }

        public float VelY
        {
            get => _velocity.Y;
            set => _velocity.Y = value;
        }

        public float VelZ
        {
            get => _velocity.Z;
            set => _velocity.Z = value;
        }


        public int MinScale
        {
            get => _minScale;
            set {
                // set this value responsibly.
                if (value < 1)
                    value = 1;
                if (value > _maxScale)
                    value = _maxScale;

                _minScale = value;

                if (_scale < _minScale)
                    _scale = MinScale;
            }
        }

        public int MaxScale
        {
            get => _maxScale;
            set
            {
                // set this value responsibly
                if (value < _minScale)
                    value = MinScale;

                _maxScale = value;

                if (_scale > _maxScale)
                    _scale = _maxScale;
            }
        }

        public double Scale
        {
            get => _scale;
            set
            {
                // insure the scale is not set outside of bounds.
                if (value > _maxScale)
                {
                    value = _maxScale;
                    VelZ = 0;
                }
                if (value < _minScale)
                {
                    value = _minScale;
                    VelZ = 0;
                }

                    
                _scale = value;
            }
        }

        // when setting position with the position property, position is the location, in tile space, of the center of the viewport.
        public Vector2 Position
        {
            get 
            {
                Vector2 toReturn = new Vector2();
                toReturn.X = _position.X + (TileBounds().Size.X / 2.0f)-.5f;
                toReturn.Y = _position.Y + (TileBounds().Size.Y / 2.0f) -.5f;
                return toReturn;
            }// too lazy to implement this properly. If we need it, I'll add it later.
            set
            {
               
                float x = (float)(-1f * ( (TileBounds().Size.X) / 2f))+ value.X+.5f;
                float y = (float)(-1f * ( (TileBounds().Size.Y)/2f)) + value.Y+.5f;
                _position = new Vector2(x, y);
                _velocity = new Vector3(0);

            }
        }

        // Constructors

        public Camera(GridView parent)
        {
            this.parent = parent;

            // default scale values
            _minScale = 10;
            _maxScale = 50;

            // set the 'camera' to reasonable values
            _scale = (_minScale + _maxScale) / 2.0;
            Position = new Vector2(0, 0);
        }



        // Drawing code:
    

        // returns the bounds in tilespace of the viewport
        public RectangleF TileBounds()
        {
            return new RectangleF(_position.X, _position.Y, 
                (float)(parent.PixelBounds.Width / Scale), 
                (float)(parent.PixelBounds.Height / Scale));
        }

        // returns  pixel coords relative to start of viewport.
        // this also works outside of the viewport.
        private Point TiletoPixelCoords(Vector2 tilePos)
        {
           
            // tile to pixel:
            // first, tile to pixel relative to base coords
            // how do we do that?
            // -(camera tile pos - tile pos) tile pos relative to base

            int x = (int)(Scale * -1.0 * (_position.X - (float)tilePos.X));
            int y = (int)(Scale * -1.0 * (_position.Y - (float)tilePos.Y));
            return new Point(x, y);

        }

        public bool RenderTexture(SpriteBatch sb, Texture2D texture, Rectangle bounds)
        {
            return RenderTexture(sb, texture, new RectangleF(bounds), Color.White);
        }

        public bool RenderTexture(SpriteBatch sb, Texture2D texture, Rectangle bounds, Color c)
        {
            return RenderTexture(sb, texture, new RectangleF(bounds), c);
        }

        public bool RenderTexture(SpriteBatch sb, Texture2D texture, RectangleF bounds)
        {
            return RenderTexture(sb, texture, bounds, Color.White);
        }

        // bounds of object to render in tilespace
        public bool RenderTexture(SpriteBatch sb, Texture2D texture, RectangleF bounds, Color c)
        {
            // check if the texture needs to be rendered by this viewport
            if(!TileBounds().Intersects(bounds))
            {
                return false;
            }


            //it does! collect some basic information.
            // we add a couple pixels to the size of things
            Point pixelCoords = TiletoPixelCoords(bounds.Location)+new Point(1);
            Point pixelSize = new Point((int)(bounds.Size.X * _scale), (int)(bounds.Size.Y * _scale))+new Point(1,1);

          
            // partial rendering...
            // render it!
            Rectangle texturePixelBounds = new Rectangle(pixelCoords, pixelSize);

            // figure out the rectangle we need to draw.

            // some flags (this is getting messy)
            // whether this side had bits cut off from it.
            int topCut = 0;
            int bottomCut = 0;
            int rightCut = 0;
            int leftCut = 0;

            // get the top left point of the drawing area
            Point topLeft = texturePixelBounds.Location;
            Point size = texturePixelBounds.Size;


            if (topLeft.X < 0)
            {
                // adjust the size to match what is visible
                size.X += topLeft.X;

                // keep track of what was removed
                leftCut = -topLeft.X;

                // set position to what was removed.
                topLeft.X = 0;
            }

            if (topLeft.Y < 0)
            {
                // adjust the size to match what is visible
                size.Y += topLeft.Y;

                // keep track of what was removed
                topCut = -topLeft.Y;

                // set position to inside of the viewport
                topLeft.Y = 0;

            }

            topLeft = topLeft + parent.PixelBounds.Location;

            // figure out the size of the rectangle we need to draw.
            int rightSide = parent.PixelBounds.X + this.parent.PixelBounds.Width;
            size.X = GetRenderSize(rightSide, texturePixelBounds.Size.X, leftCut, topLeft.X, out rightCut);

            int bottomSide =parent.PixelBounds.Y + parent.PixelBounds.Height;
            size.Y = GetRenderSize(bottomSide, texturePixelBounds.Size.Y, topCut, topLeft.Y, out bottomCut);


            Rectangle sourceRect = GetTextureSourceBounds(topCut, bottomCut, leftCut, rightCut, texturePixelBounds, texture);


            sb.Draw(
                       texture,
                       new Rectangle(topLeft, size),
                       sourceRect,
                       c // the color of the texture


                   );
            return true;
        }

        private int GetRenderSize(int viewportFarPos, int size, int nearCut, int position, out int farCut)
        {
            int currentSize = size - nearCut;

            if (!(currentSize + position > viewportFarPos))
            {
                farCut = 0;
                return currentSize;
            }

            currentSize = viewportFarPos - position;
            farCut = size - currentSize;

            return currentSize;
        }

        private Rectangle GetTextureSourceBounds(int topCut, int bottomCut, int leftCut, int rightCut, Rectangle texturePixelBounds, Texture2D texture)
        {
            // now figure out the source rectangle. what part of the image do we need to draw?

            // get the ratio of the destinations's position, multiply it by the source.
            int sourceX = (int)((float)leftCut / (float)texturePixelBounds.Width * texture.Width);
            int sourceY = (int)((float)topCut / (float)texturePixelBounds.Height * texture.Height);


            // figure out the size of the source rectangle:

            // get the width, in pixels, of the destination.
            float textureWidth = (float)(texturePixelBounds.Width - ((leftCut>rightCut)? leftCut : rightCut)); 

            // get the width of the source rectangle, as a ratio of total width of the texuture
            float textureWidthRatio = textureWidth / (float)texturePixelBounds.Width;

            // get the width of the source rectangle in pixels
            int sourceWidth = (int)( textureWidthRatio * texture.Width );


            // get the height, in pixels, of the destination.
            float textureHeight = (float)(texturePixelBounds.Height - ((bottomCut<topCut)? topCut: bottomCut));

            // get the height of the source rectangle, as a ratio of total width of the texuture
            float textureHeightRatio = textureHeight / (float)texturePixelBounds.Height;

            // get the height of the source rectangle in pixels
            int sourceHeight = (int)(textureHeightRatio * texture.Height);


            return new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight);

        }

        public void Update()
        {
            // somewhat naive camera movement functionality.
            _position += new Vector2(Velocity.X/(float)Scale, Velocity.Y/(float)Scale);
            Scale += Velocity.Z;

            _velocity.X = Reduce(Velocity.X, FRICTION);
            _velocity.Y = Reduce(Velocity.Y, FRICTION);
            _velocity.Z = Reduce(Velocity.Z, FRICTION/4);

            
            
          


        }

        // pull 'a' closer to zero by 'b' amount. don't let 'a' overshoot zero.
        private float Reduce(float a, float b)
        {
            // a should remain at zero if it is already there.
            if(a==0)
            {
                return a;
            }

            b = MathF.Abs(b);
            if (a > 0)
            {
                a -= b;
                if (a < 0)
                {
                    a = 0;
                }

                return a;

            }

            a += b;
            if(a>0)
            {
                a = 0;
            }

            return a;
        }

    }


   
}