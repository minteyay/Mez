using UnityEngine;
using System.Collections.Generic;

namespace Choice
{
	[System.Serializable]
	public class WeightedChoice : object
	{
		public GameObject Value;
		public double Weight;
		[HideInInspector]
		public double Cumulative;

		public static void UpdateCumulatives(List<WeightedChoice> choices)
		{
			double sum = 0;
			foreach (WeightedChoice choice in choices)
			{
				sum += choice.Weight;
				choice.Cumulative = sum;
			}
		}

		public static GameObject GetRandom(System.Random random, List<WeightedChoice> choices)
		{
			double value = random.NextDouble() * choices[choices.Count - 1].Cumulative;
			return choices.Find(choice => value <= choice.Cumulative).Value;
		}
	}
}
