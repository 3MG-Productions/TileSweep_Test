using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void Callback(System.Object arg);

public class EventController
{
    private static Dictionary<GameEvent, List<Delegate>> eventTable = new Dictionary<GameEvent, List<Delegate>>();

    public static void StartListening(GameEvent eventType, Callback handler, int priority = 5)
    {
        lock (eventTable)
        {
            if (!eventTable.ContainsKey(eventType))
            {
                eventTable.Add(eventType, new List<Delegate>());
            }

            List<Delegate> value = eventTable[eventType];

            value.Add(handler);
        }
    }

    public static void StopListening(GameEvent eventType, Callback handler)
    {
        lock (eventTable)
        {
            if (eventTable.ContainsKey(eventType))
            {
                List<Delegate> value = eventTable[eventType];
                value.Remove(handler);
            }
        }
    }

    public static void TriggerEvent(GameEvent eventType, System.Object arg = null)
    {
        if (eventTable.ContainsKey(eventType))
        {
            List<Delegate> value = eventTable[eventType];

            foreach (Delegate observer in value)
            {
                observer.DynamicInvoke(arg);
            }
        }
    }
}
