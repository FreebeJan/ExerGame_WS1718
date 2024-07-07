using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.World.Labeling
{
	public static class BiomeLabeler
	{
		public enum Biomes
		{
			urban, // low height + low variance in norm.y
			suburban,
			dryland, // low moisture
			pasture, // heigh moisture + low variance in norm.y
			forest, // heigh moisture 
			deepforest,
			snowforest, // heigh moisutre + heigh height
			rocky, // variance in norm.y > thresh
		}
	}
}
