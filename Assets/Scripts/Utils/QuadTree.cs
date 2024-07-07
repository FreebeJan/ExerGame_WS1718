using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Utils
{
	public interface IBoundary
	{
		bool ContainsPoint(Vector2 point);
		bool Intersects(ref RectangleBound other);
	}

	public enum QuadDataType
	{
		street,
		vegetation,
		building,
		jump,
		wayvertex
	}

	public class QuadTreeData<T>
	{
		public Vector2 location;
		public QuadDataType type;
		public T contents;

		public QuadTreeData(Vector2 location, QuadDataType type, T contents)
		{
			this.location = location;
			this.type = type;
			this.contents = contents;
		}
	}

	public struct RectangleBound : IBoundary
	{
		public Vector2 Center;
		public readonly float HalfSize;

		public RectangleBound(Vector2 center, float halfSize)
		{
			Center = center;
			HalfSize = halfSize;
		}

		public bool ContainsPoint(Vector2 point)
		{
			return (point.x <= Center.x + HalfSize && point.x >= Center.x - HalfSize && point.y <= Center.y + HalfSize && point.y >= Center.y - HalfSize);
		}

		public bool Intersects(ref RectangleBound other)
		{
			return (Center.x - HalfSize < other.Center.x + other.HalfSize
					&& Center.x + HalfSize > other.Center.x - other.HalfSize
					&& Center.y - HalfSize < other.Center.y + other.HalfSize
					&& Center.y + HalfSize > other.Center.y - other.HalfSize);
		}

		public override string ToString()
		{
			return string.Format("Rectablge at {0} with halfSize {1}", Center, HalfSize);
		}
	}

	public struct CircleBound : IBoundary
	{
		public Vector2 Center;
		public readonly float HalfSize; // radius
		private readonly float dist;

		public CircleBound(Vector2 center, float halfSize)
		{
			Center = center;
			HalfSize = halfSize;
			dist = halfSize * halfSize;
		}

		public bool ContainsPoint(Vector2 point)
		{
			return (point - Center).magnitude < dist;
		}

		public bool Intersects(ref RectangleBound other)
		{
			// for boundary check we assume the circle is a rectangle.
			return (Center.x - HalfSize < other.Center.x + other.HalfSize
					&& Center.x + HalfSize > other.Center.x - other.HalfSize
					&& Center.y - HalfSize < other.Center.y + other.HalfSize
					&& Center.y + HalfSize > other.Center.y - other.HalfSize);
		}
	}

	public struct RayBound : IBoundary
	{
		public readonly Vector2 Origin;
		public readonly Vector2 Dir;
		public Vector2 Dest;
		public float MaxDist;
		public readonly float Error;

		public RayBound(Vector2 origin, Vector2 dir, float maxDist, float error = 1f)
		{
			Origin = origin;
			Dir = dir;
			Dir.Normalize();
			MaxDist = maxDist;
			Dest = Origin + MaxDist * Dir;
			Error = error;
		}

		public void ResetDist(float dist)
		{
			MaxDist = dist;
			Dest = Origin + MaxDist * Dir;
		}

		/**
         * Function to calculate whether Ray intersetcs a given line.
         * Based on this stackoverflow post: https://stackoverflow.com/a/1968345
         */
		private bool IntersectsLine(Vector2 start, Vector2 end)
		{

			float s1_x, s1_y, s2_x, s2_y;
			s1_x = Dest.x - Origin.x;
			s1_y = Dest.y - Origin.y;
			s2_x = end.x - start.x;
			s2_y = end.y - start.y;

			float s, t;
			float det = -s2_x * s1_y + s1_x * s2_y;
			if (Mathf.Abs(det) < 1e-20) return false;
			s = (-s1_y * (Origin.x - start.x) + s1_x * (Origin.y - start.y)) / det;
			t = (s2_x * (Origin.y - start.y) - s2_y * (Origin.x - start.x)) / det;

			return (s >= 0 && s <= 1 && t >= 0 && t <= 1);
		}

		public bool ContainsPoint(Vector2 point)
		{
			Vector3 conn = point - Origin;
			float dx, dy;
			dx = conn.x / Dir.x;
			dy = conn.y / Dir.y;
			// if dx or dy is negative point lies in opposite direction.
			if (dx < 0 || dy < 0) return false;
			// avoid cases where dy is zero
			if (dx <= 1e-20f && dy <= 1e-20f) return true;
			if (dx <= 1e-20f || dy <= 1e-20f) return false;
			// no devision by zero as we made sure in the step before.
			return (Mathf.Abs(dx / dy - 1f) <= Error && conn.magnitude <= MaxDist);
		}

		public bool Intersects(ref RectangleBound other)
		{
			if (other.ContainsPoint(Origin) || other.ContainsPoint(Dest))
			{
				return true;
			}
			else
			{
				Vector2 SW = new Vector2(other.Center.x - other.HalfSize, other.Center.y - other.HalfSize);
				Vector2 SE = new Vector2(other.Center.x + other.HalfSize, other.Center.y - other.HalfSize);
				if (IntersectsLine(SE, SW)) return true;
				Vector2 NW = new Vector2(other.Center.x - other.HalfSize, other.Center.y + other.HalfSize);
				if (IntersectsLine(SW, NW)) return true;
				Vector2 NE = new Vector2(other.Center.x + other.HalfSize, other.Center.y + other.HalfSize);
				if (IntersectsLine(NE, NW)) return true;
				if (IntersectsLine(NE, SE)) return true;
			}
			return false;
		}
	}

	public class QuadTree<T>
	{
		public const int NodeCapacity = 4;
		public RectangleBound Boundary;
		List<QuadTreeData<T>> Data;

		QuadTree<T> NW, NE, SW, SE;

		public QuadTree(RectangleBound boundary)
		{
			Boundary = boundary;
			Data = new List<QuadTreeData<T>>(NodeCapacity);
		}

		/// <summary>
		/// Removes all occurences of a given data object from the tree. 
		/// (There can be more then one occurence if object lies on the edge of more then one subtree (e.g mid point of 4 quads))
		/// </summary>
		/// <param name="dataPoint">The object to delete. Retrieve it for example with Get</param>
		/// <returns></returns>
		public bool Remove(QuadTreeData<T> dataPoint)
		{
			if (!Boundary.ContainsPoint(dataPoint.location)) return false;
			bool success = false;
			if (Data.Contains(dataPoint))
			{
				Data.Remove(dataPoint);
				success = true;
			}
			if (NW != null)
				success |= NW.Remove(dataPoint);
			if (NE != null)
				success |= NE.Remove(dataPoint);
			if (SW != null)
				success |= SW.Remove(dataPoint);
			if (SE != null)
				success |= SE.Remove(dataPoint);

			return success;
		}
		
		public bool Put(Vector2 location, QuadDataType type, T label)
		{
			return Put(new QuadTreeData<T>(location, type, label));
		}

		public QuadTree<T> PutAndGrow(ref bool success, Vector2 location, QuadDataType type, T label)
		{
			return PutAndGrow(ref success, new QuadTreeData<T>(location, type, label));
		}

		// Finds new smallest root node that includes pos 
		private QuadTree<T> GrowToDataPoint(Vector2 pos)
		{
			var root = this;
			while (!root.Boundary.ContainsPoint(pos))
			{
				// iteratively grow tree.
				var old = root;
				float dx = pos.x - root.Boundary.Center.x;
				dx = dx == 0 ? 1 : Mathf.Sign(dx); // dx \in \{-1 ,1}
				float dy = pos.y - root.Boundary.Center.y;
				dy = dy == 0 ? 1 : Mathf.Sign(dy); // dy \in \{-1, 1}

				root = new QuadTree<T>(
					new RectangleBound(
						old.Boundary.Center + new Vector2(dx, dy) * old.Boundary.HalfSize, // move tree center towards pos.
						2f * old.Boundary.HalfSize // new tree area is 4 times of the old one.
					)
				);
				root.Subdivide(); // create subnodes.
								  // override corresponding subnode with old tree.
				if (dx == 1)
				{
					if (dy == 1) root.NE = old;
					else root.SE = old;
				}
				else
				{
					if (dy == 1) root.NW = old;
					else root.SW = old;
				}
			}
			return root;
		}

		// Puts Data in tree. returns root node. Extends tree to include new datapoint.
		public QuadTree<T> PutAndGrow(ref bool success, QuadTreeData<T> data)
		{
			// if point is outside of boundary grow tree.
			if (!Boundary.ContainsPoint(data.location))
			{
				var root = GrowToDataPoint(data.location);
				success = root.Put(data);
				return root;
			}
			// do normal put otherwise.
			success = this.Put(data);
			return this;
		}

		public bool Put(QuadTreeData<T> data)
		{
			if (!Boundary.ContainsPoint(data.location)) return false;
			if (Data.Count < NodeCapacity)
			{
				Data.Add(data);
				return true;
			}
			if (NW == null) Subdivide();

			if (NW.Put(data)) return true;
			if (NE.Put(data)) return true;
			if (SW.Put(data)) return true;
			if (SE.Put(data)) return true;
			return false;
		}

		// Gets Data at the exact position. Returns first match only.
		public QuadTreeData<T> Get(Vector2 position, float delta = .1f)
		{
			if (!Boundary.ContainsPoint(position)) return null;

			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if ((position - dataPoint.location).magnitude <= delta)
				{
					return dataPoint;
				}
			}
			
			QuadTreeData<T> finding;
			if (NW != null && NW.Boundary.ContainsPoint(position))
			{
				finding = NW.Get(position, delta);
				if (finding != null) return finding;
			}
			if (NE != null && NE.Boundary.ContainsPoint(position))
			{
				finding = NE.Get(position, delta);
				if (finding != null) return finding;
			}
			if (SW != null && SW.Boundary.ContainsPoint(position))
			{
				finding = SW.Get(position, delta);
				if (finding != null) return finding;
			}
			if (SE != null && SE.Boundary.ContainsPoint(position))
			{
				finding = SE.Get(position, delta);
				if (finding != null) return finding;
			}
			return null;
		}

		public QuadTreeData<T> Raycast(Vector2 Origin, Vector2 Dir, float MaxDist, float delta = .1f)
		{
			return Raycast(new RayBound(Origin, Dir, MaxDist, delta));
		}

		public QuadTreeData<T> Raycast(RayBound ray)
		{
			if (!ray.Intersects(ref Boundary)) return null;
			QuadTreeData<T> bestMatch = null;
			float bestDist = ray.MaxDist;
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (ray.ContainsPoint(dataPoint.location))
				{
					float dist = (ray.Origin - dataPoint.location).magnitude;
					if (dist <= bestDist)
					{
						bestDist = dist;
						bestMatch = dataPoint;
					}
				}
			}
			ray.ResetDist(bestDist);
			QuadTreeData<T> finding = null;
			List<QuadTree<T>> Intersections = new List<QuadTree<T>>(4);
			if (NW != null && ray.Intersects(ref NW.Boundary)) Intersections.Add(NW);
			if (NE != null && ray.Intersects(ref NE.Boundary)) Intersections.Add(NE);
			if (SW != null && ray.Intersects(ref SW.Boundary)) Intersections.Add(SW);
			if (SE != null && ray.Intersects(ref SE.Boundary)) Intersections.Add(SE);
			Intersections.Sort((a, b) => (ray.Origin - a.Boundary.Center).magnitude.CompareTo((ray.Origin - b.Boundary.Center).magnitude));
			foreach (QuadTree<T> qt in Intersections)
			{
				finding = qt.Raycast(ray);
				if (finding != null)
				{
					return finding; // since we sorted the quad children we know that this finding must be better then later ones.
				};
			}
			return bestMatch;
		}


		public QuadTreeData<T> Raycast(Vector2 Origin, Vector2 Dir, float MaxDist, QuadDataType type, float delta = .1f)
		{
			return Raycast(new RayBound(Origin, Dir, MaxDist, delta), type);
		}

		public QuadTreeData<T> Raycast(RayBound ray, QuadDataType type)
		{
			if (!ray.Intersects(ref Boundary)) return null;
			QuadTreeData<T> bestMatch = null;
			float bestDist = ray.MaxDist;
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (dataPoint.type == type && ray.ContainsPoint(dataPoint.location))
				{
					float dist = (ray.Origin - dataPoint.location).magnitude;
					if (dist <= bestDist)
					{
						bestDist = dist;
						bestMatch = dataPoint;
					}
				}
			}
			ray.ResetDist(bestDist);
			QuadTreeData<T> finding = null;
			List<QuadTree<T>> Intersections = new List<QuadTree<T>>(4);
			if (NW != null && ray.Intersects(ref NW.Boundary)) Intersections.Add(NW);
			if (NE != null && ray.Intersects(ref NE.Boundary)) Intersections.Add(NE);
			if (SW != null && ray.Intersects(ref SW.Boundary)) Intersections.Add(SW);
			if (SE != null && ray.Intersects(ref SE.Boundary)) Intersections.Add(SE);

			// sort the Intersections based on their center distance to the ray origin.
			Intersections.Sort((a, b) => (ray.Origin - a.Boundary.Center).magnitude.CompareTo((ray.Origin - b.Boundary.Center).magnitude));

			foreach (QuadTree<T> qt in Intersections)
			{
				finding = qt.Raycast(ray, type);
				if (finding != null)
				{
					return finding; // since we sorted the quad-children we know that this finding must be better then later ones.
				};
			}
			return bestMatch;
		}

		public QuadTreeData<T> Get(Vector2Int position)
		{
			return Get(new Vector2(position.x, position.y));
		}

		private void Subdivide()
		{
			float quarterSize = Boundary.HalfSize * .5f;
			NW = new QuadTree<T>(
				new RectangleBound(
					Boundary.Center + new Vector2(quarterSize, -quarterSize),
					quarterSize
					)
			);
			NE = new QuadTree<T>(
				new RectangleBound(
					Boundary.Center + new Vector2(quarterSize, quarterSize),
					quarterSize
					)
			);
			SW = new QuadTree<T>(
				new RectangleBound(
					Boundary.Center + new Vector2(-quarterSize, quarterSize),
					quarterSize
					)
			);
			SE = new QuadTree<T>(
				new RectangleBound(
					Boundary.Center + new Vector2(-quarterSize, -quarterSize),
					quarterSize
					)
			);
		}

		public bool Collides(IBoundary scope)
		{
			if (!scope.Intersects(ref Boundary)) return false;
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (scope.ContainsPoint(dataPoint.location)) return true;
			}
			if (NW != null)
			{
				if (NW.Collides(scope)) return true;
				if (NE.Collides(scope)) return true;
				if (SW.Collides(scope)) return true;
				if (SE.Collides(scope)) return true;
			}
			return false;
		}

		public bool Collides(IBoundary scope, QuadDataType type)
		{
			if (!scope.Intersects(ref Boundary)) return false;
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (scope.ContainsPoint(dataPoint.location) && dataPoint.type == type) return true;
			}
			if (NW != null)
			{
				if (NW.Collides(scope, type)) return true;
				if (NE.Collides(scope, type)) return true;
				if (SW.Collides(scope, type)) return true;
				if (SE.Collides(scope, type)) return true;
			}
			return false;
		}

		public bool GetCollisions(IBoundary scope, List<QuadTreeData<T>> Out)
		{
			bool foundSomething = false;
			if (!scope.Intersects(ref Boundary)) return false;
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (scope.ContainsPoint(dataPoint.location))
				{
					Out.Add(dataPoint);
					foundSomething = true;
				}
			}
			if (NW != null)
			{
				foundSomething = foundSomething || NW.GetCollisions(scope, Out);
				foundSomething = foundSomething || NE.GetCollisions(scope, Out);
				foundSomething = foundSomething || SW.GetCollisions(scope, Out);
				foundSomething = foundSomething || SE.GetCollisions(scope, Out);
			}
			return foundSomething;
		}

		public bool GetCollisions(IBoundary scope, QuadDataType type, List<QuadTreeData<T>> Out)
		{
			bool foundSomething = false;
			if (!scope.Intersects(ref Boundary)) return false;
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (scope.ContainsPoint(dataPoint.location) && dataPoint.type == type)
				{
					Out.Add(dataPoint);
					foundSomething = true;
				}
			}
			if (NW != null)
			{
				foundSomething = foundSomething || NW.GetCollisions(scope, type, Out);
				foundSomething = foundSomething || NE.GetCollisions(scope, type, Out);
				foundSomething = foundSomething || SW.GetCollisions(scope, type, Out);
				foundSomething = foundSomething || SE.GetCollisions(scope, type, Out);
			}
			return foundSomething;
		}

		public IEnumerator<QuadTreeData<T>> GetEnumerator()
		{
			foreach (QuadTreeData<T> dataPoint in Data)
			{
				if (dataPoint == null) continue;
				if (dataPoint.contents == null) continue;
				yield return dataPoint;
			}
			if (NW != null)
			{
				foreach (QuadTreeData<T> data in NW)
				{
					yield return data;
				}
				foreach (QuadTreeData<T> data in NE)
				{
					yield return data;
				}
				foreach (QuadTreeData<T> data in SE)
				{
					yield return data;
				}
				foreach (QuadTreeData<T> data in SW)
				{
					yield return data;
				}
			}
		}
	}
}
