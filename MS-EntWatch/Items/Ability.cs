using Sharp.Shared.GameEntities;

namespace MS_EntWatch.Items
{
    public class Ability
    {
        public string Name { get; set; }
        public string ButtonClass { get; set; } //func_button, func_door, game_ui, func_physbox
        public bool Chat_Uses { get; set; }
        public int Mode { get; set; }
        public int MaxUses { get; set; }
        public int CoolDown { get; set; }
        public string ButtonID { get; set; }
        public bool Ignore { get; set; }
        public bool LockItem { get; set; }
        public string MathID { get; set; }
        public bool MathNameFix { get; set; }
        public bool MathFindSpawned { get; set; }
        public bool MathDontShowMax { get; set; }
        public bool MathZero { get; set; }
        public string Filter { get; set; } // <activatorname> or <Context:1> or <$attribute>

        public IBaseEntity? Entity;
        public IMathCounter? MathCounter;
        public double fLastUse;
        public int iCurrentUses;

        public Ability()
        {
            Name = "";
            ButtonClass = "";
            Chat_Uses = true;
            Mode = 0;
            MaxUses = 0;
            CoolDown = 0;
            ButtonID = "";
            Ignore = false;
            LockItem = false;
            MathID = "";
            MathNameFix = false;
            MathFindSpawned = false;
            MathDontShowMax = false;
            MathZero = false;
            Filter = "";

            Entity = null;
            MathCounter = null;
            fLastUse = 0.0;
            iCurrentUses = 0;
        }

        public Ability(string name, string buttonclass, bool chat_uses, int mode, int maxuses, int cooldown, string buttonid, IBaseEntity? entity = null)
        {
            Name = name;
            ButtonClass = buttonclass;
            Chat_Uses = chat_uses;
            Mode = mode;
            MaxUses = maxuses;
            CoolDown = cooldown;
            ButtonID = buttonid;
            Ignore = false;
            LockItem = false;
            MathID = "";
            MathNameFix = false;
            MathFindSpawned = false;
            MathDontShowMax = false;
            MathZero = false;
            Filter = "";

            Entity = entity;
            MathCounter = null;

            EW.UpdateTime();
            fLastUse = EW.fGameTime - CoolDown;
            iCurrentUses = 0;
        }

        public Ability(Ability cCopyAbility)
        {
            Name = cCopyAbility.Name;
            ButtonClass = cCopyAbility.ButtonClass;
            Chat_Uses = cCopyAbility.Chat_Uses;
            Mode = cCopyAbility.Mode;
            MaxUses = cCopyAbility.MaxUses;
            CoolDown = cCopyAbility.CoolDown;
            ButtonID = cCopyAbility.ButtonID;
            Ignore = cCopyAbility.Ignore;
            LockItem = cCopyAbility.LockItem;
            MathID = cCopyAbility.MathID;
            MathNameFix = cCopyAbility.MathNameFix;
            MathFindSpawned = cCopyAbility.MathFindSpawned;
            MathDontShowMax = cCopyAbility.MathDontShowMax;
            MathZero = cCopyAbility.MathZero;
            Filter = cCopyAbility.Filter;

            Entity = null;
            MathCounter = null;
            EW.UpdateTime();
            fLastUse = EW.fGameTime - CoolDown;
            iCurrentUses = 0;
            SetSpawnedMath();
        }

        public void SetFilter(IBaseEntity activator)
        {
            if (!string.IsNullOrEmpty(Filter))
            {
                if (activator.AsPlayerPawn() is { } pawn)
                {

                    if (Filter[0] == '$')
                    {
                        if (Filter.Length > 1) pawn.AcceptInput("AddAttribute", null, null, Filter[1..]);
                    }
                    else if (Filter.Contains(':'))
                    {
                        pawn.AcceptInput("AddContext", null, null, Filter);
                    }
                    else
                    {
                        pawn.SetName(Filter);
                    }
                }
            }
        }

