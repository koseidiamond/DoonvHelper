using Celeste.Mod.DoonvHelper.Utils;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.DoonvHelper.Entities;

[CustomEntity("DoonvHelper/CustomEnemy")]
public class CustomEnemy : CustomNPC
{

	[Pooled]
	public class Bullet : Actor
	{
		public Sprite Sprite;
		public Vector2 Velocity;
		public float SafeTime;
		public FacingAt Facing;

		private Player player;

		public Bullet() : base(Vector2.Zero) { }
		public Bullet Init(string spriteID, Vector2 position, Vector2 velocity, float safeTime, Player player, FacingAt facing)
		{
			if (Sprite is not null) Remove(Sprite);
			Add(Sprite = GFX.SpriteBank.Create(spriteID));

			this.Collider = new Hitbox(Sprite.Width / 2f, Sprite.Height / 2f, -Sprite.Width / 4f, -Sprite.Height / 4f);
			this.Position = position;
			this.Velocity = velocity;
			this.SafeTime = safeTime;
			this.Facing = facing;
			this.player = player;

			return this;
		}

		public override void Update()
		{
			base.Update();

			RotateSpriteToFacing(this.Sprite, Velocity, Facing);
			SafeTime -= Engine.DeltaTime;
			if (this.CollideCheck(player))
			{
				if (SafeTime < 0f)
				{
					player.Die((player.Center - this.Center).SafeNormalize());
				}
				destroy();
			}
			if ((Scene as Level).IsInCamera(Position, 32f) == false) destroy();

			MoveH(Velocity.X * Engine.DeltaTime, destroy);
			MoveV(Velocity.Y * Engine.DeltaTime, destroy);
		}

		private void destroy() => Scene.OnEndOfFrame += () => this.RemoveSelf();
		private void destroy(CollisionData _) => Scene.OnEndOfFrame += () => this.RemoveSelf();
	}

	public string BulletSpriteID;
	public float BulletRecharge;
	public float BulletSpeed;
	public float BulletSafeTime;
	public int Health;
	public bool Dashable;
	public float BulletShootTimer = -1f;
	public float InvincibilityFramesTimer = -1f;
	public Hitbox Bouncebox;
	public FacingAt BulletFacing;
	public string deathSound = "event:/-";
	private Player player;

	public CustomEnemy(EntityData data, Vector2 offset) : this(
		data.NodesWithPosition(offset),
		new Hitbox(
			width: data.Float("hitboxWidth", 16f),
			height: data.Float("hitboxHeight", 16f),
			x: data.Float("hitboxXOffset", 0f),
			y: data.Float("hitboxYOffset", 0f)
		),
		new Vector2(data.Float("XSpeed", 48f), data.Float("YSpeed", 240f)),
		data.Float("acceleration", 6f),
		data.Enum<AIType>("aiType", AIType.Wander),
		data.Attr("spriteID", "DoonvHelper_CustomEnemy_zombie"),
		data.Float("jumpHeight", 50f),
		data.Enum<FacingAt>("facing", FacingAt.MovementFlip),
		data.Bool("waitForMovement", true),
		data.Bool("outlineEnabled", true),
		data.Bool("enforceLevelBounds", true),
		data.Attr("bulletSpriteID", "badeline_projectile"),
		data.Float("bulletRecharge", 0f),
		data.Float("bulletSpeed", 300f),
		data.Float("bulletSafeTime", 0.25f),
		new Hitbox(
			width: data.Float("bounceboxWidth", 0f),
			height: data.Float("bounceboxHeight", 0f),
			x: data.Float("bounceboxXOffset", 0f),
			y: data.Float("bounceboxYOffset", 0f)
		),
		data.Int("health", 0),
		data.Bool("dashable", false),
		data.Enum<FacingAt>("bulletFacing", FacingAt.None)
	)
	{
	}

