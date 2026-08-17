using Godot;
using System;
using System.ComponentModel;

public partial class Player : CharacterBody2D
{
    [Export]
    public AnimatedSprite2D PlayerAnimate;
    [Export]
    public AnimatedSprite2D ArmedAnimate;
    [Export]
    public Timer ShootingTimer;

    [Export]
    public float CharacterMoveSpeed = 120;
    [Export]
    public float FireInterval = 0.18f;
    [Export]
    public float BullletSpawnDistance = 18.0f;

    public static readonly StringName NORMAL_ANIMATION_PREFIX = "normal";
    public static readonly PackedScene BULLET_SCENE = GD.Load<PackedScene>
        (
        "res://scene/bullet.tscn"
        );
    public static readonly StringName ARMED_ANIMATION_PREFIX = "armed";
    public static readonly double SPIRAL_PHASE_STEP = Math.PI / 12;
    public static float DEFAULT_FIRE_RATE_MULTIPLIER = 1.0f;

    public static readonly int PLAYER_FROM_MODE_NORMAL = 0;
    public static readonly int PLAYER_FROM_MODE_ARMED = 1;
    public static readonly int SHOT_PATTERN_NORMAL = 0;
    public static readonly int SHOT_PATTERN_SPIRAL = 1;

    StringName facingSuffix = "right";
    float rapidFireRateMultiplier = DEFAULT_FIRE_RATE_MULTIPLIER;
    float formFireRateMultiplier = DEFAULT_FIRE_RATE_MULTIPLIER;
    int currentFormMode = PLAYER_FROM_MODE_NORMAL;
    int currentShotPattern = SHOT_PATTERN_NORMAL;
    float spiralPhase = 0.0f;//初始旋转弹幕相位

    public override void _Ready()
    {
        ShootingTimer.OneShot = true;
        ShootingTimer.WaitTime = GetEffectiveFireInterval();
        UpdateAnimation();
        UpdateArmedEffect();
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 moveDirection = Input.GetVector
            (
            "move_left",
            "move_right",
            "move_up",
            "move_down"
            ).Normalized();
        Vector2 shootDirection = Input.GetVector
            (
            "shoot_left",
            "shoot_right",
            "shoot_up",
            "shoot_down"
            ).Normalized();

        Velocity = new Vector2
            (
            moveDirection.X * CharacterMoveSpeed,
            moveDirection.Y * CharacterMoveSpeed
            );

        MoveAndSlide();

        //if (moveDirection != Vector2.Zero) 
        //{
        //	facingSuffix = VectorToFacingSuffix(moveDirection);
        //}
        //UpdateAnimation();
        if (currentShotPattern == SHOT_PATTERN_SPIRAL)
        {
            TryAutoSpiralShoot();
        }
        else if (shootDirection != Vector2.Zero)
        {
            TryShoot(shootDirection);
        }

        UpdateFacing(moveDirection, shootDirection);
        UpdateAnimation();
        UpdateArmedEffect();
    }

    private void UpdateAnimation()
    {
        StringName animateName = GetAnimationPrefix() + '_' + facingSuffix;

        if (!PlayerAnimate.SpriteFrames.HasAnimation(animateName))
        {
            StringName fallBackAnimationName = NORMAL_ANIMATION_PREFIX + '_' + facingSuffix;
            if (!PlayerAnimate.SpriteFrames.HasAnimation(fallBackAnimationName))
            {
                GD.PushWarning("Animation Loss:" + animateName);
                return;
            }
            animateName = fallBackAnimationName;
        }

        if (PlayerAnimate.Animation != animateName)
        {
            PlayerAnimate.Play(animateName);
        }

        //StringName animateName = NORMAL_ANIMATION_PREFIX + '_' + facingSuffix;

        //if (!PlayerAnimate.SpriteFrames.HasAnimation(animateName))
        //{
        //    GD.PushWarning("Animation Loss:" + animateName);
        //}

        //if (PlayerAnimate.Animation != animateName)
        //{
        //    PlayerAnimate.Play(animateName);
        //}
    }

    private void UpdateArmedEffect()
    {

    }
    private StringName VectorToFacingSuffix(Vector2 faceDirection)
    {
        StringName animateDirection = "";

        if (Mathf.Abs(faceDirection.X) >= Mathf.Abs(faceDirection.Y))
        {
            animateDirection = faceDirection.X > 0.0 ? "right" : "left";
        }
        else
        {
            animateDirection = faceDirection.Y > 0.0 ? "down" : "up";
        }

        return animateDirection;
    }
    private double GetEffectiveFireInterval()//
    {
        return 0.0;
    }
    private void TryShoot(Vector2 shootDirection)
    {
        if (!ShootingTimer.IsStopped()) 
        {
            return;
        }

        Vector2 fireDirection = shootDirection.Normalized();
        bool hasSpawnedBullet = FireBullets(fireDirection);
        if (hasSpawnedBullet) 
        {
            ShootingTimer.Start(GetEffectiveFireInterval());
        }
    }

    private bool FireBullets(Vector2 baseDirection) 
    {
        if (currentShotPattern == SHOT_PATTERN_SPIRAL) 
        {
            bool hasSpawnedForwardBullet = SpawnedBullet(baseDirection);
            bool hasSpawnedBackwardBullet = SpawnedBullet(baseDirection.Rotated(Mathf.Pi));
            spiralPhase = (float)Mathf.Wrap(spiralPhase + SPIRAL_PHASE_STEP, 0.0f, Mathf.Tau);
            return hasSpawnedForwardBullet || hasSpawnedBackwardBullet;
        }
        return SpawnBullet(baseDirection);
    }
    private void TryAutoSpiralShoot()//
    {

    }
    private void UpdateFacing(Vector2 moveDirection, Vector2 shootDirection)
    {
        if (currentShotPattern == SHOT_PATTERN_SPIRAL)
        {
            if (moveDirection != Vector2.Zero)
            {
                facingSuffix = VectorToFacingSuffix(moveDirection);
            }
            return;
        }

        if (shootDirection != Vector2.Zero)
        {
            facingSuffix = VectorToFacingSuffix(shootDirection);
        }
        else if (moveDirection != Vector2.Zero)
        {
            facingSuffix = VectorToFacingSuffix(moveDirection);
        }
    }
    private StringName GetAnimationPrefix()
    {
        return "";
    }
}
