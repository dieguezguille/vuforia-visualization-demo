using UnityEngine;

namespace Assets.Scripts.Models
{
	public class Marker
	{
		public Marker Previous { get; set; }

		public Vector3 Position { get; set; }

		public float LastSegmentDistance
		{
			get
			{
				if (Previous != null && Previous.Position != null && Position != null)
				{
					return Vector3.Distance(Position, Previous.Position);
				}
				else
				{
					return 0;
				}
			}
		}

	}
}
