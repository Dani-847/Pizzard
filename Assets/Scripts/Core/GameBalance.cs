using System.Collections.Generic;

namespace Pizzard.Core
{
    /// <summary>
    /// Single source of truth for every balance-relevant constant in the game.
    /// Tweak values here instead of hunting through 30+ scripts.
    /// </summary>
    public static class GameBalance
    {
        // ──────────────────────────────────────────────
        //  PLAYER
        // ──────────────────────────────────────────────
        public static class Player
        {
            public const float MoveSpeed = 5f;
            public const float DashSpeedMultiplier = 3f;
            public const float DashDuration = 0.2f;
            public const float DashCooldown = 1f;

            public const int MaxHP = 6;                    // 6 half-hearts = 3 full hearts
            public const float InvulnerabilityDuration = 1.0f;
        }

        // ──────────────────────────────────────────────
        //  MANA
        // ──────────────────────────────────────────────
        public static class Mana
        {
            public const float MaxMana = 100f;
            public const float BaseRecoveryRate = 50f;      // per second
            public const float RecoveryDelayAfterCast = 0.3f;
            public const float UpgradeMultiplier = 1.5f;
            public const float DefaultSpellCost = 30f;     // fallback for unknown combos

            public static readonly Dictionary<string, float> SpellCosts = new Dictionary<string, float>
            {
                // Tier 1 — Single element
                { "queso", 10f },
                { "pepperoni", 12f },
                { "piña", 11f },

                // Tier 2 — Two elements
                { "queso|pepperoni", 20f },
                { "queso|piña", 18f },
                { "pepperoni|queso", 22f },
                { "pepperoni|piña", 20f },
                { "piña|queso", 19f },
                { "piña|pepperoni", 21f },

                // Tier 3 — Three elements
                { "queso|pepperoni|piña", 30f },
                { "queso|piña|pepperoni", 28f },
                { "pepperoni|queso|piña", 32f },
                { "pepperoni|piña|queso", 30f },
                { "piña|queso|pepperoni", 29f },
                { "piña|pepperoni|queso", 31f },
                { "pepperoni|pepperoni|pepperoni", 35f },
                { "queso|queso|piña", 25f },
                { "queso|queso|queso", 28f },
                { "piña|piña|piña", 30f },

                // T2 missing combos
                { "queso|queso", 22f },

                // T3 piña variants
                { "piña|piña|queso", 26f },
                { "piña|piña|pepperoni", 26f },
                { "piña|queso|piña", 28f },
                { "piña|queso|queso", 28f },
                { "piña|pepperoni|piña", 30f },
                { "piña|pepperoni|pepperoni", 32f },

                // T3 queso variants
                { "queso|piña|piña", 28f },
                { "queso|piña|queso", 30f },
                { "queso|pepperoni|pepperoni", 28f },
                { "queso|pepperoni|queso", 26f },
            };

            public static float GetSpellCost(string comboKey)
            {
                return SpellCosts.TryGetValue(comboKey, out float cost) ? cost : DefaultSpellCost;
            }
        }

        // ──────────────────────────────────────────────
        //  POTIONS
        // ──────────────────────────────────────────────
        public static class Potions
        {
            public const int StartingMax = 3;
            public const int HealPerPotion = 2;            // half-hearts
        }

        // ──────────────────────────────────────────────
        //  CURRENCY
        // ──────────────────────────────────────────────
        public static class Currency
        {
            public const int BossKillReward = 150;
        }

        // ──────────────────────────────────────────────
        //  BOSSES
        // ──────────────────────────────────────────────
        public static class Bosses
        {
            public static class Pblob
            {
                public const float MaxHP = 1000f;
                public const float Phase1AlternateTime = 2f;
                public const float Phase1MoveSpeed = 4f;   // Slower than player, but pursues
                public const float Phase2Timer = 30f;
                public const float Phase2CircleMoveTime = 5f;
                public const int Phase2TimeoutDamage = 2; // Damage taken if player fails the phase 2 timer
                
                public const float GridDamagePerTick = 1f;
                
                // Attack limits
                public const float PatternDuration = 10.5f;
                public const float WanderRadius = 4f;
                public const float CircleMoveSpeed = 2.5f;  // Speed at which phase 2 circles wander
                public const float CircleScale = 1.5f;       // Scale multiplier applied at spawn
                public const float CircleSpawnRadius = 3.5f; // Horizontal spread between floor circles
                public const float Phase2FloorY = -2.5f;     // Y offset below arenaCenter for floor platforms
                public const float KnockbackDistance = 5f;
                public const float KnockbackDuration = 0.4f;

