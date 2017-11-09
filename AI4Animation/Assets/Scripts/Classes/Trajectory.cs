﻿using UnityEngine;

public class Trajectory {

	public bool Inspect = false;

	public float Width = 0.5f;

	public Point[] Points;

	private const int PastPoints = 6;
	private const int FuturePoints = 5;
	private const int Density = 10;

	public Trajectory() {
		Initialise(Vector3.zero, Vector3.forward);
	}

	public Trajectory(Vector3 position, Vector3 direction) {
		Initialise(position, direction);
	}

	private void Initialise(Vector3 position, Vector3 direction) {
		Inspect = false;
		Width = 0.5f;
		Points = new Point[GetPointCount()];
		for(int i=0; i<Points.Length; i++) {
			Points[i] = new Point();
			Points[i].SetPosition(position);
			Points[i].SetDirection(direction);
			Points[i].Stand = 1f;
			Points[i].Walk = 0f;
			Points[i].Jog = 0f;
			Points[i].Crouch = 0f;
			Points[i].Jump = 0f;
			Points[i].Bump = 0f;
		}
	}

	public int GetPastPoints() {
		return PastPoints;
	}

	public int GetFuturePoints() {
		return FuturePoints;
	}

	public int GetDensity() {
		return Density;
	}

	public Point GetSample(int index) {
		return Points[index*Density];
	}

	public int GetSampleCount() {
		return PastPoints + FuturePoints + 1;
	}

	public int GetPointCount() {
		return Density*(PastPoints + FuturePoints) + 1;
	}

	public int GetRootSampleIndex() {
		return PastPoints;
	}

	public int GetRootPointIndex() {
		return Density*PastPoints;
	}

	public Point GetRoot() {
		return Points[GetRootPointIndex()];
	}

	public Point GetPrevious() {
		return Points[GetRootPointIndex()-1];
	}

	/*
	public void Postprocess() {
		LayerMask mask = LayerMask.GetMask("Ground");
		for(int i=0; i<GetPointCount(); i+=Density) {
			Vector3 position = Points[i].GetPosition();
			position.y = Utility.GetHeight(Points[i].GetPosition(), mask);
			Points[i].SetPosition(position);
		}
		for(int i=Density; i<GetPointCount(); i+=Density) {
			Vector3 A = Points[i-Density].GetPosition();
			Vector3 B = Points[i].GetPosition();
			float GK = B.y - A.y;
			A.y = 0f;
			B.y = 0f;
			float AK = Vector3.Distance(A,B);
			if(AK == 0f) {
				Points[i].Jump = 0f;
			} else {
				Points[i].Jump = Mathf.Abs(Mathf.Sin(GK / AK));
			}
		}
		for(int i=Density; i<=GetPointCount(); i+=Density) {
			for(int j=i-Density+1; j<i; j++) {
				Vector3 position = Points[j].GetPosition();
				float rateA = (float)((float)j-(float)i)/(float)Density + 1f;
				float rateB = (float)((float)i-(float)j)/(float)Density;
				position.y = rateB*Points[i-Density].GetPosition().y + rateA*Points[i].GetPosition().y;
				Points[j].SetPosition(position);
				Points[j].Jump = rateB*Points[i-Density].Jump + rateA*Points[i].Jump;
			}
		}
	}
	*/

	public class Point {
		private Vector3 Position;
		private Vector3 Direction;

		public float Stand, Walk, Jog, Crouch, Jump, Bump;

		public Point() {
			Position = Vector3.zero;
			Direction = Vector3.forward;
			Stand = 1f;
			Walk = 0f;
			Jog = 0f;
			Crouch = 0f;
			Jump = 0f;
			Bump = 0f;
		}

		public void SetPosition(Vector3 position) {
			Position = position;
			Position.y = Utility.GetHeight(Position, LayerMask.GetMask("Ground"));
			Jump = Utility.GetRise(Position, LayerMask.GetMask("Ground"));
		}

		public Vector3 GetPosition() {
			return Position;
		}

		public void SetDirection(Vector3 direction) {
			Direction = direction;
		}

		public Vector3 GetDirection() {
			return Direction;
		}

		public void SetHeight(float height) {
			Position.y = height;
		}

		public float GetHeight() {
			return Position.y;
		}

		public Quaternion GetRotation() {
			return Quaternion.LookRotation(Direction, Vector3.up);
		}

		public Vector3 SampleSide(float distance) {
			Vector3 ortho = Quaternion.Euler(0f, 90f, 0f) * Direction;
			Vector3 proj = Position + distance * ortho.normalized;
			proj.y = Utility.GetHeight(proj, LayerMask.GetMask("Ground"));
			return proj;
		}
	}

	public void Draw() {
		UnityGL.Start();
		int step = Density;

		//Connections
		for(int i=0; i<GetPointCount()-step; i+=step) {
			UnityGL.DrawLine(Points[i].GetPosition(), Points[i+step].GetPosition(), 0.01f, Utility.Black);
		}

		//Projections
		for(int i=0; i<GetPointCount(); i+=step) {
			Vector3 right = Points[i].SampleSide(Width/2f);
			Vector3 left = Points[i].SampleSide(-Width/2f);
			UnityGL.DrawCircle(right, 0.01f, Utility.Yellow);
			UnityGL.DrawCircle(left, 0.01f, Utility.Yellow);
		}

		//Directions
		Color transparentDirection = new Color(Utility.Orange.r, Utility.Orange.g, Utility.Orange.b, 0.75f);
		for(int i=0; i<GetPointCount(); i+=step) {
			UnityGL.DrawLine(Points[i].GetPosition(), Points[i].GetPosition() + 0.25f * Points[i].GetDirection(), 0.025f, 0f, transparentDirection);
		}

		//Rises
		Color transparentRise = new Color(Utility.Blue.r, Utility.Blue.g, Utility.Blue.b, 0.75f);
		for(int i=0; i<GetPointCount(); i+=step) {
			UnityGL.DrawLine(Points[i].GetPosition(), Points[i].GetPosition() + 1f * Points[i].Jump * Vector3.up, 0.025f, 0f, transparentRise);
		}

		//Positions
		for(int i=0; i<GetPointCount(); i+=step) {
			if(i % GetDensity() == 0) {
				if(i == GetRootPointIndex()) {
					UnityGL.DrawCircle(Points[i].GetPosition(), 0.025f, Utility.Magenta);
				} else {
					UnityGL.DrawCircle(Points[i].GetPosition(), 0.025f, Utility.Black);
				}
			} else {
				UnityGL.DrawCircle(Points[i].GetPosition(), 0.005f, Utility.Black);
			}
		}
		UnityGL.Finish();
	}

}
