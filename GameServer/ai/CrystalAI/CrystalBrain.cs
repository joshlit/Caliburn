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

using Crystal;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS;


public class CrystalBrain : ABrain, IControlledBrain
{
    public GameLoopDecisionMaker DecisionMaker;

    public CrystalBrain(GameLiving attachedEntity) : base()
    {
        DecisionMaker = GameLoopDecisionMaker.CreateWarrior(attachedEntity);
        DecisionMaker.Start();
    }


    public override void Think()
    {
        DecisionMaker?.Think();
    }

    public override void KillFSM()
    {
        
    }

    public eWalkState WalkState { get; }
    public eAggressionState AggressionState { get; set; }
    protected GameLiving m_owner;
    public GameLiving Owner
    {
        get { return m_owner; }
        set { m_owner = value; }
    }

    public GameLiving AttachedEntity;
    public void Attack(GameObject target)
    {
        
    }

    public void Disengage()
    {
        
    }

    public void Follow(GameObject target)
    {
        
    }

    public void FollowOwner()
    {
        
    }

    public void Stay()
    {
        
    }

    public void ComeHere()
    {
        
    }

    public void Goto(GameObject target)
    {
        
    }

    public void UpdatePetWindow()
    {
        
    }

    public GamePlayer GetPlayerOwner()
    {
        return Owner as GamePlayer;
    }

    public GameNPC GetNPCOwner()
    {
        return Owner as GameNPC;
    }

    public GameLiving GetLivingOwner()
    {
        return Owner;
    }

    public void SetAggressionState(eAggressionState state)
    {
        
    }

    public bool IsMainPet { get; set; }
}