                // Health bar
                public const float HealthBarDrainDelay = 0.5f;   // Seconds before orange bar starts draining
                public const float HealthBarDrainSpeed = 0.35f;  // fillAmount units per second

                // Phase 3
                public const float Phase3MoveSpeed = 6f;    // Boss chase speed in enrage
                public const float GridSpawnOffsetY = 0.2f;  // You can adjust this slightly to perfectly map to your visual 'gridgoeshere' setup
            }

            public static class Heckiel
            {
                public const float AttackInterval = 3f;
                public const float Phase2ThresholdPercent = 0.5f; // 50% HP
            }

            public static class Pomodoro
            {
                public const float AttackInterval = 4f;
            }

            public static class Niggel
            {
                // ── CoinVault (HP system) ──
                public const int CoinVaultMax = 200;
                public const int Enrage1Threshold = 150;
                public const int Enrage2Threshold = 100;
                public const int Enrage3Threshold = 50;

                // ── Movement (progressive: interpolates from slow→fast as HP drops) ──
                public const float BaseMoveSpeed = 1.8f;   // speed at full HP
                public const float MaxMoveSpeed  = 7.5f;   // speed at 0 HP

                // ── Dash ──
                public const float BaseDashCooldown = 15f;
                public const float Enrage2DashCooldown = 10f;
                public const float Enrage3DashCooldown = 6f;
                public const float DashDistance = 5f;
                public const float DashDuration = 0.25f;

                // ── Coin bag (Attack 1) ──
                public const float CoinBagSpeed = 3f;
                public const float Enrage1CoinBagSpeed = 5f;
                public const float CoinBagDamage = 2f;

                // ── Healing coin (Enrage 2+) ──
                public const float HealingCoinSpeed = 9f;
                public const int HealingCoinAmount = 67;
                public const float HealingCoinCooldown = 6f;

                // ── Coin shield burst (Enrage 3) ──
                public const float ShieldCooldown = 6f;
                public const float ShieldChargeDuration = 1.5f;
                public const int ShieldBurstCount = 10;
                public const float ShieldBurstDamage = 2f;

                // ── Black dot barriers (Enrage 1) ──
                public const int BarrierRowCount = 3;
                public const int BarrierDotsPerRow = 5;
                public const float BarrierDotSpacing = 0.8f;

                // ── Momentum (player side) ──
                public const int MaxMomentum = 5;
                public const float MomentumResetDelay = 4f;
                public const float MomentumDamagePerStack = 0.1f;  // Increased to make buff obvious (max 50% extra damage)
                public const float MomentumSpeedPerStack = 0.08f;  // Increased to make buff obvious (max 40% extra speed)

                // ── Attack timing ──
                public const float BaseAttackInterval = 2.5f;
                public const float Enrage1AttackInterval = 2.5f;
                public const float Enrage2AttackInterval = 1.7f;

                // ── Legacy constants (kept for backward compatibility) ──
                public const float AttackInterval = 3f;
                public const float StealRange = 2.5f;
                public const int CurrencyStealAmount = 10;
                public const float SpeedBuffPerSteal = 0.2f;
            }
        }

        // ──────────────────────────────────────────────
        //  SPELLS — BASE PROJECTILES
        // ──────────────────────────────────────────────
        public static class Spells
        {
            public static class Base
            {
                public const float Speed = 10f;
                public const float Lifetime = 3f;
                public const float Damage = 10f;
            }

            public static class Enemy
            {
                public const float Speed = 10f;
                public const float Lifetime = 3f;
                public const float Damage = 1f;
            }

            // -- Pepperoni (single) --
            public static class Pepperoni
            {
                public const float EffectDuration = 7f;
                public const int InitialStacks = 2;
            }

            // -- PepperoniPepperoni (fire trail) --
            public static class PepperoniPepperoni
            {
                public const int InitialSpiceCharges = 4;
                public const int TrailSpiceCharges = 2;
                public const float TrailDamageInterval = 0.2f;
                public const float TrailDuration = 3f;
                public const float TrailRadius = 2f;
                public const float SpiceEffectDuration = 5f;
                public const float TrailSpawnInterval = 0.1f;
                public const float MinDistanceBetweenTrails = 0.5f;
            }

