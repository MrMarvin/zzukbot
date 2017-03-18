using System.ComponentModel.Composition;
using ZzukBot.Constants;
using ZzukBot.ExtensionFramework.Classes;
using ZzukBot.Game.Statics;
using ZzukBot.Objects;
using FluentBehaviourTree;

[Export(typeof(CustomClass))]
public class SimpleWarlock : CustomClass
{
    IBehaviourTreeNode bt;

    public override void Dispose() { }
    public override bool Load() {
        var builder = new BehaviourTreeBuilder();
         bt = builder
            .Sequence("fight")
            .Do("applyDot_Immolate", t =>
            {
                if (Player.Rage >= 10 && Target.DistanceToPlayer <= 5 && !Target.GotDebuff("Rend") && Spell.Instance.IsSpellReady("Rend"))
                {
                    Spell.Instance.Cast("Immolate");
                    return BehaviourTreeStatus.Success;
                }
                return BehaviourTreeStatus.Failure;
            })
            .Do("action2", t =>
            {
                // Action 2.
                return BehaviourTreeStatus.Success;
            })
        .End()
        .Build();

        return true;
    }
    public override bool OnBuff() { return true; }

    private void DebugMsg(string String)
    {
        Lua.Instance.Execute("DEFAULT_CHAT_FRAME:AddMessage(\"SimpleWarlock DEBUG: " + String + "\");");
    }
    
    public override void OnFight()
    {
        if(Player.Rage >= 10 && Target.DistanceToPlayer <= 5 && !Target.GotDebuff("Rend") && Spell.Instance.IsSpellReady("Rend"))
        {
            Spell.Instance.Cast("Rend");
        }
        // BUG: Awaiting fix on LocalPlayer#GotAura.
        /*else if(Local.Rage >= 10 && !Local.GotAura("Battle Shout") && Spell.Instance.IsSpellReady("Battle Shout"))
        {
            Spell.Instance.Cast("Battle Shout");
        }*/
        // TODO: Range?
        else if(Local.Rage >= 15 && Target.DistanceToPlayer <= 5 && Spell.Instance.IsSpellReady("Heroic Strike"))
        {
            Spell.Instance.Cast("Heroic Strike");
        }

        Spell.Instance.Attack();
    }

    public override void OnPull()
    {
        if (Target == null)
            return;

        float TargetDistance = Target.DistanceToPlayer;

        if(Target.DistanceToPlayer >= 8 && Target.DistanceToPlayer < 25)
        {
            Spell.Instance.Cast("Charge");
        }
        
    }
    
    public override void OnRest()
    {

    }

    public override void ShowGui() {}
    public override void Unload() {}

    public WoWUnit Target {get{return ObjectManager.Instance.Target;}}
    public LocalPlayer Player {get{return ObjectManager.Instance.Player;}}

    public override string Author {get{return "schneck";}}
    public override string Name {get{return "SimpleWalock";}}
    public override int Version {get{return 42;}}
    public override Enums.ClassId Class {get {return Enums.ClassId.Warlock;}}
    public override bool SuppressBotMovement {get{return false;}}
    public override float CombatDistance {get{return 5.0f;}}
}
