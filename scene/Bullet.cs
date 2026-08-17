using Godot;
using Godot.Collections;
using System;
using System.Runtime.CompilerServices;

public partial class Bullet : Area2D
{
	[Export]
	float Speed = 320.0f;
	[Export]
	float MaxBulletLifeTime = 2.0f;

	Vector2 Direction = Vector2.Right;
	float RemainBulletLifeTime = 0.0f;

	public static readonly uint WORLD_COLLISION_MASK = 1;

	public override void _Ready()
	{
		RemainBulletLifeTime = MaxBulletLifeTime;
		AreaEntered += OnAreaEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector2 currentPosition = GlobalPosition;
		Vector2 nextPosition = currentPosition + Direction * Speed * (float)delta;

		if(WillHitTheWorld(currentPosition,nextPosition)) 
		{
			QueueFree();
			return;
		}
		
		GlobalPosition = nextPosition;
		RemainBulletLifeTime -= (float)delta;
		
		if(RemainBulletLifeTime <= 0.0f) 
		{
			QueueFree();	
		}
	}

	public void SetUp(Vector2 initDirection) 
	{
		if(initDirection != Vector2.Zero) 
		{
			Direction = initDirection.Normalized();
		}

		Rotation = Direction.Angle();
	}

	private void OnAreaEntered(Area2D hitArea) 
	{
		if(hitArea is Bullet) 
		{
			return;
		}

		QueueFree();
	}

	private bool WillHitTheWorld(Vector2 SourcePosition,Vector2 TargetPosition) 
	{
		 PhysicsDirectSpaceState2D spaceState = GetWorld2D().DirectSpaceState;
		
		if(spaceState == null) 
		{
			return false;
		}
		
		PhysicsRayQueryParameters2D query = PhysicsRayQueryParameters2D.Create
			(
				SourcePosition,
				TargetPosition,
				WORLD_COLLISION_MASK
			);
		query.CollideWithBodies = true;
		query.CollideWithAreas = false;

		Dictionary hitResult = spaceState.IntersectRay(query);

		return hitResult.Count > 0;
	}
}