            // -- PepperoniPepperoniPepperoni (dire trail spawner) --
            public static class PepperoniPepperoniPepperoni
            {
                public const float InitialMoveTime = 0.1f;
                public const float StickDuration = 2f;
                public const float SpawnInterval = 0.2f;
                public const float SpawnAreaRadius = 2f;
                // child fire trail params
                public const int ChildCharges = 1;
                public const float ChildInterval = 0.3f;
                public const float ChildDuration = 1f;
                public const float ChildRadius = 0.5f;
                public const float ChildEffectDuration = 2f;
            }

            // -- PepperoniPepperoniPineapple (rotating spawner) --
            public static class PepperoniPepperoniPineapple
            {
                public const float InitialMoveTime = 0.1f;
                public const float StickDuration = 2f;
                public const float SpawnInterval = 0.2f;
                public const float RotationSpeed = 360f;
                public const float SpawnedProjectileSpeed = 8f;
            }

            // -- PepperoniPepperoniQueso --
            public static class PepperoniPepperoniQueso
            {
                public const float InitialMoveTime = 0.1f;
                public const float StickDuration = 2f;
                public const float SpawnInterval = 0.2f;
                public const float SpawnAreaRadius = 3f;
                // child fire trail params
                public const int ChildCharges = 1;
                public const float ChildInterval = 0.3f;
                public const float ChildDuration = 2f;
                public const float ChildRadius = 2f;
                public const float ChildEffectDuration = 4f;
            }

            // -- PepperoniPiña (bomb) --
            public static class PepperoniPina
            {
                public const float ExplosionRadius = 4f;
                public const float ExplosionDamage = 25f;
                public const float ExplosionForce = 15f;
                public const float ExplosionDelay = 3f;
                public const float MinScale = 0.7f;
                public const float MaxScale = 1.3f;
                public const float ScaleAnimationSpeed = 2f;
            }

            // -- PepperoniPineapplePepperoni (catapult) --
            public static class PepperoniPineapplePepperoni
            {
                public const float ExplosionRadius = 4f;
                public const float ExplosionDamage = 35f;
                public const float ExplosionForce = 25f;
                public const float FuseTime = 1f;
                public const float MinScale = 0.8f;
                public const float MaxScale = 1.2f;
                public const float FireTrailRadius = 1.5f;
                public const float FireTrailDuration = 4f;
                public const int FireTrailSpiceCharges = 2;
                public const float FireTrailSpiceDuration = 4f;
            }

            // -- PepperoniPineapplePineapple --
            public static class PepperoniPineapplePineapple
            {
                public const float ExtraRadiusMultiplier = 1.6f;
                public const float ExtraDamageMultiplier = 1.5f;
            }

            // -- PepperoniPineappleQueso --
            public static class PepperoniPineappleQueso
            {
                public const float ExtraFireRadiusMultiplier = 2f;
                public const float ExtraFireDurationMultiplier = 1.3f;
                public const float BurnDurationIfIgnited = 5f;
                public const int BurnStacksIfIgnited = 3;
                public const float BurnDurationDefault = 2f;
                public const int BurnStacksDefault = 1;
            }

            // -- PepperoniQuesoPepperoni (sticky burn) --
            public static class PepperoniQuesoPepperoni
            {
                public const float StickDuration = 2f;
                public const int BurnStacksOnStick = 4;
                public const float BurnDuration = 4f;
                public const float TickInterval = 0.5f;
                public const float DamageScaleMax = 1f;
            }

            // -- PepperoniQuesoPineapple (sticky + children) --
            public static class PepperoniQuesoPineapple
            {
                public const float StickDuration = 1f;
                public const int BurnStacksOnStick = 3;
                public const float BurnDuration = 4f;
                public const float TickInterval = 0.25f;
                public const float DamageScaleMax = 1f;
                public const int ChildCount = 3;
                public const float ChildDamageMultiplier = 0.6f;
                public const float ChildSpawnRadius = 0.5f;
                public const float ChildSpreadAngle = 60f;
                public const float ChildStickDuration = 1f;
                public const int ChildBurnStacks = 1;
                public const float ChildBurnDuration = 3f;
                public const float ChildTickInterval = 0.25f;
            }

            // -- PepperoniQuesoQueso (long sticky) --
            public static class PepperoniQuesoQueso
            {
                public const float StickDuration = 4f;
                public const int BurnStacksOnStick = 1;
                public const float BurnDuration = 4f;
                public const float TickInterval = 0.25f;
                public const float DamageScaleMax = 1f;
            }

