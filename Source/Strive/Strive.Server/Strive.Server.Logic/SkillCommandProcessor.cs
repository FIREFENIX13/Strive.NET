using System;
using System.Collections.Generic;
using Common.Logging;
using Strive.Common;
using Strive.Network.Messages.ToServer;
using Strive.Network.Messaging;
using Strive.Model;


namespace Strive.Server.Logic
{
    public class SkillCommandProcessor
    {
        static ILog Log = LogManager.GetCurrentClassLogger();

        public static void ProcessUseSkill(World world, ClientConnection client, UseSkill message)
        {
            var avatar = client.Avatar as Avatar;
            if (avatar == null)
            {
                client.LogMessage("Requested a skill, but doesn't have an avatar.");
                return;
            }
            Schema.EnumSkillRow esr = Global.ModelSchema.EnumSkill.FindByEnumSkillID(message.SkillId);
            if (esr == null)
            {
                client.LogMessage("Requested an invalid skill " + message.SkillId);
                return;
            }

            // If already performing a skill invocation, just queue the request for later.
            if (avatar.ActivatingSkill != null)
            {
                avatar.SkillQueue.Enqueue(message);
                return;
            }

            if (esr.LeadTime <= 0)
            {
                // process it now
                UseSkillNow(world, avatar, message);
            }
            else
            {
                // process it later, after lead-time has elapsed
                avatar.ActivatingSkill = message;
                avatar.ActivatingSkillTimestamp = Global.Now;
                avatar.ActivatingSkillLeadTime = TimeSpan.FromSeconds(esr.LeadTime);
            }
        }

        public static void ProcessCancelSkill(ClientConnection client, CancelSkill message)
        {
            var avatar = client.Avatar as Avatar;
            if (avatar == null)
            {
                client.LogMessage("Canceled a skill invocation, but don't have an avatar.");
                return;
            }

            // If already performing invocation, just cancel it
            bool found = false;
            if (avatar.ActivatingSkill != null && avatar.ActivatingSkill.InvokationId == message.InvokationId)
            {
                avatar.ActivatingSkill = null;
                found = true;
            }
            else
            {
                // search for it in queued skill invocations
                // just generate a new queue with the invocation missing
                var newQueue = new Queue<UseSkill>();
                foreach (UseSkill m in avatar.SkillQueue)
                {
                    if (m.InvokationId == message.InvokationId)
                    {
                        // don't add it
                        found = true;
                    }
                    else
                        newQueue.Enqueue(m);
                }
                avatar.SkillQueue = newQueue;
            }
            if (found)
                client.LogMessage("Successfully canceled invocation " + message.InvokationId);
            else
                client.LogMessage("Failed to cancel invocation " + message.InvokationId);
        }

        public static void UseSkillNow(World world, Avatar caster, UseSkill message)
        {
            Schema.EnumSkillRow esr = Global.ModelSchema.EnumSkill.FindByEnumSkillID(message.SkillId);
            if (esr == null)
            {
                caster.SendLog("Requested an invalid skill " + message.SkillId);
                return;
            }

            if (esr.EnergyCost > caster.Energy)
            {
                caster.SendLog("Not enough energy to use " + esr.EnumSkillName + ", require " + esr.EnergyCost + ".");
                return;
            }

            // TODO: make this work
            /*
            if ( esr.EnumMobileState > caster.MobileState ) {
                caster.SendLog( "Not while " + caster.MobileState.Name );
            }
            */

            Avatar target;
            switch ((EnumTargetType)esr.EnumTargetTypeID)
            {
                case EnumTargetType.TargetSelf:
                    target = caster;
                    break;
                case EnumTargetType.TargetMobile:
                    if (message.TargetPhysicalObjectIDs.Length == 0)
                    {
                        caster.SendLog("No target specified, this skill may only be used on Mobiles.");
                        return;
                    }
                    target = (Avatar)world.PhysicalObjects[message.TargetPhysicalObjectIDs[0]];
                    if (target == null)
                    {
                        caster.SendLog("Target " + message.TargetPhysicalObjectIDs[0] + " not found.");
                        return;
                    }
                    if ((caster.Position - target.Position).Length > esr.Range)
                    {
                        // target is out of range
                        caster.SendLog(target.TemplateObjectName + " is out of range");
                        return;
                    }
                    break;
                default:
                    caster.SendLog("That skill does not work yet, contact admin.");
                    Log.Error("Unhandled target type " + esr.EnumTargetTypeID + " for skill " + esr.EnumSkillID);
                    return;
            }

            if (TargetSkill(caster, target, esr))
            {
                // successful casting affects affinity with the elements
                caster.AffinityAir += esr.AirAffinity / 1000f;
                caster.AffinityEarth += esr.EarthAffinity / 1000f;
                caster.AffinityFire += esr.FireAffinity / 1000f;
                caster.AffinityLife += esr.LifeAffinity / 1000f;
                caster.AffinityWater += esr.WaterAffinity / 1000f;
            }

            // deduct energy regardless
            caster.Energy -= esr.EnergyCost;
        }

        public static bool TargetSkill(Avatar caster, Avatar target, Schema.EnumSkillRow esr)
        {
            // test adeptness
            float competancy = caster.GetCompetancy((EnumSkill)esr.EnumSkillID);
            double roll = Global.Rand.NextDouble();
            if (competancy < roll * 100)
            {
                // failed
                caster.SendLog("You failed " + esr.EnumSkillName + ".");
                return false;
            }

            switch ((EnumActivationType)esr.EnumActivationTypeID)
            {
                case EnumActivationType.AttackSpell:
                    float damage = esr.EnergyCost;
                    damage += esr.AirAffinity * caster.AffinityAir / target.AffinityAir;
                    damage += esr.EarthAffinity * caster.AffinityEarth / target.AffinityEarth;
                    damage += esr.FireAffinity * caster.AffinityFire / target.AffinityFire;
                    damage += esr.LifeAffinity * caster.AffinityLife / target.AffinityLife;
                    damage += esr.WaterAffinity * caster.AffinityWater / target.AffinityWater;
                    caster.MagicalAttack(target, damage);
                    Log.Info("Attack spell cast!");
                    break;
                case EnumActivationType.Enchantment:
                    break;
                case EnumActivationType.Glamour:
                    break;
                case EnumActivationType.HealingSpell:
                    break;
                case EnumActivationType.Skill:
                    if (esr.EnumSkillID == (int)EnumSkill.Kill)
                        DoKill(caster, target);
                    else if (esr.EnumSkillID == (int)EnumSkill.Kick)
                        DoKick(caster, target);
                    else
                        Log.Warn("Unhandled SkillID " + esr.EnumSkillID);
                    break;
                case EnumActivationType.Sorcery:
                    break;
                default:
                    caster.SendLog("That skill does not work yet, contact admin.");
                    Log.Error("Unhandled activation type " + esr.EnumActivationTypeID + " for skill " + esr.EnumSkillID);
                    return false;
            }

            // if we made it this far, the skill was successful :)
            return true;
        }

        public static void DoKill(Avatar caster, EntityModel target)
        {
            caster.Attack(target);
        }

        public static void DoKick(Avatar caster, EntityModel target)
        {
            caster.Kick(target);
        }
    }
}
