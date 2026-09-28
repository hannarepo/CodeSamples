using UnityEngine;

namespace CatFish.Enemies
{
	public class EnemyMovementState : EnemyStateBase
	{
		public override EnemyStateType EnemyStateType => EnemyStateType.Movement;
		protected WaypointPath _path = null;
		private Waypoint _target = null;
		private Vector3 _movementDirection = Vector3.zero;
		private float _distanceTravelled = 0f;
		private bool _isMovingToStart = false;
		protected float _speed = 2.0f;
		private float _stopTimer = 0f;
		protected bool _isStopped = false;

		public EnemyMovementState(EnemyContext context) : base(context)
		{
		}

		#region Unity Funtions
		public override void OnEnter()
		{
			if (_path == null)
			{
				SetWaypointPath(_context.WaypointPath);
			}
		}

		public override void OnFixedUpdate()
		{
			base.OnFixedUpdate();
			_stopTimer += Time.fixedDeltaTime;

			// If enemy ia moving to the first waypoint.
			if (_isMovingToStart)
			{
				Vector3 directionToTarget = _target.transform.position - _context.Transform.position;
				if (Vector3.Dot(_movementDirection, directionToTarget) <= 0)
				{
					// Target reached
					_target = _path.GetNextWaypoint(_target, _context.Direction);
					_movementDirection = _target.transform.position - _context.Transform.position;
					_isMovingToStart = false;
				}
				FaceTowardsNextWaypoint(_target.transform.position);
				Move(_movementDirection, _speed);
			}
			else if (_path != null && !_isStopped)
			{
				MoveAlongSpline();
			}
		}
		#endregion

		#region Path related
		/// <summary>
		/// Set the waypoint path that is used for movement.
		/// </summary>
		/// <param name="path">The path to be used.</param>
		public void SetWaypointPath(WaypointPath path)
		{
			_path = path;
			if (_path == null)
			{
				Debug.LogWarning("No waypoint path found!");
			}
			else
			{
				_target = _path.GetNextWaypoint(previous: _target, _context.Direction);
				_movementDirection = _target.transform.position - _context.Transform.position;
				_isMovingToStart = true;
				_distanceTravelled = 0;
			}
		}

		/// <summary>
		/// Change direction of the path.
		/// </summary>
		private void ToggleDirection()
		{
			_context.Direction = _context.Direction == WaypointPath.Direction.Forward
				? WaypointPath.Direction.Backward
				: WaypointPath.Direction.Forward;
		}
		#endregion

		#region  Movement
		public void Move(Vector3 movementDirection, float speed)
		{
			Vector3 movement = movementDirection.normalized * speed * Time.deltaTime;
			Vector3 position = _context.Rigidbody.position;
			position += movement;
			_context.Rigidbody.MovePosition(position);

			movementDirection = Vector3.zero;
		}

		/// <summary>
		/// Move along the spline of the path.
		/// </summary>
		private void MoveAlongSpline()
		{
			// Set the direction multiplier based on the current direction of the path.
			int directionMultiplier = _context.Direction == WaypointPath.Direction.Forward ? 1 : -1;
			// Set the distance travelled based on the speed and direction. Distance is the point on the spline.
			_distanceTravelled = _distanceTravelled + directionMultiplier * _speed * Time.deltaTime;
			float splineLength = _path.GetSplineLength();

			// If the path is a loop, use modulus to wrap the distance travelled around the spline length.
			// If the path is a ping pong, reverse the direction when the end is reached.
			if (_path.Type == WaypointPath.PathType.DelayLoop)
			{
				_distanceTravelled %= splineLength;
			}
			else
			{
				if (_distanceTravelled > splineLength)
				{
					_distanceTravelled = 2 * splineLength - _distanceTravelled;
					ToggleDirection();
				}
				if (_distanceTravelled < 0)
				{
					_distanceTravelled = -1 * _distanceTravelled;
					ToggleDirection();
				}
			}
			Vector3 position = _path.GetPointOnSpline(_distanceTravelled);
			FaceTowardsNextWaypoint(position);

			// If the timer is less than the stop interval, move towards the target position.
			// Otherwise, stop the enemy and reset the timer.
			if (_stopTimer < _context.StopInterval)
			{
				Move(position - _context.Transform.position, _speed);
			}
			else
			{
				_isStopped = true;
				_stopTimer = 0f;
			}
		}
	
		/// <summary>
		/// Rotate the enemy towards the next waypoint.
		/// </summary>
		/// <param name="target">The next waypoint.</param>
		private void FaceTowardsNextWaypoint(Vector3 target)
		{
			// Calculate the direction to the target and normalize it.
			Vector3 lookDirection = new Vector3(target.x, _context.Transform.position.y, target.z)
				- _context.Transform.position;
			lookDirection.Normalize();

			// Calculate the rotation needed to look at the target and apply it to the enemy's transform.
			// Use Quaternion.Slerp to limit the rotation speed based on the turn speed.
			Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
			Quaternion limitedRotation = Quaternion.Slerp(_context.Transform.rotation, targetRotation,
				_context.TurnSpeed * Time.deltaTime);
			_context.Transform.rotation = limitedRotation;
		}
		#endregion
	}
}
