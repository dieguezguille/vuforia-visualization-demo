using UnityEngine;

namespace Assets.Scripts.Models
{
	public class Marker : MonoBehaviour
	{
		public Marker PreviousMarker { get; set; }

		public Vector3 Position { get; set; }

		public float LastSegmentDistance
		{
			get
			{
				if (PreviousMarker != null && PreviousMarker.Position != null && Position != null)
				{
					return Vector3.Distance(Position, PreviousMarker.Position);
				}
				else
				{
					return 0;
				}
			}
		}

	}
}
