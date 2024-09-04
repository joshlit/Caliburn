using DOL.AI;
using DOL.AI.Brain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DOL.GS.Scripts
{
    public class MimicGroup
    {
        private Group _group;
        public GameLiving MainLeader { get; private set; }
        public GameLiving MainAssist { get; private set; }
        public GameLiving MainTank { get; private set; }
        public GameLiving MainCC { get; private set; }
        public GameLiving MainPuller { get; private set; }
        public Point3D CampPoint { get; private set; }
        public Point2D PullFromPoint {get; private set; }

        public List<GameLiving> CCTargets = new List<GameLiving>();

        public int ConLevelFilter = -2;

        public GameObject CurrentTarget
        {
            get { return MainAssist.TargetObject; }
        }

        public MimicGroup(GameLiving leader, Group group) 
        {
            MainLeader = leader;
            MainAssist = leader;
            MainTank = leader;
            MainCC = leader;
            MainPuller = leader;

            _group = group;
        }

        public bool SetLeader(GameLiving living)
        {
            if (living == null)
                return false;

            MainLeader = living;
            living.Say("Follow me! I will now lead the group. Not really though this isn't implemented.");

            return true;
        }

        public bool SetMainAssist(GameLiving living)
        {
            if (living == null)
                return false;

            MainAssist = living;
            living.Say("Assist me! I will be the main assist. Not really though this isn't implemented.");

            return true;
        }

        public bool SetMainTank(GameLiving living)
        {
            if (living == null)
                return false;

            MainTank = living;
            living.Say("I will tank.");
            return true;
        }

        public bool SetMainCC(GameLiving living)
        {
            if (living == null)
                return false;

            MainCC = living;
            living.Say("I'll be the main CC.");

            return true;
        }

        public bool SetMainPuller(GameLiving living)
        {
            if (living == null || living.Inventory.GetItem(eInventorySlot.DistanceWeapon) == null)
                return false;

            MainPuller = living;
            living.Say("I'll be the puller.");

            return true;
        }

        public void SetCampPoint(Point3D point)
        {
            if (point != null)
                CampPoint = new Point3D(point);
            else
                CampPoint = null;
        }

        public void SetPullPoint(Point2D point)
        {
            PullFromPoint = new Point2D(point);
        }
    }
}