        public void SetSpawnedMath()
        {
            if ((Mode == 6 || Mode == 7) && MathFindSpawned && !string.IsNullOrEmpty(MathID) && !string.Equals(MathID, "0"))
            {
                foreach (var entMath in EntWatch._entities!.GetAllEntitiesByClassname("math_counter"))
                {
                    if (entMath != null && entMath.IsValid() && string.Equals(entMath.HammerId, MathID))
                    {
                        MathCounter = entMath.As<IMathCounter>();
                        break;
                    }
                }
            }
        }

        public void Used()
        {
            switch (Mode)
            {
                case 2:
                    if (fLastUse < EW.fGameTime) fLastUse = EW.fGameTime + CoolDown;
                    break;
                case 3:
                    if (iCurrentUses < MaxUses)
                    {
                        iCurrentUses++;
                        fLastUse = EW.fGameTime + 1;
                    }
                    break;
                case 4:
                    if (iCurrentUses < MaxUses)
                    {
                        iCurrentUses++;
                        fLastUse = EW.fGameTime + CoolDown;
                    }
                    break;
                case 5:
                    iCurrentUses++;
                    fLastUse = EW.fGameTime + 1;
                    if (iCurrentUses == MaxUses)
                    {
                        fLastUse = EW.fGameTime + CoolDown;
                        iCurrentUses = 0;
                    }
                    break;
            }
        }

        public string GetMessage()
        {
            switch (Mode)
            {
                case 2:
                    if (fLastUse < EW.fGameTime) return "R";
                    else return $"{Math.Round(fLastUse - EW.fGameTime, 0)}";
                case 3:
                    if (iCurrentUses < MaxUses) return $"{iCurrentUses}/{MaxUses}";
                    else return "E";
                case 4:
                    if (fLastUse < EW.fGameTime)
                    {
                        if (iCurrentUses < MaxUses) return $"{iCurrentUses}/{MaxUses}";
                        else return "E";
                    }
                    else return $"{Math.Round(fLastUse - EW.fGameTime, 0)}";
                case 5:
                    if (fLastUse < EW.fGameTime) return $"{iCurrentUses}/{MaxUses}";
                    else return $"{Math.Round(fLastUse - EW.fGameTime, 0)}";
                case 6:
                    {
                        if (MathCounter != null && MathCounter.IsValid())
                        {
                            float fValue = MathCounter.Value;
                            if (fValue > MathCounter.MinValue) return $"{fValue:R}" + (!MathDontShowMax ? $"/{MathCounter.MaxValue:R}" : "");
                            else return "E";
                        }
                        else return "+";
                    }
                case 7:
                    {
                        if (MathCounter != null && MathCounter.IsValid())
                        {
                            float fValue = MathCounter.MaxValue - MathCounter.Value;
                            if (fValue < MathCounter.MaxValue) return $"{fValue:R}" + (!MathDontShowMax ? $"/{MathCounter.MaxValue:R}" : "");
                            else return "E";
                        }
                        else return "+";
                    }
                case 8:
                    {
                        if (Entity != null && Entity.IsValid()) return $"{Entity.Health} HP";
                        else return "+";
                    }

                default: return "+";
            }
        }

        public bool Ready()
        {
            if (LockItem) return false;
            if (fLastUse >= EW.fGameTime) return false;
            switch (Mode)
            {
                case 2: return true;
                case 3:
                    if (iCurrentUses < MaxUses) return true;
                    else return false;
                case 4:
                    if (iCurrentUses < MaxUses) return true;
                    else return false;
                case 5: return true;
                case 6:
                    if (MathCounter != null && MathCounter.IsValid() && (MathZero ? MathCounter.Value >= MathCounter.MinValue : MathCounter.Value > MathCounter.MinValue)) return true;
                    else return false;
                case 7:
                    if (MathCounter != null && MathCounter.IsValid() && (MathZero ? (MathCounter.MaxValue - MathCounter.Value) <= MathCounter.MaxValue : (MathCounter.MaxValue - MathCounter.Value) < MathCounter.MaxValue)) return true;
                    else return false;
                case 8: return false;
                default: return true;
            }
        }
    }
}