            // -- SmallStickyProjectile --
            public static class SmallSticky
            {
                public const float StickDuration = 1f;
                public const float TickInterval = 0.25f;
                public const int BurnStacks = 1;
                public const float BurnDuration = 3f;
            }

            // -- PineappleCheese (absorbing projectile) --
            public static class PineappleCheese
            {
                public const float AbsorptionRadius = 2f;
                public const int MaxAbsorbedProjectiles = 10;
                public const float GrowthPerProjectile = 0.05f;
                public const float MaxGrowthMultiplier = 1.5f;
                public const float ImpactDamage = 15f;
                public const float DamageOverTime = 5f;
                public const float DamageTickInterval = 0.5f;
                public const float DamageDuration = 3f;
                public const float ImpactRadius = 2.5f;
                public const float BonusDamagePerAbsorbed = 2f;
                public const float BonusDotPerAbsorbed = 0.5f;
            }

            // -- PineapplePepperoni (teleport) --
            public static class PineapplePepperoni
            {
                public const float TeleportDistance = 5f;
                public const float TeleportDelay = 0.2f;
                public const float PlayerRadius = 0.5f;
                public const float ExplosionRadius = 3f;
                public const float ExplosionDamage = 20f;
                public const float ExplosionForce = 10f;
                public const float ExplosionDelay = 0.1f;
            }

            // -- piña|piña (splitter) --
            public static class PinaPina
            {
                public const float SmallProjectileSpeed = 7f;
                public const float SmallProjectileDamageMultiplier = 0.5f;
                public const float SmallProjectileLifetime = 1f;
                public const float SpawnOffset = 0.5f;
            }

            // -- Shields --
            public static class Shields
            {
                // CheeseShield (dynamic, orbiting)
                public const float KnockbackForce = 5f;
                public const float ReflectionSpeedMultiplier = 1.5f;
                public const float ShieldDuration = 3f;
                public const float ContactDamagePerTick = 5f;
                public const float ContactTickInterval = 0.5f;
                public const float ReflectionCooldown = 0.5f;
                public const float ShieldDistance = 1.2f;

                // StaticCheeseShield (same values as dynamic except no distance)
                public const float StaticKnockbackForce = 5f;
                public const float StaticReflectionSpeed = 1.5f;
                public const float StaticDuration = 3f;
                public const float StaticContactDamage = 5f;
                public const float StaticContactInterval = 0.5f;
                public const float StaticReflectionCooldown = 0.5f;
            }

            // -- CheesePepperoniWall --
            public static class CheesePepperoniWall
            {
                public const float WallDuration = 4f;
                public const float KnockbackForce = 3f;
                public const float ReflectionSpeedMultiplier = 1.2f;
                public const float ReflectionCooldown = 0.3f;
                public const float BurnEffectDuration = 7f;
                public const int BurnInitialStacks = 2;
            }

            // ── T2 Missing combos ──────────────────────────────────────
            public static class QuesoQueso
            {
                public const float WallDuration = 6f;
                public const float WallHP = 60f;
                public const float TickDamage = 4f;
                public const float TickInterval = 0.5f;
                public const float ReflectionSpeedMultiplier = 1.2f;
                public const float ReflectionCooldown = 0.3f;
            }
            public static class QuesoPepperoni
            {
                public const float AreaDuration = 5f;
                public const float TickDamage = 5f;
                public const float TickInterval = 0.4f;
                public const float BurnDuration = 2f;
                public const int BurnStacks = 1;
                public const float DamageScalePerSecond = 0.5f;
            }
            public static class QuesoPina
            {
                public const float PillarDuration = 5f;
                public const float TickDamage = 6f;
                public const float TickInterval = 0.5f;
                public const float Radius = 1.5f;
            }
            public static class PepperoniQueso
            {
                public const float StickDuration = 2f;
                public const float BaseDamage = 8f;
                public const float DamagePerSecondStuck = 5f;
                public const float BurnDuration = 2f;
                public const int BurnStacks = 1;
            }

            // ── T3 piña|piña variants ──────────────────────────────────
            public static class PinaPinaPina
            {
                public const float SubExplosionRadius = 0.8f;
                public const float SubExplosionDamage = 8f;
            }
            public static class PinaPinaQueso
            {
                public const float SubRadius = 0.5f;
                public const float SubAbsorbRadius = 1.2f;
            }
            public static class PinaPinaPepperoni
            {
                public const float BurnDuration = 2f;
                public const int BurnStacks = 2;
            }

