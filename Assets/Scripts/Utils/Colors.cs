using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Utils
{
	static class Colors
	{
		/**
         * Create rainbow color.
         * params:
         *  maxindex: maximum color index to create
         *  index: index to create.
         * optional params: Saturation, Value, hdr
         */
		public static Color RainbowColor(int index, int maxindex, float Saturation = 1f, float Value = 1f, bool hdr = false)
		{
			return Color.HSVToRGB((float)index / (float)maxindex, Saturation, Value, hdr);
		}
	}
}
