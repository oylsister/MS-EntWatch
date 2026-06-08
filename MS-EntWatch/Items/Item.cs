using MS_EntWatch.Helpers;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;

namespace MS_EntWatch.Items
{
    public class Item: ItemConfig
    {
        public IBaseWeapon WeaponHandle;
        public IGameClient? Owner;
        public IBaseParticle? Particle;
        public IBaseAnimGraph? Prop;

        public double fDelay;
        public CStrikeTeam Team;

        public bool ThisItem(EntityIndex weaponindex)
        {
            if (WeaponHandle.Index == weaponindex) return true;
            return false;
        }

        public Item(ItemConfig cNewItem, IBaseWeapon weapon)
        {
            WeaponHandle = weapon;
            Name = cNewItem.Name;
            ShortName = cNewItem.ShortName;
            Color = cNewItem.Color;
            HammerID = cNewItem.HammerID;
            if (cNewItem.GlowColor.Length == 4) GlowColor = cNewItem.GlowColor;
            else GlowColor = [255, 255, 255, 255];
            BlockPickup = cNewItem.BlockPickup;
            //if (BlockPickup || Cvar.GlobalBlock) WeaponHandle.CanBePickedUp = false;
            //else WeaponHandle.CanBePickedUp = true;
            AllowTransfer = cNewItem.AllowTransfer;
            ForceDrop = cNewItem.ForceDrop;
            Chat = cNewItem.Chat;
            Hud = cNewItem.Hud;
            UsePriority = cNewItem.UsePriority;
            TriggerID = cNewItem.TriggerID;
            SpawnerID = cNewItem.SpawnerID;
            AbilityList = [];
            foreach (Ability ability in cNewItem.AbilityList.ToList())
            {
                AbilityList.Add(new Ability(ability));
            }
            Owner = null;

            if (Cvar.GlowParticle)
            {
                var kv = new Dictionary<string, KeyValuesVariantValueItem>
                {
                    {"effect_name", "particles/overhead_icon_fx/player_ping_ground_rings.vpcf"},
                    {"tint_cp", 1},
                    {"tint_cp_color", $"{GlowColor[0]} {GlowColor[1]} {GlowColor[2]} {GlowColor[3]}"},
                    {"start_active", true}
                };

                if (EntWatch._entities!.SpawnEntitySync<IBaseParticle>("info_particle_system", kv) is { } particle)
                {
                    particle.AcceptInput("FollowEntity", WeaponHandle, null, "!activator");
                    Particle = particle;
                    if (Cvar.GlowVIP)
                    {
                        EntWatch._transmits!.AddEntityHooks(Particle, true);
                    }
                }
            }

            if (Cvar.GlowSpawn && WeaponHandle.GetGlowProperty() is { } glow)
            {
                glow.GlowColorOverride = new Color32((byte)GlowColor[0], (byte)GlowColor[1], (byte)GlowColor[2], (byte)GlowColor[3]);
                glow.GlowColor = new Vector(GlowColor[0], GlowColor[1], GlowColor[2]);
                glow.GlowType = 3;
                glow.GlowRangeMax = 5000;
                glow.GlowRangeMin = 1;
                glow.GlowTeam = -1;
                glow.GlowTime = 1;
                glow.Glowing = true;
            }

            if (!Cvar.GlowSpawn) EnableGlow();

            EW.UpdateTime();
            fDelay = EW.fGameTime - Cvar.Delay;

            UI.EWSysInfo("EntWatch.Info.Item.Spawn", 8, Name, weapon.Index);

        }

        public void EnableGlow()
        {
            if (Cvar.GlowProp && WeaponHandle.GetBodyComponent().GetSceneNode()?.AsSkeletonInstance?.GetModelState().ModelName is { } model)
            {
                var kv = new Dictionary<string, KeyValuesVariantValueItem>
                {
                    {"spawnflags", 256},
                    {"glowcolor", $"{GlowColor[0]} {GlowColor[1]} {GlowColor[2]} {GlowColor[3]}"},
                    {"glowrange", 5000},
                    {"glowrangemin", 1},
                    {"glowteam", -1},
                    {"glowstate", 3},
                    {"disabled", true}
                };

                if (EntWatch._entities!.SpawnEntitySync<IBaseAnimGraph>("prop_dynamic", kv) is { } prop)
                {
                    prop.SetModel(model); //In KV not work
                    prop.AcceptInput("FollowEntity", WeaponHandle, null, "!activator");
                    Prop = prop;
                    if (Cvar.GlowVIP)
                    {
                        EntWatch._transmits!.AddEntityHooks(Prop, true);
                    }
                }
            }
        }

        public void DisableGlow()
        {
            if (Cvar.GlowProp)
            {
                if (Prop != null && Prop.IsValid())
                {
                    Prop.Kill();
                }
                Prop = null;
            }
        }

        ~Item()
        {
            AbilityList.Clear();
        }

        public bool CheckDelay()
        {
            EW.UpdateTime();
            if (fDelay < EW.fGameTime) return true;
            return false;
        }

        public void SetDelay()
        {
            EW.UpdateTime();
            fDelay = EW.fGameTime + Cvar.Delay;
        }
    }
}
