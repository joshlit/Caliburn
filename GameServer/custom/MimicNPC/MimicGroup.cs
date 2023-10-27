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
        public GameLiving MainLeader { get; private set; }
        public GameLiving MainAssist { get; private set; }
        public GameLiving MainTank { get; private set; }
        public GameLiving MainCC { get; private set; }
        public Point3D CampPoint { get; private set; }
           

        public Queue<QueueRequest> GroupQueue = new Queue<QueueRequest>();

        public GameObject CurrentTarget
        {
            get { return MainAssist.TargetObject; }
        }

        public MimicGroup(GameLiving leader) 
        {
            MainLeader = leader;
            MainAssist = leader;
            MainTank = leader;
            MainCC = leader;
        }

        public void AddToQueue(QueueRequest request)
        {
            GroupQueue.Enqueue(request);
        }

        public QueueRequest ProcessQueue(eMimicGroupRole role)
        {
            lock (GroupQueue)
            {
                return GroupQueue.FirstOrDefault(x => x.Role == role);
            }
        }

        public void RespondQueue(eQueueMessageResult result)
        {
            switch (result)
            {
            }
        }

        private void RemoveQueue(QueueRequest request)
        {
            lock(GroupQueue)
            {
            }
        }

        public void SetMainAssist(GameLiving living)
        {
            if (living == null)
                return;

            MainAssist = living;
        }

        public void SetMainTank(GameLiving living)
        {
            if (living == null)
                return;

            MainTank = living;
        }

        public void SetMainCC(GameLiving living)
        {
            if (living == null)
                return;

            MainCC = living;
        }

        public void SetCampPoint(Point3D point)
        {
            if (point == null)
                return;

            CampPoint = point;
        }

        public void RemoveCampPoint()
        {
            CampPoint = null;
        }

        public class QueueRequest
        {
            public GameLiving Requester { get; private set; }
            public eQueueMessage QueueMessage { get; private set; }
            public eMimicGroupRole Role { get; private set; }

            public QueueRequest(GameLiving requester, eQueueMessage request, eMimicGroupRole role)
            {
                Requester = requester;
                QueueMessage = request;
                Role = role;
            }
        }
    }
}
