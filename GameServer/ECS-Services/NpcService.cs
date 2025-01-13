﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DOL.AI;
using DOL.AI.Brain;
using ECS.Debug;
using log4net;

namespace DOL.GS
{
    public static class NpcService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string SERVICE_NAME = nameof(NpcService);

        private static int _nonNullBrainCount;
        private static int _nullBrainCount;

        public static int DebugTickCount { get; set; } // Will print active brain count/array size info for debug purposes if superior to 0.
        private static bool Debug => DebugTickCount > 0;
        private static List<ABrain> _list;

        public static void Tick()
        {
            GameLoop.CurrentServiceTick = SERVICE_NAME;
            Diagnostics.StartPerfCounter(SERVICE_NAME);

            if (Debug)
            {
                _nonNullBrainCount = 0;
                _nullBrainCount = 0;
            }

            _list = EntityManager.UpdateAndGetAll<ABrain>(EntityManager.EntityType.Brain, out int lastValidIndex);
            Parallel.For(0, lastValidIndex + 1, TickInternal);

            if (Debug)
            {
                log.Debug($"==== Non-null NCs in EntityManager array: {_nonNullBrainCount} | Null NPCs: {_nullBrainCount} | Total size: {_list.Count} ====");
                DebugTickCount--;
            }

            Diagnostics.StopPerfCounter(SERVICE_NAME);
        }

        private static void TickInternal(int index)
        {
            ABrain brain = _list[index];

            if (brain?.EntityManagerId.IsSet != true)
            {
                if (Debug)
                    Interlocked.Increment(ref _nullBrainCount);

                return;
            }

            if (Debug)
                Interlocked.Increment(ref _nonNullBrainCount);

            try
            {
                GameNPC npc = brain.Body;

                if (ServiceUtils.ShouldTickAdjust(ref brain.NextThinkTick))
                {
                    if (!brain.IsActive)
                    {
                        brain.Stop();
                        return;
                    }

                    long startTick = GameLoop.GetCurrentTime();
                    brain.Think();
                    long stopTick = GameLoop.GetCurrentTime();

                    if (stopTick - startTick > 25)
                        log.Warn($"Long {SERVICE_NAME}.{nameof(Tick)} for {npc.Name}({npc.ObjectID}) Interval: {brain.ThinkInterval} BrainType: {brain.GetType()} Time: {stopTick - startTick}ms");

                    brain.NextThinkTick += brain.ThinkInterval;
                }

                npc.movementComponent.Tick();
            }
            catch (Exception e)
            {
                ServiceUtils.HandleServiceException(e, SERVICE_NAME, brain, brain.Body);
            }
        }
    }
}
