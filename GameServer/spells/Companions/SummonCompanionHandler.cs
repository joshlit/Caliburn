/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections.Generic;
using System.Text;
using DOL.GS.Effects;
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.PropertyCalc;
using System.Collections;
using System.Reflection;
using DOL.Language;

namespace DOL.GS.Spells
{
    
    [SpellHandler("SummonCompanion")]
    public class SummonCompanionHandler : SummonSpellHandler
    {
	    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public SummonCompanionHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
        {
            
        }

        protected Companion m_companion = null;

        public Companion Companion
        {
            get => m_companion;
            protected set => m_companion = value;
        }   
        
        public override void CreateECSEffect(ECSGameEffectInitParams initParams)
        {
            new CompanionECSGameEffect(initParams);
        }
        
        public override void FinishSpellCast(GameLiving target)
        {
            foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
            {
                if (player != m_caster)
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObject.Casting.CastsASpell", m_caster.GetName(0, true)), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }

            m_caster.Mana -= PowerCost(target);

            base.FinishSpellCast(target);

            if (m_companion == null)
                return;

            if (Spell.Message1 == string.Empty)
            {
                MessageToCaster(string.Format("The {0} is now under your control.", m_companion.Name), eChatType.CT_Spell);
            }
            else
                MessageToCaster(Spell.Message1, eChatType.CT_Spell);
        }
        
        protected virtual void GetCompanionLocation(out int x, out int y, out int z, out ushort heading, out Region region)
        {
            Point2D point = Caster.GetPointFromHeading( Caster.Heading, 64 );
            x = point.X;
            y = point.Y;
            z = Caster.Z;
            heading = (ushort)((Caster.Heading + 2048) % 4096);
            region = Caster.CurrentRegion;
        }

        protected virtual Companion GetGamePet(INpcTemplate template)
        {
            return new Companion(template);
        }

        protected new IControlledBrain GetPetBrain(GameLiving owner)
        {
            return new CrystalBrain(owner);
        }
        
        public override void ApplyEffectOnTarget(GameLiving target)
		{
			INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);

			if (template == null)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
				MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
				return;
			}

			IControlledBrain brain = null;

			if (template.ClassType != null && template.ClassType.Length > 0)
			{
				Assembly asm = Assembly.GetExecutingAssembly();
				brain = (IControlledBrain)asm.CreateInstance(template.ClassType, true);

				if (brain == null && log.IsWarnEnabled)
					log.Warn($"ApplyEffectOnTarget(): ClassType {template.ClassType} on NPCTemplateID {template.TemplateId} not found, using default ControlledBrain");
			}
			
			
			m_companion = GetGamePet(template);
			
			if (brain == null)
				brain = new CrystalBrain(m_companion);
			m_companion.SetOwnBrain(brain as AI.ABrain);
			m_companion.SummonSpellDamage = Spell.Damage;
			m_companion.SummonSpellValue = Spell.Value;

			int x, y, z;
			ushort heading;
			Region region;

			GetCompanionLocation(out x, out y, out z, out heading, out region);

			m_companion.X = x;
			m_companion.Y = y;
			m_companion.Z = z;
			m_companion.Heading = heading;
			m_companion.CurrentRegion = region;
			m_companion.CurrentSpeed = 0;
			m_companion.Realm = Caster.Realm;

			m_companion.AddToWorld();
			
			// Check for buffs
			if (brain is ControlledNpcBrain)
				(brain as ControlledNpcBrain).CheckSpells(StandardMobBrain.eCheckSpellType.Defensive);

			//add to player companion list
			if (Caster is GamePlayer p)
			{
				p.Companions.Add(m_companion);
				if (p.Group != null)
				{
					p.Group.AddMember(m_companion);
				}
				else
				{
					
					Group group = new Group(p);
					GroupMgr.AddGroup(group);
					group.AddMember(p);
					group.AddMember(m_companion);
				}
			}

			m_companion.SetPetLevel();
			m_companion.Health = m_companion.MaxHealth;
			m_companion.Spells = template.Spells; // Have to sort spells again now that the pet level has been assigned.
			
			if (m_companion.Brain is CrystalBrain cb)
			{
				cb.Owner = Caster;
				cb.DecisionMaker.SetPlayerOwner(Caster as GamePlayer);
			}

			CreateECSEffect(new ECSGameEffectInitParams(m_companion, CalculateEffectDuration(target, Effectiveness), Effectiveness, this));
		}

		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}
		
		public override IList<string> DelveInfo
		{
			get
			{
				var list = new List<string>();

				// TODO: Fix no spellType
				//list.Add("Function: " + (Spell.SpellType == "" ? "(not implemented)" : Spell.SpellType));
				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				if (Spell.InstrumentRequirement != 0)
					list.Add("Instrument require: " + GlobalConstants.InstrumentTypeToName(Spell.InstrumentRequirement));
				list.Add("Target: " + Spell.Target);
				if (Spell.Range != 0)
					list.Add("Range: " + Spell.Range);
				if (Spell.Duration >= ushort.MaxValue * 1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration / 60000, (Spell.Duration % 60000 / 1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration / 1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency * 0.001).ToString("0.0"));
				if (Spell.Power != 0)
					list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				list.Add("Casting time: " + (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay / 1000).ToString() + " sec");
				if (Spell.Concentration != 0)
					list.Add("Concentration cost: " + Spell.Concentration);
				if (Spell.Radius != 0)
					list.Add("Radius: " + Spell.Radius);
				if (Spell.DamageType != eDamageType.Natural)
					list.Add("Damage: " + GlobalConstants.DamageTypeToName(Spell.DamageType));

				return list;
			}
		}
    }
}