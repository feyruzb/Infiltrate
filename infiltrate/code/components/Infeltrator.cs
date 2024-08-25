using Sandbox;
using Sandbox.Citizen;

public sealed class Infeltrator : Component
{
	[Property]
	[Category( "Components" )]

	public GameObject Camera { get; set; }
	
	[Property]
	[Category( "Components" )]

	public CharacterController Controller { get; set; }

	[Property]
	[Category( "Components" )]
	public CitizenAnimationHelper Animator { get; set; }


	/// <summary>
	/// game changing properties
	/// </summary>

	[Property]
	[Category("Stats")]
	[Range(0f, 400f ,1f)]
	public float WalkSpeed { get; set; } = 120f;

	[Property]
	[Category( "Stats" )]
	[Range( 0f, 800f, 1f )]
	public float RunSpeed { get; set; } = 250f;

	[Property]
	[Category( "Stats" )]
	[Range( 0f, 10000f, 10f )]
	public float JumpStrength { get; set; } = 400f;

	[Property]
	public Vector3 EyePosition { get; set; }

	public Angles EyeAngles { get; set; }

	Transform _initialCameraTransform;

	protected override void DrawGizmos()
	{
		Gizmo.Draw.LineSphere( EyePosition, 10f );
	}

	protected override void OnUpdate()
	{
		EyeAngles += Input.AnalogLook;
		EyeAngles = EyeAngles.WithPitch( MathX.Clamp( EyeAngles.pitch, -80f, 80f ) );
		Transform.Rotation = Rotation.FromYaw( EyeAngles.yaw );

		if (Camera != null )
		{
			Camera.Transform.Local = _initialCameraTransform.RotateAround(EyePosition, EyeAngles.WithYaw(0f));
		}

	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();
		if ( Controller == null ) { return; }

		var wishSpeed = Input.Down("Run") ? RunSpeed : WalkSpeed;
		var wishVelocity = Input.AnalogMove.Normal * wishSpeed * Transform.Rotation;

		Controller.Accelerate(wishVelocity);
		
		if (Controller.IsOnGround )
		{
			Controller.Acceleration = 10f;
			Controller.ApplyFriction( 5f );

			if ( Input.Pressed( "Jump" ) )
			{
				Controller.Acceleration = 5f;
				Controller.Punch( Vector3.Up * JumpStrength );

				if(Animator != null )
				{
					Animator.TriggerJump();
				}
			}
		}
		else
		{
			Controller.Velocity += Scene.PhysicsWorld.Gravity * Time.Delta;
		}

		Controller.Move();

		if (Animator != null )
		{
			Animator.IsGrounded = Controller.IsOnGround;
			Animator.WithVelocity( Controller.Velocity );
		}
	}

	protected override void OnStart()
	{
		if ( Camera != null )
		{
			_initialCameraTransform = Camera.Transform.Local;
		}
		if ( Components.TryGet<SkinnedModelRenderer>( out var model ) )
		{
			var clothing = ClothingContainer.CreateFromLocalUser();
			clothing.Apply(model);
		}
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