            // ── T3 piña|queso absorbing variants ──────────────────────
            public static class PinaQuesoPina
            {
                public const float DamagePerAbsorbed = 12f;
            }
            public static class PinaQuesoQueso
            {
                public const float ConeRadius = 2.5f;
                public const float ConeHalfAngle = 45f;
                public const float ConeDamage = 15f;
            }
            public static class PinaQuesoPepperoni
            {
                public const float BurnDurationPerStack = 1.5f;
            }

            // ── T3 piña|pepperoni teleport variants ───────────────────
            public static class PinaPepperoniPina
            {
                public const float ExplosionRadius = 2f;
                public const float ExplosionDamage = 20f;
            }
            public static class PinaPepperoniPepperoni
            {
                public const float ExplosionRadius = 3f;
                public const float ExplosionDamage = 30f;
            }
            public static class PinaPepperoniQueso
            {
                public const float ExplosionRadius = 2f;
                public const float ExplosionDamage = 20f;
                public const float ReflectionSpeedMultiplier = 1.2f;
            }

            // ── T3 queso variants — pillars ───────────────────────────
            public static class QuesoPinaPina
            {
                public const float Radius = 2.5f;
                public const float TickDamage = 10f;
                public const float TickInterval = 0.4f;
                public const float Duration = 5f;
            }
            public static class QuesoPinaQueso
            {
                public const float Radius = 1.5f;
                public const float TickDamage = 6f;
                public const float TickInterval = 0.5f;
                public const float Duration = 7f;
                public const float HP = 100f;
                public const float ProjectileSlowMultiplier = 0.5f;
            }
            public static class QuesoPinaPepperoni
            {
                public const float Radius = 1.5f;
                public const float TickDamage = 5f;
                public const float TickInterval = 0.4f;
                public const float Duration = 5f;
                public const float BurnDuration = 2f;
                public const int BurnStacksPerTick = 1;
                public const float BonusDamagePerStack = 2f;
            }

            // ── T3 queso variants — ground areas ──────────────────────
            public static class QuesoPepperoniPepperoni
            {
                public const float AreaDuration = 5f;
                public const float TickDamage = 5f;
                public const float TickInterval = 0.4f;
                public const int BurnStacks = 2;
                public const float BurnDuration = 2f;
            }
            public static class QuesoPepperoniPina
            {
                public const float AreaDuration = 5f;
                public const float TickDamage = 5f;
                public const float TickInterval = 0.4f;
                public const float BurnDuration = 2f;
                public const int BurnStacks = 1;
                public const float DoTDamage = 3f;
                public const float DoTInterval = 0.5f;
            }
            public static class QuesoPepperoniQueso
            {
                public const float AreaDuration = 5f;
                public const float Radius = 3f;
                public const float TickDamage = 5f;
                public const float TickInterval = 0.4f;
                public const float BurnDuration = 2f;
                public const int BurnStacks = 1;
            }

            // ── T3 queso|queso|queso black hole ───────────────────────
            public static class QuesoQuesoQueso
            {
                public const float AbsorbDuration = 4f;
                public const float AbsorbRadius = 3f;
                public const float ReturnDamageMultiplier = 1.5f;
                public const float ReturnProjectileSpeed = 10f;
            }
        }

        // ──────────────────────────────────────────────
        //  FIRE TRAIL (standalone)
        // ──────────────────────────────────────────────
        public static class FireTrail
        {
            public const int SpiceChargesPerTick = 2;
            public const float DamageInterval = 0.3f;
            public const float Duration = 3f;
            public const float Radius = 1.5f;
            public const float SpiceEffectDuration = 5f;
        }

        // ──────────────────────────────────────────────
        //  CASTING (distances used by PlayerAimAndCast)
        // ──────────────────────────────────────────────
        public static class Casting
        {
            public const float StaticShieldDistance = 2f;
            public const float WallDistance = 2.5f;
            public const float CatapultMinDistance = 2f;
            public const float QuesoQuesoWallDistance = 2.5f;
            public const float QuesoPepperoniAreaDistance = 2f;
            public const float QuesoPinaPillarDistance = 2f;
            public const float QuesoQuesoQuesoDistance = 2f;
        }
    }
}