	public CustomEnemy(
		Vector2[] nodes,
		Hitbox hitbox,
		Vector2 speed,
		float acceleration,
		AIType ai,
		string spriteID,
		float jumpHeight,
		FacingAt facing,
		bool waitForMovement,
		bool outlineEnabled,
		bool enforceLevelBounds,
		string bulletSpriteID = "badeline_projectile",
		float bulletRecharge = 0.0f,
		float bulletSpeed = 300f,
		float bulletSafeTime = 0.25f,
		Hitbox bouncebox = null,
		int health = 1,
		bool dashable = false,
		FacingAt bulletFacing = FacingAt.None
	) : base(nodes, hitbox, speed, acceleration, ai, spriteID, jumpHeight, facing, waitForMovement, outlineEnabled, enforceLevelBounds)
	{
		this.BulletSpriteID = bulletSpriteID;
		this.BulletRecharge = bulletRecharge;
		this.BulletSpeed = bulletSpeed;
		this.BulletSafeTime = bulletSafeTime;
		this.Health = health;
		this.Dashable = dashable;
		this.BulletFacing = bulletFacing;

		if (bouncebox is not null && bouncebox.Width > 0 && bouncebox.Height > 0)
		{
			bouncebox.Position.X -= bouncebox.Width / 2f;
			bouncebox.Position.Y -= bouncebox.Height;
			this.Bouncebox = bouncebox;
			Add(new PlayerCollider(OnPlayerBounce, bouncebox));
		}

		Add(new PlayerCollider(OnPlayerCollide, this.Collider.Inflated(-4f)));
	}

	public override void Awake(Scene scene)
	{
		base.Awake(scene);
		player = scene.Tracker.GetEntity<Player>();
	}

	public override void Update()
	{
		base.Update();
		if (player is null || StateMachine.State == (int)St.Dummy || (WaitForMovement && player.JustRespawned)) return;
		if (BulletRecharge > 0f)
		{
			BulletShootTimer -= Engine.DeltaTime;
			if (BulletShootTimer < 0f)
			{
				Shoot(player.Position);
				BulletShootTimer = BulletRecharge;
			}
		}
		if (InvincibilityFramesTimer > 0f)
		{
			InvincibilityFramesTimer -= Engine.DeltaTime;
			if (InvincibilityFramesTimer % 0.1f > 0.05f)
				Sprite.Color = Color.Transparent;
			else Sprite.Color = Color.White;
		}
		else Sprite.Color = Color.White;
	}

	/// <summary>
	/// Makes the enemy shoot at the <paramref name="target"/>.
	/// </summary>
	/// <param name="target">The thing to shoot at.</param>
	public virtual void Shoot(Vector2 target)
	{
		// I don't know how the pooler works really I'm just copying from Spekio's toolbox
		// I hope this increases performance or something? idk
		Scene.Add(Engine.Pooler.Create<Bullet>().Init(
			BulletSpriteID,
			this.Center,
			(target - this.Center).SafeNormalize(BulletSpeed),
			BulletSafeTime,
			player,
			BulletFacing
		));
	}

	/// <summary>
	/// Makes the enemy takes damage, kills them if the damage is lethal.
	/// </summary>
	/// <param name="damage">The amount of health to deal.</param>
	/// <returns>A bool which is true if damage was dealt to the enemy.</returns>
	public virtual bool Damage(int damage = 1)
	{
		if (InvincibilityFramesTimer > 0f) return false;
		int newHealth = Health - damage;
		if (newHealth <= 0)
		{
  			Audio.Play(deathSound);
			this.Kill();
			return true;
		}
		Health = newHealth;
		InvincibilityFramesTimer = 0.5f;
		return true;
	}

	private void OnPlayerCollide(Player player)
	{
		if (Dashable && player.StateMachine.State == Player.StDash)
		{
			this.Damage();
			if (Input.Jump.Pressed)
			{
				player.Jump();
				player.RefillDash();
				player.RefillStamina();
				Input.Jump.ConsumePress();
			}
		}
		else if (InvincibilityFramesTimer <= 0f) player.Die((player.Center - this.Center).SafeNormalize());
	}

	private void OnPlayerBounce(Player player)
	{
		if (this.Damage())
		{
			player.Jump();
			player.RefillDash();
			player.RefillStamina();
		}
	}
}
