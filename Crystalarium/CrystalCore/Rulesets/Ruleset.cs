using CrystalCore.Model;
using CrystalCore.Model.Communication;
using CrystalCore.Model.Objects;
using CrystalCore.Util;
using CrystalCore.View.SignalRender;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CrystalCore.Rulesets
{
    public class Ruleset
    {
        
        /*
         * A Ruleset describes a usable system that Crystalarium can use.
         * It describes both how it looks and behaves.
         * 
         */

        private List<AgentType> _agentTypes; // the types of agents that make up this ruleset.


        public bool RotateLock { get; set; } // whether agents can be rotated or face a direction other than up.
        public bool DiagonalSignalsAllowed { get; set; }  // whether signals are only allowed to face the 4 orthagonal directions. If set to false, no AgentType can be of size greater than 1 x 1.
        public PortChannelMode PortChannelMode { get; set; } // the channel mode of every port created by this ruleset.

        public SignalType SignalType { get; set; }

        // mildly hacky.
        public int BeamMinLength { get; set; }
        public int BeamMaxLength { get; set; }


        public string Name // the human readable name of this ruleset.
        {
            get;
            private set;
        }

        public List<AgentType> AgentTypes 
        {
            get => _agentTypes;
            
        }

        public BeamViewTemplate BeamRenderConfig { get; set; }

        public Ruleset(string name)
        {
            Name = name;
            _agentTypes = new List<AgentType>();
            BeamRenderConfig = new BeamViewTemplate();

            RotateLock = false;
            DiagonalSignalsAllowed = false;
            PortChannelMode = PortChannelMode.fullDuplex;
            SignalType = SignalType.Beam;

            
            BeamMinLength = 1; // defaults, infinite, unbounded beams.
            BeamMaxLength = 0;

        }

        // create a new agentType.
        public AgentType CreateType(String name, Point size)
        {
            foreach(AgentType at in _agentTypes)
            {
                if(name == at.Name)
                {
                    throw new ArgumentException("Agent Type name already used in this ruleset");
                }
            }    

            _agentTypes.Add(new AgentType(this, name, size));
            
            return _agentTypes[_agentTypes.Count - 1];
        }

       public AgentType GetAgentType(string name)
       {
            foreach(AgentType at in _agentTypes)
            {
                if (name == at.Name)
                {
                    return at;
                }
            }
            return null;
       }

       internal Port CreatePort(CompassPoint facing, int ID, Agent parent)
       {
            switch (PortChannelMode)
            { 
                case PortChannelMode.fullDuplex:
                    return new FullPort(facing, ID, parent);
                case PortChannelMode.halfDuplex:
                    return new HalfPort(facing, ID, parent);
            
            }

            throw new InvalidOperationException("Invalid Port Channel: No case to create Port of type: " + PortChannelMode.ToString());

       }

        internal Signal CreateSignal(Grid g, Port transmitter, int value)
        {
            switch(SignalType)
            {
               
                case SignalType.Beam:
                    return new Beam(g, transmitter, value, BeamMinLength, BeamMaxLength);
                   
            }

            throw new InvalidOperationException("Invalid Signal Type: No case to create Signal of type: "+SignalType.ToString());
        }
  
    }
}
