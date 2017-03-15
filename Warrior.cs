using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;

public class Warrior : CustomClass
{
    public override void Dispose() { }
    public override bool Load() { return true;  }
    public override bool OnBuff() { return true; }
    
    public override void OnFight()
    {
        if(Local.Rage >= 10 && Target.DistanceToPlayer <= 5 && !Target.GotDebuff("Rend") && Spell.Instance.IsSpellReady("Rend"))
        {
            Spell.Instance.Cast("Rend");
        }
        else if(Local.Rage >= 10 && !Local.GotAura("Battle Shout") && Spell.Instance.IsSpellReady("Battle Shout"))
        {
            Spell.Instance.Cast("Battle Shout");
        }
        // TODO: Range?
        else if(Local.Rage >= 15 && Target.DistanceToPlayer <= 5 && Spell.Instance.IsSpellReady("Heroic Strike"))
        {
            Spell.Instance.Cast("Heroic Strike");
        }

        Spell.Instance.Attack();
    }

    public override void OnPull()
    {
        if(Target != null)
        {
            float TargetDistance = Target.DistanceToPlayer;

            if(Target.DistanceToPlayer >= 8 && Target.DistanceToPlayer < 25)
            {
                Spell.Instance.Cast("Charge");
            }
        }
    }

    public override void OnRest()
    {

    }

    public override void ShowGui() {}
    public override void Unload() {}

    public WoWUnit Target {get{return ObjectManager.Instance.Target;}}
    public LocalPlayer Local {get{return ObjectManager.Instance.Player;}}

    public override string Author {get{return "aswerw";}}
    public override string Name {get{return "SimpleWarrior";}}
    public override int Version {get{return 1;}}
    public override Enums.ClassId Class {get {return Enums.ClassId.Warrior;}}
    public override bool SuppressBotMovement {get{return false;}}
    public override float CombatDistance {get{return 5.0f;}}
}
