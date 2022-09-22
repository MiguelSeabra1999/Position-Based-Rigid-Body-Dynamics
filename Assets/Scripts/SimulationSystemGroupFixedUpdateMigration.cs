/*using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

/// <summary>
/// Migrates the premade (default) SimulationSystemGroup containing all
/// automatically added systems from the Update loop to the FixedUpdate loop.
/// This happens during world bootstrapping.
/// </summary>
///
///

public struct SimulationSystemGroupFixedUpdateMigration : ICustomBootstrap
{
    public bool Initialize(string defaultWorldName)
    {
        Debug.Log("INITIALIZING");
        // Initalize the world normally
        var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

        var world = new World(defaultWorldName);
        World.DefaultGameObjectInjectionWorld = world;

        DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
        //ScriptBehaviourUpdateOrder.UpdatePlayerLoop(world);
        //ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);

        // Moving SimulationSystemGroup to FixedUpdate is done in two parts.
        // The PlayerLoopSystem of type SimulationSystemGroup has to be found,
        // stored, and removed before adding it to the FixedUpdate PlayerLoopSystem.

        PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        ScriptBehaviourUpdateOrder.AppendWorldToPlayerLoop(world, ref  playerLoop);

        string log = DeepDive(ref playerLoop);

        // simulationSystem has to be constructed or compiler will complain due to
        //    using non-assigned variables.
        PlayerLoopSystem simulationSystem = new PlayerLoopSystem();
        bool simSysFound = false;
        Debug.Log("subsystemlen " + playerLoop.subSystemList.Length);

        // Find the location of the SimulationSystemGroup under the Update Loop
        for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
        {
            int subsystemListLength = 0;
            if (playerLoop.subSystemList[i].subSystemList != null)
                subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;

            // Find Update loop...
            if (playerLoop.subSystemList[i].type == typeof(Update))
            {
                Debug.Log("found update");
                // Pop out SimulationSystemGroup and store it temporarily
                var newSubsystemList = new PlayerLoopSystem[subsystemListLength - 1];
                int k = 0;
                if (subsystemListLength == 0)
                {
                    Debug.Log("here");
                    if (playerLoop.subSystemList[i].type == typeof(SimulationSystemGroup))
                    {
                        Debug.Log("found sim");
                        simulationSystem = playerLoop.subSystemList[i];
                        simSysFound = true;
                    }
                }
                else
                    for (var j = 0; j < subsystemListLength; ++j)
                    {
                        if (playerLoop.subSystemList[i].subSystemList[j].type == typeof(SimulationSystemGroup))
                        {
                            simulationSystem = playerLoop.subSystemList[i].subSystemList[j];
                            simSysFound = true;
                        }
                        else
                        {
                            newSubsystemList[k] = playerLoop.subSystemList[i].subSystemList[j];
                            k++;
                        }
                    }
                playerLoop.subSystemList[i].subSystemList = newSubsystemList;
            }
        }

        string finalLog = DeepDive(ref playerLoop);
        if (log == finalLog)
            Debug.Log("nothing changed");

        // This should never happen if SimulationSystemGroup was created like usual
        // (or at least I think it might not happen :P )
        if (!simSysFound)
            throw new System.Exception("SimulationSystemGroup was not found!");

        // Round 2: find FixedUpdate...
        for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
        {
            int subsystemListLength = 0;
            if (playerLoop.subSystemList[i].subSystemList != null)
                subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;

            // Found FixedUpdate
            if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
            {
                Debug.Log("found fixed update");
                // Allocate new space for stored SimulationSystemGroup
                //    PlayerLoopSystem, and place simulation group at index defined by
                //    temporary variable.
                var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                int k = 0;

                int indexToPlaceSimulationSystemGroupIn = subsystemListLength;

                if (subsystemListLength == 0)
                {
                    Debug.Log("appending new thing");
                    if (0 == indexToPlaceSimulationSystemGroupIn)
                        newSubsystemList[0] = simulationSystem;
                    else
                    {
                        newSubsystemList[0] = playerLoop.subSystemList[i];
                    }
                }
                for (var j = 0; j < subsystemListLength + 1; ++j)
                {
                    if (j == indexToPlaceSimulationSystemGroupIn)
                        newSubsystemList[j] = simulationSystem;
                    else
                    {
                        newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[k];
                        k++;
                    }
                }
                playerLoop.subSystemList[i].subSystemList = newSubsystemList;
            }
        }

        // Set the beautiful, new player loop
        //  ScriptBehaviourUpdateOrder.SetPlayerLoop(playerLoop);

        PlayerLoop.SetPlayerLoop(playerLoop);

        //PlayerLoopSystem newLoop = ScriptBehaviourUpdateOrder.CurrentPlayerLoop;
        playerLoop = PlayerLoop.GetCurrentPlayerLoop();
        string lastLog = DeepDive(ref playerLoop);
        if (lastLog == finalLog)
            Debug.Log("nothing changed");


        //REMOVE ANY SYSTEMS FROM FIXED STEP SIMULATION
        var buffer = new PlayerLoopSystem[0];

        for (int i = 0; i < playerLoop.subSystemList.Length; i++)
        {
            if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
                for (int j = 0; j < playerLoop.subSystemList[i].subSystemList.Length; j++)
                {
                    if (playerLoop.subSystemList[i].subSystemList[j].type == typeof(SimulationSystemGroup))
                    {
                        for (int l = 0; l < playerLoop.subSystemList[i].subSystemList[j].subSystemList.Length; l++)
                        {
                            if (playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].type == typeof(FixedStepSimulationSystemGroup))
                            {
                                Debug.Log("found fixed step");
                                buffer = new PlayerLoopSystem[playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList.Length];
                                for (int k = 0; k < playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList.Length; k++)
                                {
                                    buffer[k] = playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList[k];
                                }
                                playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList = new PlayerLoopSystem[0];
                                break;
                            }
                        }
                    }
                }
        }

           //ADD  SYSTEMS TO VARIABLE STEP SIMULATION
           for (int i = 0; i < playerLoop.subSystemList.Length; i++)
           {
               if (playerLoop.subSystemList[i].type == typeof(FixedUpdate))
                   for (int j = 0; j < playerLoop.subSystemList[i].subSystemList.Length; j++)
                   {
                       if (playerLoop.subSystemList[i].subSystemList[j].type == typeof(SimulationSystemGroup))
                       {
                           for (int l = 0; l < playerLoop.subSystemList[i].subSystemList[j].subSystemList.Length; l++)
                           {
                               if (playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].type == typeof(VariableRateSimulationSystemGroup))
                               {
                                   var buffer1 = new PlayerLoopSystem[simulationSystem.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList.Length];
                                   for (int k = 0; k < buffer1.Length; k++)
                                   {
                                       buffer1[k] = playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList[k];
                                   }
                                   playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList = new PlayerLoopSystem[buffer1.Length + buffer.Length];
                                   for (int k = 0; k < playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList.Length; k++)
                                   {
                                       if (k < buffer1.Length)
                                           playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList[k] = buffer1[k];
                                       else
                                           playerLoop.subSystemList[i].subSystemList[j].subSystemList[l].subSystemList[k]  = buffer[k - buffer1.Length];
                                   }
                               }
                           }
                       }
                   }
           }
        PlayerLoop.SetPlayerLoop(playerLoop);

        return true;
    }

    string DeepDive(ref PlayerLoopSystem sys)
    {
        string log = "";
        for (var i = 0; i < sys.subSystemList.Length; ++i)
        {
            if (sys.subSystemList[i].subSystemList == null)
            {
                try
                {
                    //  Debug.Log(sys.subSystemList[i].type);
                    log += sys.subSystemList[i].type;
                    DeepDive(ref sys.subSystemList[i]);
                }
                catch {}
            }
            else
            {
                for (var j = 0; j < sys.subSystemList[i].subSystemList.Length; ++j)
                {
                    try
                    {
                        //Debug.Log(sys.subSystemList[i].subSystemList[j].type);
                        log += sys.subSystemList[i].subSystemList[j].type;
                        DeepDive(ref sys.subSystemList[i].subSystemList[j]);
                    }
                    catch {}
                }
            }
        }
        return log;
    }
}
*/
